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
using Advanced_Dynotis_Software.Services.Logger;
using Irony.Parsing;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.IO.Ports;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class SoftwareUpdateViewModel : INotifyPropertyChanged
    {
        // Add properties and methods as needed
        private DeviceViewModel _selectedDevice;
        private DeviceViewModel _connectedDevice;
        //Firmware update yapılacak cihaz
        private SerialPort _device_Port;
        private string _device_Port_Name;
        private string _bootloader_Mode;
        private string _mode;
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
        public ICommand FirmwareUpdateCommand { get; }

        public SoftwareUpdateViewModel()
        {
            AvailableDevices = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            ConnectCommand = new RelayCommand(async _ => await ConnectToDeviceAsync());
            FirmwareUpdateCommand = new RelayCommand(async _ => await FirmwareUpdateAsync());

            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
            DeviceManager.Instance.DeviceConnected += OnDeviceConnected;
        }

        private async Task ConnectToDeviceAsync()
        {
            if (SelectedDevice == null || ConnectedDevice == SelectedDevice) return;

            await DeviceManager.Instance.ConnectToDeviceAsync(SelectedDevice.Device.PortName);
            ConnectedDevice = SelectedDevice;
        }
        private async Task FirmwareUpdateAsync()
        {
            try
            {
                if (ConnectedDevice != null)
                {
                    // Bootloader komutu gönderiliyor
                    string bootloaderCommand = "Device_Status:6;ESC:800;";
                    await ConnectedDevice.Device.SendCommandAsync(bootloaderCommand);
                    // USB bağlantısını kapatın
                    ConnectedDevice.Device.Port.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo($"Bootloader işleminde hata oluştu (FirmwareUpdateAsync): {ex.Message}");
                MessageBox.Show($"Bootloader işleminde hata: {ex.Message}");
            }
        }
        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            try
            {
                if (ConnectedDevice == device)
                {
                    if (ConnectedDevice != null)
                    {
                        ConnectedDevice.CurrentFirmwareUpdate = null;
                    }
                    // Now safely set ConnectedDevice to null
                    ConnectedDevice = null;
                }
                RefreshAvailableDevices();
            }
            catch (Exception ex)
            {
                Logger.LogInfo($"OnDeviceDisconnected: {ex.Message}");
            }
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
        public string Bootloader_Mode
        {
            get => _bootloader_Mode;
            set
            {
                if (SetProperty(ref _bootloader_Mode, value))
                {
                    OnPropertyChanged(nameof(Bootloader_Mode));
                }
            }
        }
        public string Mode
        {
            get => _mode;
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    OnPropertyChanged(nameof(Mode));
                }
            }
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
