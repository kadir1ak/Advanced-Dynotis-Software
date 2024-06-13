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
        public ObservableCollection<DeviceViewModel> DevicesViewModel { get; set; }
        public SerialPortsManager SerialPortsManager { get; set; }

        private ObservableCollection<string> _availablePorts;
        public ObservableCollection<string> AvailablePorts
        {
            get => _availablePorts;
            set
            {
                _availablePorts = value;
                OnPropertyChanged();
            }
        }

        private string _selectedPort;
        public string SelectedPort
        {
            get => _selectedPort;
            set
            {
                _selectedPort = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddDeviceCommand { get; set; }
        public ICommand RemoveDeviceCommand { get; set; }

        public MainViewModel()
        {
            DevicesViewModel = new ObservableCollection<DeviceViewModel>();
            SerialPortsManager = new SerialPortsManager();
            SerialPortsManager.SerialPortsEvent += SerialPortsEvent;
            AvailablePorts = new ObservableCollection<string>(SerialPortsManager.GetSerialPorts());
            AddDeviceCommand = new RelayCommand(AddDevice);
            RemoveDeviceCommand = new RelayCommand(RemoveDevice);
        }

        private void SerialPortsEvent()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailablePorts = new ObservableCollection<string>(SerialPortsManager.GetSerialPorts());
                AddMiniCard();
            });
        }

        private void AddMiniCard()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var device in DevicesViewModel.ToList())
                {
                    if (!AvailablePorts.Contains(device.Device.PortName))
                    {
                        _ = device.Device.ClosePortAsync();
                        DevicesViewModel.Remove(device);
                    }
                }

                foreach (var port in AvailablePorts)
                {
                    if (!DevicesViewModel.Any(device => device.Device.PortName == port))
                    {
                        DevicesViewModel.Add(new DeviceViewModel(port));
                    }
                }
            });
        }

        private void AddDevice(object parameter)
        {
            if (!string.IsNullOrEmpty(SelectedPort) && !DevicesViewModel.Any(device => device.Device.PortName == SelectedPort))
            {
                DevicesViewModel.Add(new DeviceViewModel(SelectedPort));
            }
        }

        private void RemoveDevice(object parameter)
        {
            if (parameter is DeviceViewModel deviceViewModel)
            {
                _ = deviceViewModel.Device.ClosePortAsync();
                DevicesViewModel.Remove(deviceViewModel);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
