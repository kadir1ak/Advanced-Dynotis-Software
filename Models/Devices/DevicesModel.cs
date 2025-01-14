using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advanced_Dynotis_Software.Models.Devices.Device;
using Advanced_Dynotis_Software.Models.Interface;
using Advanced_Dynotis_Software.Services.BindableBase;

namespace Advanced_Dynotis_Software.Models.Devices
{
    public class DevicesModel : BindableBase
    {
        // Benzersiz cihaz ID'si ile cihaz ve InterfaceData'larını tutan sözlük
        private ConcurrentDictionary<string, (DeviceModel Device, InterfaceData InterfaceData)> _devices = new();

        // Cihazları dışarıya yalnızca okunabilir olarak sunar
        public IReadOnlyDictionary<string, (DeviceModel Device, InterfaceData InterfaceData)> Devices => _devices;

        // Yeni bir cihaz eklemek için yöntem
        public void AddDevice(DeviceModel device, string deviceId)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("Device ID cannot be null or empty.", nameof(deviceId));

            if (_devices.ContainsKey(deviceId))
            {
                throw new InvalidOperationException($"A device with ID '{deviceId}' already exists.");
            }

            // Yeni cihaz için InterfaceData oluştur ve cihazı sözlüğe ekle
            var interfaceData = new InterfaceData(device);
            device.Info.ID = deviceId;

            if (!_devices.TryAdd(deviceId, (device, interfaceData)))
            {
                throw new InvalidOperationException($"Failed to add device with ID '{deviceId}'.");
            }

            OnPropertyChanged(nameof(Devices)); // UI güncellenmesi için bildir
        }

        // Bir cihazı listeden kaldırmak için yöntem
        public void RemoveDevice(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("Device ID cannot be null or empty.", nameof(deviceId));

            if (!_devices.TryRemove(deviceId, out _))
            {
                throw new KeyNotFoundException($"No device found with ID '{deviceId}'.");
            }

            OnPropertyChanged(nameof(Devices)); // UI güncellenmesi için bildir
        }

        // Belirli bir cihazı seri porta bağlamak için yöntem
        public async Task ConnectDevice(string deviceId, string portName)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("Device ID cannot be null or empty.", nameof(deviceId));
            if (string.IsNullOrWhiteSpace(portName)) throw new ArgumentException("Port name cannot be null or empty.", nameof(portName));

            if (!_devices.TryGetValue(deviceId, out var deviceData))
            {
                throw new KeyNotFoundException($"No device found with ID '{deviceId}'.");
            }

            await deviceData.Device.SerialPortConnect(portName);
        }

        // Tüm cihazları seri port bağlantısını durdurmak için yöntem
        public void StopAllDevices()
        {
            Parallel.ForEach(_devices.Values, deviceData =>
            {
                deviceData.Device.StopSerialPort();
            });
        }

        // Tüm cihazların verilerini güncellemek için yöntem
        public void UpdateAllDevices()
        {
            Parallel.ForEach(_devices.Values, deviceData =>
            {
                deviceData.InterfaceData.UpdateDeviceData(deviceData.Device);
                deviceData.InterfaceData.UpdateCharts(deviceData.Device);
            });
        }

        // Belirli bir cihazın verilerini güncellemek için yöntem
        public void UpdateDevice(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("Device ID cannot be null or empty.", nameof(deviceId));

            if (!_devices.TryGetValue(deviceId, out var deviceData))
            {
                throw new KeyNotFoundException($"No device found with ID '{deviceId}'.");
            }

            deviceData.InterfaceData.UpdateDeviceData(deviceData.Device);
            deviceData.InterfaceData.UpdateCharts(deviceData.Device);
        }
    }
}