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
            AvailableDevices = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            ConnectedDevices = new ObservableCollection<DeviceViewModel>();
            ConnectCommand = new RelayCommand(ConnectToDevice);
        }

        private async void ConnectToDevice(object parameter)
        {
            if (SelectedDevice != null && !ConnectedDevices.Contains(SelectedDevice))
            {
                ConnectedDevices.Add(SelectedDevice);
                await DeviceManager.Instance.ConnectToDeviceAsync(SelectedDevice.Device.PortName);
            }
        }

        public async void OnNavigatedFrom()
        {
            foreach (var device in ConnectedDevices.ToList())
            {
                await DeviceManager.Instance.DisconnectDeviceAsync(device.Device.PortName);
                ConnectedDevices.Remove(device);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
