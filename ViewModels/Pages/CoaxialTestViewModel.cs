using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.ViewModels.Device;
using System.Linq;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class CoaxialTestViewModel : INotifyPropertyChanged
    {
        private DeviceViewModel _selectedDeviceOne;
        private DeviceViewModel _selectedDeviceTwo;
        private DeviceViewModel _connectedOneDevice;
        private DeviceViewModel _connectedTwoDevice;

        public ObservableCollection<DeviceViewModel> AvailableDevicesOne { get; private set; }
        public ObservableCollection<DeviceViewModel> AvailableDevicesTwo { get; private set; }

        public DeviceViewModel SelectedDeviceOne
        {
            get => _selectedDeviceOne;
            set
            {
                if (_selectedDeviceOne != value)
                {
                    _selectedDeviceOne = value;
                    OnPropertyChanged();
                    UpdateAvailableDevices();
                }
            }
        }

        public DeviceViewModel SelectedDeviceTwo
        {
            get => _selectedDeviceTwo;
            set
            {
                if (_selectedDeviceTwo != value)
                {
                    _selectedDeviceTwo = value;
                    OnPropertyChanged();
                    UpdateAvailableDevices();
                }
            }
        }

        public DeviceViewModel ConnectedOneDevice
        {
            get => _connectedOneDevice;
            set
            {
                if (_connectedOneDevice != value)
                {
                    _connectedOneDevice = value;
                    OnPropertyChanged();
                }
            }
        }

        public DeviceViewModel ConnectedTwoDevice
        {
            get => _connectedTwoDevice;
            set
            {
                if (_connectedTwoDevice != value)
                {
                    _connectedTwoDevice = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ConnectCommand { get; }

        public CoaxialTestViewModel()
        {
            AvailableDevicesOne = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            AvailableDevicesTwo = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            ConnectCommand = new RelayCommand(ConnectToDevice);

            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;

            // Sayfa yüklenirken mevcut bağlantıları kontrol et ve ayarla
            UpdateAvailableDevices();
        }

        private async void ConnectToDevice(object parameter)
        {
            if (parameter is string device)
            {
                switch (device)
                {
                    case "One":
                        if (SelectedDeviceOne != null)
                        {
                            await DeviceManager.Instance.ConnectToDeviceAsync(SelectedDeviceOne.Device.PortName);
                            DeviceManager.Instance.AddConnectedDevice(SelectedDeviceOne);
                            ConnectedOneDevice = SelectedDeviceOne;
                        }
                        break;
                    case "Two":
                        if (SelectedDeviceTwo != null)
                        {
                            await DeviceManager.Instance.ConnectToDeviceAsync(SelectedDeviceTwo.Device.PortName);
                            DeviceManager.Instance.AddConnectedDevice(SelectedDeviceTwo);
                            ConnectedTwoDevice = SelectedDeviceTwo;
                        }
                        break;
                }
            }
        }

        private void UpdateAvailableDevices()
        {
            var allDevices = DeviceManager.Instance.GetAllDevices().ToList();

            AvailableDevicesOne.Clear();
            AvailableDevicesTwo.Clear();

            foreach (var device in allDevices)
            {
                if (device != SelectedDeviceTwo)
                {
                    AvailableDevicesOne.Add(device);
                }
                if (device != SelectedDeviceOne)
                {
                    AvailableDevicesTwo.Add(device);
                }
            }

            OnPropertyChanged(nameof(AvailableDevicesOne));
            OnPropertyChanged(nameof(AvailableDevicesTwo));
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (ConnectedOneDevice == device)
            {
                ConnectedOneDevice = null;
            }
            if (ConnectedTwoDevice == device)
            {
                ConnectedTwoDevice = null;
            }

            UpdateAvailableDevices();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
