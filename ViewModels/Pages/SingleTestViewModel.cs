using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.ViewModels.Device;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class SingleTestViewModel : INotifyPropertyChanged
    {
        private DeviceViewModel _selectedDevice;
        private DeviceViewModel _connectedDevice;

        public ObservableCollection<DeviceViewModel> AvailableDevices { get; set; }

        public DeviceViewModel SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged();
            }
        }

        public DeviceViewModel ConnectedDevice
        {
            get => _connectedDevice;
            set
            {
                _connectedDevice = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCommand { get; set; }

        public SingleTestViewModel()
        {
            AvailableDevices = DeviceManager.Instance.GetAllDevices();
            ConnectCommand = new RelayCommand(ConnectToDevice);

            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
        }

        private async void ConnectToDevice(object parameter)
        {
            if (SelectedDevice == null) return;

            // Yeni cihazı bağla
            await DeviceManager.Instance.ConnectToDeviceAsync(SelectedDevice.Device.PortName);
            DeviceManager.Instance.AddConnectedDevice(SelectedDevice);
            ConnectedDevice = SelectedDevice;
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (ConnectedDevice == device)
            {
                ConnectedDevice = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
