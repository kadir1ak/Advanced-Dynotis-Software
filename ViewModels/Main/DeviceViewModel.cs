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
        private bool _isUpdatingInterfaceVariables;

        public ChartViewModel ChartViewModel { get; }

        public string DeviceDisplayName => $"{Device.Model} - {Device.SeriNo}";

        private Dynotis _device;
        private DynotisData _latestDynotisData;
        private readonly object _dataLock = new();
        private CancellationTokenSource _cancellationTokenSource;

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
                    // Ensure the data is updated when the EquipmentParametersViewModel changes
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
                                OnPropertyChanged(nameof(Device.DynotisData)); // Trigger PropertyChanged event for the entire DynotisData object
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
                    // Ensure the data is updated when the ESCParametersViewModel changes
                    if (_currentESCParameters != null)
                    {
                        _currentESCParameters.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(ESCParametersViewModel.ESCValue) ||
                                e.PropertyName == nameof(ESCParametersViewModel.ESCStatus))
                            {
                                Device.DynotisData.ESCValue = _currentESCParameters.ESCValue;
                                Device.DynotisData.ESCStatus = _currentESCParameters.ESCStatus ? "Locked" : "Unlocked";
                                OnPropertyChanged(nameof(Device.DynotisData)); // Trigger PropertyChanged event for the entire DynotisData object
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
                    // Ensure the data is updated when the BatterySecurityLimitsViewModel changes
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
                                OnPropertyChanged(nameof(Device.DynotisData)); // Trigger PropertyChanged event for the entire DynotisData object
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
                InitializeTasks(_cancellationTokenSource.Token);
            }
        }

        private async void InitializeDeviceAsync()
        {
            await Device.OpenPortAsync();
        }

        private void InitializeTasks(CancellationToken token)
        {
            Task.Run(() => UpdateChartDataLoop(token), token);
            Task.Run(() => UpdateInterfaceVariablesLoop(token), token);
        }

        private async Task UpdateChartDataLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1); // 1000Hz

                DynotisData latestData;
                lock (_dataLock)
                {
                    latestData = _latestDynotisData;
                    _latestDynotisData = null;
                }

                if (latestData != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ChartViewModel.UpdateChartData(latestData);
                    }));
                }
            }
        }

        private async Task UpdateInterfaceVariablesLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(20); // 50Hz

                DynotisData latestData;
                lock (_dataLock)
                {
                    latestData = _latestDynotisData;
                }

                if (latestData != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        InterfaceVariables.UpdateFrom(latestData);
                    }));
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
                    OnPropertyChanged(nameof(Device.DynotisData)); // Notify when DynotisData changes
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
