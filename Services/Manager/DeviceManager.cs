using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Models.Serial;
using Advanced_Dynotis_Software.ViewModels.Main;

namespace Advanced_Dynotis_Software.Services
{
    public class DeviceManager
    {
        private static readonly Lazy<DeviceManager> _instance = new Lazy<DeviceManager>(() => new DeviceManager());
        public static DeviceManager Instance => _instance.Value;

        private readonly SerialPortsManager _serialPortsManager;

        public ObservableCollection<DeviceViewModel> Devices { get; private set; }
        public ObservableCollection<DeviceViewModel> ConnectedDevices { get; private set; }

        public event Action<DeviceViewModel> DeviceDisconnected;
        public event Action<DeviceViewModel> DeviceConnected;

        private DeviceManager()
        {
            Devices = new ObservableCollection<DeviceViewModel>();
            ConnectedDevices = new ObservableCollection<DeviceViewModel>();
            _serialPortsManager = new SerialPortsManager();
            _serialPortsManager.SerialPortAdded += OnSerialPortAdded;
            _serialPortsManager.SerialPortRemoved += OnSerialPortRemoved;

            InitializeDevices();
        }

        private void InitializeDevices()
        {
            foreach (var portName in _serialPortsManager.GetSerialPorts())
            {
                _ = ConnectToDeviceAsync(portName);
            }
        }

        private async void OnSerialPortAdded(string portName)
        {
            var device = await ConnectToDeviceAsync(portName);
            DeviceConnected?.Invoke(device);
        }

        private async void OnSerialPortRemoved(string portName)
        {
            var device = await DisconnectDeviceAsync(portName);
            DeviceDisconnected?.Invoke(device);
        }

        public async Task<DeviceViewModel> ConnectToDeviceAsync(string portName)
        {
            var existingDevice = Devices.FirstOrDefault(d => d.Device.PortName == portName);
            if (existingDevice != null)
            {
                return existingDevice;
            }

            var deviceViewModel = new DeviceViewModel(portName);
            await deviceViewModel.Device.OpenPortAsync();
            Devices.Add(deviceViewModel);
            DeviceConnected?.Invoke(deviceViewModel);
            return deviceViewModel;
        }

        public async Task<DeviceViewModel> DisconnectDeviceAsync(string portName)
        {
            var device = Devices.FirstOrDefault(d => d.Device.PortName == portName);
            if (device != null)
            {
                await device.Device.ClosePortAsync();
                Devices.Remove(device);
                ConnectedDevices.Remove(device);
                DeviceDisconnected?.Invoke(device);
                return device;
            }

            return null;
        }

        public void AddConnectedDevice(DeviceViewModel device)
        {
            if (!ConnectedDevices.Contains(device))
            {
                ConnectedDevices.Add(device);
            }
        }

        public ObservableCollection<DeviceViewModel> GetAllDevices()
        {
            return Devices;
        }
    }
}
