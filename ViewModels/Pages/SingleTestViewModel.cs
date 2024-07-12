using Advanced_Dynotis_Software.Models.Dynotis;
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

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class SingleTestViewModel : INotifyPropertyChanged
    {
        private DeviceViewModel _selectedDevice;
        private DeviceViewModel _connectedDevice;
        private EquipmentParametersManager _equipmentParametersManager;
        private ESCParametersManager _escParametersManager;
        private BatterySecurityLimitsManager _batterySecurityLimitsManager;
        private TareManager _tareManager;

        private EquipmentParametersViewModel _currentEquipmentParameters;
        private ESCParametersViewModel _currentESCParameters;
        private BatterySecurityLimitsViewModel _currentBatterySecurityLimits;
        private TareViewModel _currentTare;

        public ObservableCollection<DeviceViewModel> AvailableDevices { get; }

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
        public TareViewModel CurrentTare
        {
            get => _currentTare;
            set
            {
                if (SetProperty(ref _currentTare, value))
                {
                    if (_currentTare != null)
                    {
                        _currentTare.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(TareViewModel.TareCommand))
                            {
                                ConnectedDevice.InterfaceVariables.TareTorqueValue = ConnectedDevice.InterfaceVariables.Torque.Value;
                                ConnectedDevice.InterfaceVariables.TareThrustValue = ConnectedDevice.InterfaceVariables.Thrust.Value;
                                ConnectedDevice.InterfaceVariables.TareCurrentValue = ConnectedDevice.InterfaceVariables.Current;
                                ConnectedDevice.InterfaceVariables.TareMotorSpeedValue = ConnectedDevice.InterfaceVariables.MotorSpeed.Value;
                                ConnectedDevice.Device.OnPropertyChanged(nameof(Dynotis.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        public EquipmentParametersViewModel CurrentEquipmentParameters
        {
            get => _currentEquipmentParameters;
            set
            {
                if (SetProperty(ref _currentEquipmentParameters, value))
                {
                    CurrentEquipmentParameters.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == nameof(EquipmentParametersViewModel.UserPropellerArea) ||
                            e.PropertyName == nameof(EquipmentParametersViewModel.UserMotorInner) ||
                            e.PropertyName == nameof(EquipmentParametersViewModel.UserNoLoadCurrents))
                        {
                            ConnectedDevice.Device.DynotisData.PropellerArea = CurrentEquipmentParameters.UserPropellerArea;
                            ConnectedDevice.Device.DynotisData.MotorInner = CurrentEquipmentParameters.UserMotorInner;
                            ConnectedDevice.Device.DynotisData.NoLoadCurrents = CurrentEquipmentParameters.UserNoLoadCurrents;
                            ConnectedDevice.Device.OnPropertyChanged(nameof(Dynotis.DynotisData));
                        }
                    };
                }
            }
        }

        public ESCParametersViewModel CurrentESCParameters
        {
            get => _currentESCParameters;
            set
            {
                if (SetProperty(ref _currentESCParameters, value))
                {
                    CurrentESCParameters.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == nameof(ESCParametersViewModel.ESCValue) ||
                            e.PropertyName == nameof(ESCParametersViewModel.ESCStatus))
                        {
                            ConnectedDevice.Device.DynotisData.ESCValue = CurrentESCParameters.ESCValue;
                            ConnectedDevice.Device.DynotisData.ESCStatus = CurrentESCParameters.ESCStatus ? true : false;
                            ConnectedDevice.Device.OnPropertyChanged(nameof(Dynotis.DynotisData));
                        }
                    };
                }
            }
        }

        public BatterySecurityLimitsViewModel CurrentBatterySecurityLimits
        {
            get => _currentBatterySecurityLimits;
            set
            {
                if (SetProperty(ref _currentBatterySecurityLimits, value))
                {
                    CurrentBatterySecurityLimits.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == nameof(BatterySecurityLimitsViewModel.MaxCurrent) ||
                            e.PropertyName == nameof(BatterySecurityLimitsViewModel.BatteryLevel) ||
                            e.PropertyName == nameof(BatterySecurityLimitsViewModel.SecurityStatus))
                        {
                            ConnectedDevice.Device.DynotisData.MaxCurrent = CurrentBatterySecurityLimits.MaxCurrent;
                            ConnectedDevice.Device.DynotisData.BatteryLevel = CurrentBatterySecurityLimits.BatteryLevel;
                            ConnectedDevice.Device.DynotisData.SecurityStatus = CurrentBatterySecurityLimits.SecurityStatus;
                            ConnectedDevice.Device.OnPropertyChanged(nameof(Dynotis.DynotisData));
                        }
                    };
                }
            }
        }

        public ICommand ConnectCommand { get; }

        public SingleTestViewModel()
        {
            AvailableDevices = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            ConnectCommand = new RelayCommand(async _ => await ConnectToDeviceAsync());
            _equipmentParametersManager = new EquipmentParametersManager();
            _escParametersManager = new ESCParametersManager();
            _batterySecurityLimitsManager = new BatterySecurityLimitsManager();
            _tareManager = new TareManager();

            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
            DeviceManager.Instance.DeviceConnected += OnDeviceConnected;
        }

        private async Task ConnectToDeviceAsync()
        {
            if (SelectedDevice == null || ConnectedDevice == SelectedDevice) return;

            await DeviceManager.Instance.ConnectToDeviceAsync(SelectedDevice.Device.PortName);
            ConnectedDevice = SelectedDevice;

            CurrentEquipmentParameters = _equipmentParametersManager.GetEquipmentParametersViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData);
            CurrentESCParameters = _escParametersManager.GetESCParametersViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData);
            CurrentBatterySecurityLimits = _batterySecurityLimitsManager.GetBatterySecurityLimitsViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData);
            CurrentTare = _tareManager.GetTareViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData);
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (ConnectedDevice == device)
            {
                ConnectedDevice = null;
                CurrentEquipmentParameters = null;
                CurrentESCParameters = null;
                CurrentBatterySecurityLimits = null;
                CurrentTare = null;
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
