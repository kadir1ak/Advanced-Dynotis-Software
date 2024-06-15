using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Advanced_Dynotis_Software.Models.Serial;
using Advanced_Dynotis_Software.ViewModels.Device;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.Services;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class SingleTestViewModel : INotifyPropertyChanged
    {
        private SerialPortsManager _serialPortsManager;
        private string _selectedPort;
        private ObservableCollection<DeviceViewModel> _connectedDevices;
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

        public ObservableCollection<DeviceViewModel> ConnectedDevices
        {
            get => _connectedDevices;
            set
            {
                _connectedDevices = value;
                OnPropertyChanged();
            }
        }

        public string SelectedPort
        {
            get => _selectedPort;
            set
            {
                _selectedPort = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCommand { get; set; }

        public SingleTestViewModel()
        {
            _serialPortsManager = new SerialPortsManager();
            AvailablePorts = new ObservableCollection<string>(_serialPortsManager.GetSerialPorts());
            ConnectedDevices = new ObservableCollection<DeviceViewModel>();
            _serialPortsManager.SerialPortsEvent += OnSerialPortsChanged;
            ConnectCommand = new RelayCommand(ConnectToDevice);
        }

        private void OnSerialPortsChanged()
        {
            AvailablePorts = new ObservableCollection<string>(_serialPortsManager.GetSerialPorts());
            OnPropertyChanged(nameof(AvailablePorts));
        }

        private async void ConnectToDevice(object parameter)
        {
            if (!string.IsNullOrEmpty(SelectedPort))
            {
                var deviceViewModel = await DeviceManager.Instance.ConnectToDeviceAsync(SelectedPort);
                if (!ConnectedDevices.Contains(deviceViewModel))
                {
                    ConnectedDevices.Clear();
                    ConnectedDevices.Add(deviceViewModel);
                }
            }
        }

        public async void OnNavigatedFrom()
        {
            foreach (var device in ConnectedDevices)
            {
                await DeviceManager.Instance.DisconnectDeviceAsync(device.Device.PortName);
            }
            ConnectedDevices.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
