using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Advanced_Dynotis_Software.Models.Devices.Device;
using Advanced_Dynotis_Software.Models.Interface;
using Advanced_Dynotis_Software.Services.BindableBase;

namespace Advanced_Dynotis_Software.Models.Devices
{
    public class DevicesModel : BindableBase
    {
        public DevicesModel()
        {
            _devices = new ConcurrentDictionary<string, (DeviceModel Device, InterfaceData InterfaceData)>();
        }

        // Benzersiz cihaz ID'si ile cihaz ve InterfaceData'larını tutan sözlük
        private ConcurrentDictionary<string, (DeviceModel Device, InterfaceData InterfaceData)> _devices;

        // Cihazları dışarıya yalnızca okunabilir olarak sunar
        public IReadOnlyDictionary<string, (DeviceModel Device, InterfaceData InterfaceData)> Devices => _devices;


        // Yeni bir cihaz eklemek için yöntem
        private void AddDevice(DeviceModel device, string deviceId)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("Device ID cannot be null or empty.", nameof(deviceId));

            if (_devices.ContainsKey(deviceId))
            {
                throw new InvalidOperationException($"A device with ID '{deviceId}' already exists.");
            }

            var interfaceData = new InterfaceData(device);
            device.Info.ID = deviceId;

            if (!_devices.TryAdd(deviceId, (device, interfaceData)))
            {
                throw new InvalidOperationException($"Failed to add device with ID '{deviceId}'.");
            }

            OnPropertyChanged(nameof(Devices));
        }

        // Bir cihazı listeden kaldırmak için yöntem
        private void RemoveDevice(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("Device ID cannot be null or empty.", nameof(deviceId));

            if (!_devices.TryRemove(deviceId, out _))
            {
                throw new KeyNotFoundException($"No device found with ID '{deviceId}'.");
            }

            OnPropertyChanged(nameof(Devices));
        }

        // Belirli bir porta bağlandığında _devices listesine cihazın eklenmesini yöntem
        public async Task ConnectDevice(string deviceId, string portName)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID cannot be null or empty.", nameof(deviceId));
            if (string.IsNullOrWhiteSpace(portName))
                throw new ArgumentException("Port name cannot be null or empty.", nameof(portName));

            if (_devices.ContainsKey(deviceId))
            {
                throw new InvalidOperationException($"Device with ID '{deviceId}' is already connected.");
            }

            var newDevice = new DeviceModel();

            try
            {
                await newDevice.SerialPortConnect(portName);

                int retryCount = 0;
                while (!newDevice.Info.DeviceIdentified && retryCount < 10)
                {
                    await Task.Delay(100);
                    retryCount++;
                }

                if (newDevice.Info.DeviceIdentified)
                {
                    AddDevice(newDevice, deviceId);
                }
                else
                {
                   // throw new InvalidOperationException($"Device on port '{portName}' could not be identified.");
                }
            }
            catch (Exception ex)
            {
                newDevice.StopSerialPort();
                throw new InvalidOperationException($"Failed to connect device with ID '{deviceId}' on port '{portName}': {ex.Message}", ex);
            }
        }

        // Tüm cihazları seri port bağlantısını durdurmak için yöntem
        private void StopAllDevices()
        {
            Parallel.ForEach(_devices.Values, deviceData =>
            {
                deviceData.Device.StopSerialPort();
            });
        }

        // Tüm cihazların verilerini güncellemek için yöntem
        private void UpdateAllDevices()
        {
            Parallel.ForEach(_devices.Values, deviceData =>
            {
                deviceData.InterfaceData.UpdateDeviceData(deviceData.Device);
                deviceData.InterfaceData.UpdateCharts(deviceData.Device);
            });
        }
    }
}