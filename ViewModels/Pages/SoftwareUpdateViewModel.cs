using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.ViewModels.Main;
using Advanced_Dynotis_Software.ViewModels.Managers;
using Advanced_Dynotis_Software.Models.Dynotis;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class SoftwareUpdateViewModel : INotifyPropertyChanged
    {
        // Add properties and methods as needed
        private DeviceViewModel _selectedDevice;
        private DeviceViewModel _connectedDevice;
        private FirmwareUpdateManager _firmwareUpdateManager;
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

        public ICommand ConnectCommand { get; }

        public SoftwareUpdateViewModel()
        {
            AvailableDevices = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            ConnectCommand = new RelayCommand(async _ => await ConnectToDeviceAsync());
            _firmwareUpdateManager = new FirmwareUpdateManager();

            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
            DeviceManager.Instance.DeviceConnected += OnDeviceConnected;
        }

        private async Task ConnectToDeviceAsync()
        {
            if (SelectedDevice == null || ConnectedDevice == SelectedDevice) return;

            await DeviceManager.Instance.ConnectToDeviceAsync(SelectedDevice.Device.PortName);
            ConnectedDevice = SelectedDevice;

            ConnectedDevice.CurrentFirmwareUpdate = _firmwareUpdateManager.GetFirmwareUpdateViewModel(SelectedDevice.Device);
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (ConnectedDevice == device)
            {
                ConnectedDevice = null;
                ConnectedDevice.CurrentFirmwareUpdate = null;
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
