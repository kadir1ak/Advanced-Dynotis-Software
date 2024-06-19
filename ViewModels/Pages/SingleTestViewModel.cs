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

        public ObservableCollection<DeviceViewModel> AvailableDevices { get; set; }
        public ObservableCollection<DeviceViewModel> ConnectedDevices { get; set; }

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
            ConnectedDevices = DeviceManager.Instance.ConnectedDevices;
            ConnectCommand = new RelayCommand(ConnectToDevice);

            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
        }

        private void ConnectToDevice(object parameter)
        {
            if (SelectedDevice != null && !ConnectedDevices.Contains(SelectedDevice))
            {
                DeviceManager.Instance.AddConnectedDevice(SelectedDevice);
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
