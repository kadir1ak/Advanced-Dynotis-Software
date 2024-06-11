using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Advanced_Dynotis_Software.Models.Serial;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.ViewModels.Device;

namespace Advanced_Dynotis_Software.ViewModels.Windows
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DeviceViewModel> devicesViewModel { get; set; }
        public SerialPortsManager serialPortsManager { get; set; }

        private ObservableCollection<string>? _availablePorts;
        public ObservableCollection<string>? AvailablePorts
        {
            get => _availablePorts;
            set
            {
                _availablePorts = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddDeviceCommand { get; set; }
        public ICommand RemoveDeviceCommand { get; set; }

        private string selectedPort;
        public string SelectedPort
        {
            get => selectedPort;
            set
            {
                selectedPort = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            devicesViewModel = new ObservableCollection<DeviceViewModel>();
            serialPortsManager = new SerialPortsManager();
            serialPortsManager.SerialPortsEvent += SerialPortsEvent;
            AvailablePorts = new ObservableCollection<string>(serialPortsManager.GetSerialPorts());
            AddMiniCard();
            AddDeviceCommand = new RelayCommand(AddDevice);
            RemoveDeviceCommand = new RelayCommand(RemoveDevice);
        }

        public void SerialPortsEvent()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailablePorts = new ObservableCollection<string>(serialPortsManager.GetSerialPorts());
                AddMiniCard();
            });
        }

        public void AddMiniCard()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Eski cihazları kaldır
                foreach (var device in devicesViewModel.ToList())
                {
                    if (!AvailablePorts.Contains(device.Device.PortName))
                    {
                        _ = device.Device.ClosePortAsync();
                        devicesViewModel.Remove(device);
                    }
                }

                // Yeni cihazları ekle
                foreach (var port in AvailablePorts)
                {
                    if (!string.IsNullOrEmpty(port) && !devicesViewModel.Any(device => device.Device.PortName == port))
                    {
                        devicesViewModel.Add(new DeviceViewModel(port));
                    }
                }
            });
        }

        private void AddDevice(object parameter)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(SelectedPort))
                {
                    // Check if the device with the selected port already exists
                    if (!devicesViewModel.Any(device => device.Device.PortName == SelectedPort))
                    {
                        devicesViewModel.Add(new DeviceViewModel(SelectedPort));
                    }
                }
            });
        }

        private void RemoveDevice(object parameter)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (parameter is DeviceViewModel deviceViewModel)
                {
                    _ = deviceViewModel.Device.ClosePortAsync();
                    devicesViewModel.Remove(deviceViewModel);
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
