using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
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

        private List<string>? _availablePorts;
        public List<string>? AvailablePorts
        {
            get => _availablePorts;
            set
            {
                _availablePorts = value;
                OnPropertyPortsChanged();
            }
        }
        protected void OnPropertyPortsChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                OnPropertyChanged(nameof(SelectedPort));
            }
        }
        public MainViewModel()
        {
            devicesViewModel = new ObservableCollection<DeviceViewModel>();
            serialPortsManager = new SerialPortsManager();
            serialPortsManager.serialPortsEvent += SerialPortsEvent;
            AvailablePorts = serialPortsManager.serialPorts.Select(port => port.PortName).ToList();
            AddMiniCard();
            AddDeviceCommand = new RelayCommand(AddDevice);
            RemoveDeviceCommand = new RelayCommand(RemoveDevice);
        }
        public void SerialPortsEvent()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailablePorts = serialPortsManager.serialPorts.Select(port => port.PortName).ToList();
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
                    if (!AvailablePorts.Contains(device.Device.Name))
                    {
                        device.Device.ClosePort();
                        devicesViewModel.Remove(device);
                    }
                }

                // Yeni cihazları ekle
                foreach (var port in AvailablePorts)
                {
                    if (!string.IsNullOrEmpty(port) && !devicesViewModel.Any(device => device.Device.Name == port))
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
                    if (!devicesViewModel.Any(device => device.Device.Name == SelectedPort))
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
                    deviceViewModel.Device.ClosePort();
                    devicesViewModel.Remove(deviceViewModel);
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
