using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.ViewModels.Managers;

namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DeviceViewModel> DevicesViewModel { get; }

        private BatterySecurityLimitsManager _batterySecurityLimitsManager;
        private EquipmentParametersManager _equipmentParametersManager;
        private ESCParametersManager _escParametersManager;

        public MainViewModel()
        {
            DevicesViewModel = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            _batterySecurityLimitsManager = new BatterySecurityLimitsManager();
            _equipmentParametersManager = new EquipmentParametersManager();
            _escParametersManager = new ESCParametersManager();

            DeviceManager.Instance.DeviceConnected += OnDeviceConnected;
            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
        }

        private void OnDeviceConnected(DeviceViewModel device)
        {
            if (!DevicesViewModel.Contains(device))
            {
                DevicesViewModel.Add(device);
                device.CurrentBatterySecurityLimits = _batterySecurityLimitsManager.GetBatterySecurityLimitsViewModel(device.Device.PortName, device.Device.DynotisData);
                device.CurrentEquipmentParameters = _equipmentParametersManager.GetEquipmentParametersViewModel(device.Device.PortName, device.Device.DynotisData);
                device.CurrentESCParameters = _escParametersManager.GetESCParametersViewModel(device.Device.PortName, device.Device.DynotisData);
            }
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (DevicesViewModel.Contains(device))
            {
                DevicesViewModel.Remove(device);
                _batterySecurityLimitsManager.RemoveBatterySecurityLimitsViewModel(device.Device.PortName);
                _equipmentParametersManager.RemoveEquipmentParametersViewModel(device.Device.PortName);
                _escParametersManager.RemoveESCParametersViewModel(device.Device.PortName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
