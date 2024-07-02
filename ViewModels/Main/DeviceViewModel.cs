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

        private EquipmentParametersViewModel _equipmentParametersViewModel;
        public EquipmentParametersViewModel EquipmentParametersViewModel
        {
            get => _equipmentParametersViewModel;
            set
            {
                if (_equipmentParametersViewModel != value)
                {
                    _equipmentParametersViewModel = value;
                    OnPropertyChanged();
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

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
