using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.Device;

namespace Advanced_Dynotis_Software.Services
{
    public class DeviceManager
    {
        private static DeviceManager _instance;
        public static DeviceManager Instance => _instance ??= new DeviceManager();

        public ObservableCollection<DeviceViewModel> Devices { get; private set; }

        private DeviceManager()
        {
            Devices = new ObservableCollection<DeviceViewModel>();
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
            return deviceViewModel;
        }

        public async Task DisconnectDeviceAsync(string portName)
        {
            var device = Devices.FirstOrDefault(d => d.Device.PortName == portName);
            if (device != null)
            {
                await device.Device.ClosePortAsync();
                Devices.Remove(device);
            }
        }
    }
}
