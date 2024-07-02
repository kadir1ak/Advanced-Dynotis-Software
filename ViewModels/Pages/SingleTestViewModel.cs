using Advanced_Dynotis_Software.ViewModels.Managers;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.ViewModels.Main;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advanced_Dynotis_Software.Models.Dynotis;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class SingleTestViewModel : INotifyPropertyChanged
    {
        private DeviceViewModel _selectedDevice;
        private DeviceViewModel _connectedDevice;
        private EquipmentParametersManager _equipmentParametersManager;
        private EquipmentParametersViewModel _currentEquipmentParameters;

        public ObservableCollection<DeviceViewModel> AvailableDevices { get; }

        public DeviceViewModel SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }

        public DeviceViewModel ConnectedDevice
        {
            get => _connectedDevice;
            set
            {
                if (SetProperty(ref _connectedDevice, value))
                {
                    if (_connectedDevice != null)
                    {
                        _connectedDevice.Device.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(Dynotis.DynotisData))
                            {
                                OnPropertyChanged(nameof(ConnectedDevice.Device.DynotisData));
                            }
                        };
                    }
                }
            }
        }

        public EquipmentParametersViewModel CurrentEquipmentParameters
        {
            get => _currentEquipmentParameters;
            set
            {
                if (SetProperty(ref _currentEquipmentParameters, value))
                {
                    // Ensure the data is updated when the EquipmentParametersViewModel changes
                    CurrentEquipmentParameters.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == nameof(EquipmentParametersViewModel.UserPropellerArea) ||
                            e.PropertyName == nameof(EquipmentParametersViewModel.UserMotorInner) ||
                            e.PropertyName == nameof(EquipmentParametersViewModel.UserNoLoadCurrents))
                        {
                            ConnectedDevice.Device.DynotisData.PropellerArea = CurrentEquipmentParameters.UserPropellerArea;
                            ConnectedDevice.Device.DynotisData.MotorInner = CurrentEquipmentParameters.UserMotorInner;
                            ConnectedDevice.Device.DynotisData.NoLoadCurrents = CurrentEquipmentParameters.UserNoLoadCurrents;
                            // Trigger PropertyChanged event for the entire DynotisData object
                            ConnectedDevice.Device.OnPropertyChanged(nameof(DynotisData));
                        }
                    };
                }
            }
        }

        public ICommand ConnectCommand { get; }

        public SingleTestViewModel()
        {
            AvailableDevices = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            ConnectCommand = new RelayCommand(async _ => await ConnectToDeviceAsync());
            _equipmentParametersManager = new EquipmentParametersManager();

            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
            DeviceManager.Instance.DeviceConnected += OnDeviceConnected;
        }

        private async Task ConnectToDeviceAsync()
        {
            if (SelectedDevice == null || ConnectedDevice == SelectedDevice) return;

            await DeviceManager.Instance.ConnectToDeviceAsync(SelectedDevice.Device.PortName);
            ConnectedDevice = SelectedDevice;

            CurrentEquipmentParameters = _equipmentParametersManager.GetEquipmentParametersViewModel(SelectedDevice.Device.PortName, SelectedDevice.Device.DynotisData);
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (ConnectedDevice == device)
            {
                ConnectedDevice = null;
                CurrentEquipmentParameters = null;
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

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
