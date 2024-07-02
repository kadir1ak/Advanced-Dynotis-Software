using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.ViewModels.Main;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class SingleTestViewModel : INotifyPropertyChanged
    {
        private DeviceViewModel _selectedDevice;
        private DeviceViewModel _connectedDevice;
        private double _userPropellerArea;
        private double _userMotorInner;
        private double _userNoLoadCurrents;
        private double _userESCValue;
        private string _userESCStatus;

        public ObservableCollection<DeviceViewModel> AvailableDevices { get; }

        public DeviceViewModel SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;
                    OnPropertyChanged();
                }
            }
        }

        public DeviceViewModel ConnectedDevice
        {
            get => _connectedDevice;
            set
            {
                if (_connectedDevice != value)
                {
                    _connectedDevice = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UserPropellerArea));
                    OnPropertyChanged(nameof(UserMotorInner));
                    OnPropertyChanged(nameof(UserNoLoadCurrents));
                    OnPropertyChanged(nameof(UserESCStatus));
                    OnPropertyChanged(nameof(UserESCValue));
                }
            }
        }

        public double UserPropellerArea
        {
            get => _userPropellerArea;
            set
            {
                if (_userPropellerArea != value)
                {
                    _userPropellerArea = value;
                    if (ConnectedDevice?.Device.DynotisData != null)
                    {
                        ConnectedDevice.Device.DynotisData.PropellerArea = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public double UserMotorInner
        {
            get => _userMotorInner;
            set
            {
                if (_userMotorInner != value)
                {
                    _userMotorInner = value;
                    if (ConnectedDevice?.Device.DynotisData != null)
                    {
                        ConnectedDevice.Device.DynotisData.MotorInner = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public double UserNoLoadCurrents
        {
            get => _userNoLoadCurrents;
            set
            {
                if (_userNoLoadCurrents != value)
                {
                    _userNoLoadCurrents = value;
                    if (ConnectedDevice?.Device.DynotisData != null)
                    {
                        ConnectedDevice.Device.DynotisData.NoLoadCurrents = value;
                    }
                    OnPropertyChanged();
                }
            }
        }
        public double UserESCValue
        {
            get => _userESCValue;
            set
            {
                if (_userESCValue != value)
                {
                    _userESCValue = value;
                    if (ConnectedDevice?.Device.DynotisData != null)
                    {
                        ConnectedDevice.Device.DynotisData.ESCValue = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public string UserESCStatus
        {
            get => _userESCStatus;
            set
            {
                if (_userESCStatus != value)
                {
                    _userESCStatus = value;
                    if (ConnectedDevice?.Device.DynotisData != null)
                    {
                        ConnectedDevice.Device.DynotisData.ESCStatus = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ConnectCommand { get; }

        public SingleTestViewModel()
        {
            AvailableDevices = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            ConnectCommand = new RelayCommand(async _ => await ConnectToDeviceAsync());

            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
            DeviceManager.Instance.DeviceConnected += OnDeviceConnected;
        }

        private async Task ConnectToDeviceAsync()
        {
            if (SelectedDevice == null || ConnectedDevice == SelectedDevice) return;

            await DeviceManager.Instance.ConnectToDeviceAsync(SelectedDevice.Device.PortName);
            ConnectedDevice = SelectedDevice;
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (ConnectedDevice == device)
            {
                ConnectedDevice = null;
            }
            RefreshAvailableDevices();
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
            // Reassign SelectedDevice if it is still available
            SelectedDevice = AvailableDevices.FirstOrDefault(d => d.Device.PortName == previouslySelectedDevice?.Device.PortName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
