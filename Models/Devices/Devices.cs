using System;
using System.Collections.Generic;
using DeviceModel = Advanced_Dynotis_Software.Models.Device.Device;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Advanced_Dynotis_Software.Models.Devices
{
    public class Devices
    {
        // Cihaz listesini tutan koleksiyon
        private readonly List<DeviceModel> _devices;

        public Devices()
        {
            _devices = new List<DeviceModel>();
        }

        // Tüm cihazları döndürür
        public IReadOnlyList<DeviceModel> DeviceList => _devices.AsReadOnly();

        // Yeni bir cihaz ekler
        public void AddDevice(DeviceModel device)
        {
            if (device != null && !_devices.Contains(device))
            {
                _devices.Add(device);
            }
        }

        // Belirtilen cihazı kaldırır
        public void RemoveDevice(DeviceModel device)
        {
            if (device != null && _devices.Contains(device))
            {
                _devices.Remove(device);
            }
        }

        // Cihazları temizler
        public void ClearDevices()
        {
            _devices.Clear();
        }
    }
}
