using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advanced_Dynotis_Software.ViewModels.Main;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.ViewModels.Managers;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class MultiTestViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<DeviceViewModel> _devicesViewModel;
        private readonly EquipmentParametersManager _equipmentParametersManager;

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
            DevicesViewModel = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            DeviceManager.Instance.DeviceConnected += OnDeviceConnected;
            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;

            foreach (var device in DevicesViewModel)
            {
                device.EquipmentParametersViewModel = _equipmentParametersManager.GetEquipmentParametersViewModel(device.Device.PortName, device.Device.DynotisData);
            }
        }

        private void OnDeviceConnected(DeviceViewModel device)
        {
            if (!DevicesViewModel.Contains(device))
            {
                DevicesViewModel.Add(device);
                // Add EquipmentParametersViewModel for the new device
                device.EquipmentParametersViewModel = _equipmentParametersManager.GetEquipmentParametersViewModel(device.Device.PortName, device.Device.DynotisData);
            }
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (DevicesViewModel.Contains(device))
            {
                DevicesViewModel.Remove(device);
                // Optionally remove the EquipmentParametersViewModel for the disconnected device
                _equipmentParametersManager.RemoveEquipmentParametersViewModel(device.Device.PortName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
