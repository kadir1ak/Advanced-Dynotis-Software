using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;

namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class DeviceViewModel : INotifyPropertyChanged, IDisposable
    {
        public InterfaceVariables InterfaceVariables { get; private set; }

        public ChartViewModel ChartViewModel { get; }

        public string DeviceDisplayName => $"{Device.Model} - {Device.SeriNo}";

        private Dynotis _device;
        private DynotisData _latestDynotisData;
        private readonly object _dataLock = new();
        private CancellationTokenSource _cancellationTokenSource;
        private int UpdateTimeMillisecond = 50;

        public Dynotis Device
        {
            get => _device;
            set
            {
                if (_device != value)
                {
                    _device = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Device.DynotisData)); // Notify when the device changes
                }
            }
        }

        private EquipmentParametersViewModel _currentEquipmentParameters;
        public EquipmentParametersViewModel CurrentEquipmentParameters
        {
            get => _currentEquipmentParameters;
            set
            {
                if (SetProperty(ref _currentEquipmentParameters, value))
                {
                    if (_currentEquipmentParameters != null)
                    {
                        _currentEquipmentParameters.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(EquipmentParametersViewModel.UserPropellerArea) ||
                                e.PropertyName == nameof(EquipmentParametersViewModel.UserMotorInner) ||
                                e.PropertyName == nameof(EquipmentParametersViewModel.UserNoLoadCurrents))
                            {
                                Device.DynotisData.PropellerArea = _currentEquipmentParameters.UserPropellerArea;
                                Device.DynotisData.MotorInner = _currentEquipmentParameters.UserMotorInner;
                                Device.DynotisData.NoLoadCurrents = _currentEquipmentParameters.UserNoLoadCurrents;
                                OnPropertyChanged(nameof(Device.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        private ESCParametersViewModel _currentESCParameters;
        public ESCParametersViewModel CurrentESCParameters
        {
            get => _currentESCParameters;
            set
            {
                if (SetProperty(ref _currentESCParameters, value))
                {
                    if (_currentESCParameters != null)
                    {
                        _currentESCParameters.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(ESCParametersViewModel.ESCValue) ||
                                e.PropertyName == nameof(ESCParametersViewModel.ESCStatus))
                            {
                                Device.DynotisData.ESCValue = _currentESCParameters.ESCValue;
                                Device.DynotisData.ESCStatus = _currentESCParameters.ESCStatus;
                                OnPropertyChanged(nameof(Device.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        private BatterySecurityLimitsViewModel _currentBatterySecurityLimits;
        public BatterySecurityLimitsViewModel CurrentBatterySecurityLimits
        {
            get => _currentBatterySecurityLimits;
            set
            {
                if (SetProperty(ref _currentBatterySecurityLimits, value))
                {
                    if (_currentBatterySecurityLimits != null)
                    {
                        _currentBatterySecurityLimits.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(BatterySecurityLimitsViewModel.MaxCurrent) ||
                                e.PropertyName == nameof(BatterySecurityLimitsViewModel.BatteryLevel) ||
                                e.PropertyName == nameof(BatterySecurityLimitsViewModel.SecurityStatus))
                            {
                                Device.DynotisData.MaxCurrent = _currentBatterySecurityLimits.MaxCurrent;
                                Device.DynotisData.BatteryLevel = _currentBatterySecurityLimits.BatteryLevel;
                                Device.DynotisData.SecurityStatus = _currentBatterySecurityLimits.SecurityStatus;
                                OnPropertyChanged(nameof(Device.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        public DeviceViewModel(string portName)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                InterfaceVariables = new InterfaceVariables();
                Device = new Dynotis(portName);
                ChartViewModel = new ChartViewModel();
                _cancellationTokenSource = new CancellationTokenSource();

                Device.PropertyChanged += Device_PropertyChanged;

                InitializeDeviceAsync();
                Task.Run(() => UpdateDataLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
        }

        private async void InitializeDeviceAsync()
        {
            await Device.OpenPortAsync();
        }

        private async Task UpdateDataLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(UpdateTimeMillisecond); 

                DynotisData latestData;
                lock (_dataLock)
                {
                    latestData = _latestDynotisData;
                    _latestDynotisData = null;
                }

                if (latestData != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        InterfaceVariables.UpdateFrom(latestData);
                        ChartViewModel.UpdateChartData(InterfaceVariables); // InterfaceVariables'den güncellemeleri ChartViewModel'e iletme

                    });
                }
            }
        }

        private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Dynotis.DynotisData))
            {
                lock (_dataLock)
                {
                    _latestDynotisData = Device.DynotisData;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
