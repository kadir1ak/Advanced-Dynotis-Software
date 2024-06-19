using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.ViewModels.Device;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class SingleTestViewModel : INotifyPropertyChanged
    {
        private DeviceViewModel _selectedDevice;
        private ObservableCollection<DeviceViewModel> _connectedDevices;
        private ObservableCollection<DeviceViewModel> _availableDevices;

        public ObservableCollection<DeviceViewModel> AvailableDevices
        {
            get => _availableDevices;
            set
            {
                _availableDevices = value;
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

        public DeviceViewModel SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCommand { get; set; }

        public SingleTestViewModel()
        {
            AvailableDevices = DeviceManager.Instance.GetAllDevices();
            ConnectedDevices = new ObservableCollection<DeviceViewModel>();
            ConnectCommand = new RelayCommand(ConnectToDevice);

            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
        }

        private void ConnectToDevice(object parameter)
        {
            if (SelectedDevice != null)
            {
                if (!ConnectedDevices.Contains(SelectedDevice))
                {
                    ConnectedDevices.Clear();
                    ConnectedDevices.Add(SelectedDevice);
                }
            }
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (ConnectedDevices.Contains(device))
            {
                ConnectedDevices.Remove(device);
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
