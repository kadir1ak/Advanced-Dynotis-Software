using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.ViewModels.Main;
using Advanced_Dynotis_Software.ViewModels.Managers;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class MultiTestViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<DeviceViewModel> _devicesViewModel;
        private EquipmentParametersManager _equipmentParametersManager;
        private ESCParametersManager _escParametersManager;
        private BatterySecurityLimitsManager _batterySecurityLimitsManager;

        public ObservableCollection<DeviceViewModel> DevicesViewModel
        {
            get => _devicesViewModel;
            private set
            {
                if (_devicesViewModel != value)
                {
                    _devicesViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        public MultiTestViewModel()
        {
            _equipmentParametersManager = new EquipmentParametersManager();
            _escParametersManager = new ESCParametersManager();
            _batterySecurityLimitsManager = new BatterySecurityLimitsManager();
            DevicesViewModel = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            DeviceManager.Instance.DeviceConnected += OnDeviceConnected;
            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;

            foreach (var device in DevicesViewModel)
            {
                device.CurrentEquipmentParameters = _equipmentParametersManager.GetEquipmentParametersViewModel(device.Device.PortName, device.Device.DynotisData);
                device.CurrentESCParameters = _escParametersManager.GetESCParametersViewModel(device.Device.PortName, device.Device.DynotisData);
                device.CurrentBatterySecurityLimits = _batterySecurityLimitsManager.GetBatterySecurityLimitsViewModel(device.Device.PortName, device.Device.DynotisData);
            }
        }

        private void OnDeviceConnected(DeviceViewModel device)
        {
            if (!DevicesViewModel.Contains(device))
            {
                DevicesViewModel.Add(device);
                device.CurrentEquipmentParameters = _equipmentParametersManager.GetEquipmentParametersViewModel(device.Device.PortName, device.Device.DynotisData);
                device.CurrentESCParameters = _escParametersManager.GetESCParametersViewModel(device.Device.PortName, device.Device.DynotisData);
                device.CurrentBatterySecurityLimits = _batterySecurityLimitsManager.GetBatterySecurityLimitsViewModel(device.Device.PortName, device.Device.DynotisData);
            }
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (DevicesViewModel.Contains(device))
            {
                DevicesViewModel.Remove(device);
                _equipmentParametersManager.RemoveEquipmentParametersViewModel(device.Device.PortName);
                _escParametersManager.RemoveESCParametersViewModel(device.Device.PortName);
                _batterySecurityLimitsManager.RemoveBatterySecurityLimitsViewModel(device.Device.PortName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
