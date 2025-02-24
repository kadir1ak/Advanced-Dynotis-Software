﻿using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.ViewModels.Main;
using Advanced_Dynotis_Software.ViewModels.Managers;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class BalancerViewModel : INotifyPropertyChanged
    {
        private DeviceViewModel _selectedDevice;
        private DeviceViewModel _connectedDevice;
        private EquipmentParametersManager _equipmentParametersManager;
        private ESCParametersManager _escParametersManager;
        private BatterySecurityLimitsManager _batterySecurityLimitsManager;
        private TareManager _tareManager;
        private RecordManager _recordManager;
        private BalancerParametersManager _balancerParametersManager;
        private BalancerRoutingStepsManager _balancerRoutingStepsManager;
        private BalancedPropellerTestsChartManager _balancedPropellerTestsChartManager;
        private BalancerRoutingStepsVibrationLevelsManager _balancerRoutingStepsVibrationLevelsManager;
        private BalancedPropellersManager _balancedPropellersManager;
        private BalancerPolarChartManager _balancerPolarChartManager;
        public ObservableCollection<DeviceViewModel> AvailableDevices { get; }

        private List<double> _highVibrations;
        private int _balancingIterationStep;

        public List<double> HighVibrations
        {
            get => _highVibrations;
            set
            {
                _highVibrations = value;
                OnPropertyChanged(nameof(HighVibrations));
                UpdateVibrationChart();
            }
        }

        public int BalancingIterationStep
        {
            get => _balancingIterationStep;
            set
            {
                _balancingIterationStep = value;
                OnPropertyChanged(nameof(BalancingIterationStep));
            }
        }

        public SeriesCollection VibrationSeriesCollection { get; set; }
        public Func<double, string> XFormatter { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public DeviceViewModel SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }

        public DeviceViewModel ConnectedDevice
        {
            get => _connectedDevice;
            set
            {
                if (SetProperty(ref _connectedDevice, value))
                {
                    if (_connectedDevice != null)
                    {
                        _connectedDevice.Device.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(Dynotis.DynotisData))
                            {
                                OnPropertyChanged(nameof(ConnectedDevice.Device.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        public ICommand ConnectCommand { get; }

        public BalancerViewModel()
        {
            AvailableDevices = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            ConnectCommand = new RelayCommand(async _ => await ConnectToDeviceAsync());
            _equipmentParametersManager = new EquipmentParametersManager();
            _escParametersManager = new ESCParametersManager();
            _batterySecurityLimitsManager = new BatterySecurityLimitsManager();
            _tareManager = new TareManager();
            _recordManager = new RecordManager();
            _balancerParametersManager = new BalancerParametersManager();
            _balancerRoutingStepsManager = new BalancerRoutingStepsManager();
            _balancedPropellerTestsChartManager = new BalancedPropellerTestsChartManager();
            _balancerRoutingStepsVibrationLevelsManager = new BalancerRoutingStepsVibrationLevelsManager();
            _balancedPropellersManager = new BalancedPropellersManager();
            _balancerPolarChartManager = new BalancerPolarChartManager();

            HighVibrations = new List<double> {}; 
            BalancingIterationStep = 0;

            VibrationSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Vibration Levels",
                    Values = new ChartValues<double>(HighVibrations)
                }
            };

            XFormatter = value => value.ToString("0.00");
            YFormatter = value => value.ToString("0");

            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
            DeviceManager.Instance.DeviceConnected += OnDeviceConnected;
        }
        private void UpdateVibrationChart()
        {
            if (VibrationSeriesCollection != null && VibrationSeriesCollection.Count > 0)
            {
                var series = VibrationSeriesCollection[0] as LineSeries;
                if (series != null)
                {
                    series.Values = new ChartValues<double>(HighVibrations);
                }
            }
        }

        private async Task ConnectToDeviceAsync()
        {
            if (SelectedDevice == null || ConnectedDevice == SelectedDevice) return;

            await DeviceManager.Instance.ConnectToDeviceAsync(SelectedDevice.Device.PortName);
            ConnectedDevice = SelectedDevice;

            ConnectedDevice.CurrentEquipmentParameters = _equipmentParametersManager.GetEquipmentParametersViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData);
            ConnectedDevice.CurrentESCParameters = _escParametersManager.GetESCParametersViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData);
            ConnectedDevice.CurrentBatterySecurityLimits = _batterySecurityLimitsManager.GetBatterySecurityLimitsViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData);
            ConnectedDevice.CurrentTare = _tareManager.GetTareViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData, SelectedDevice.DeviceInterfaceVariables);
            ConnectedDevice.CurrentRecord = _recordManager.GetRecordViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData, SelectedDevice.DeviceInterfaceVariables);
            ConnectedDevice.CurrentBalancerParameters = _balancerParametersManager.GetBalancerParametersViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData, SelectedDevice.DeviceInterfaceVariables);
            ConnectedDevice.CurrentBalancerRoutingSteps = _balancerRoutingStepsManager.GetBalancerRoutingStepsViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData, SelectedDevice.DeviceInterfaceVariables);
            ConnectedDevice.CurrentBalancerRoutingStepsVibrationLevels = _balancerRoutingStepsVibrationLevelsManager.GetBalancerRoutingStepsVibrationLevelsViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData, SelectedDevice.DeviceInterfaceVariables);
            ConnectedDevice.CurrentBalancedPropellerTestsChart = _balancedPropellerTestsChartManager.GetBalancedPropellerTestsChartViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData, SelectedDevice.DeviceInterfaceVariables);
            ConnectedDevice.CurrentBalancedPropellers = _balancedPropellersManager.GetBalancedPropellersViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData, SelectedDevice.DeviceInterfaceVariables);
            ConnectedDevice.CurrentBalancerPolarChart = _balancerPolarChartManager.GetBalancerPolarChartViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData, SelectedDevice.DeviceInterfaceVariables);
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (ConnectedDevice == device)
            {
                ConnectedDevice = null;
                ConnectedDevice.CurrentEquipmentParameters = null;
                ConnectedDevice.CurrentESCParameters = null;
                ConnectedDevice.CurrentBatterySecurityLimits = null;
                ConnectedDevice.CurrentTare = null;
                ConnectedDevice.CurrentRecord = null;
                ConnectedDevice.CurrentBalancerParameters = null;
                ConnectedDevice.CurrentBalancerRoutingSteps = null;
                ConnectedDevice.CurrentBalancerRoutingStepsVibrationLevels = null;
                ConnectedDevice.CurrentBalancedPropellerTestsChart = null;
                ConnectedDevice.CurrentBalancedPropellers = null;
                ConnectedDevice.CurrentBalancerPolarChart = null;
            }
            RefreshAvailableDevices();
        }

        private void OnDeviceConnected(DeviceViewModel device)
        {
            if (!AvailableDevices.Contains(device))
            {
                AvailableDevices.Add(device);
            }
        }

        private void RefreshAvailableDevices()
        {
            var previouslySelectedDevice = SelectedDevice;
            AvailableDevices.Clear();
            foreach (var device in DeviceManager.Instance.GetAllDevices())
            {
                AvailableDevices.Add(device);
            }
            SelectedDevice = AvailableDevices.FirstOrDefault(d => d.Device.PortName == previouslySelectedDevice?.Device.PortName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
