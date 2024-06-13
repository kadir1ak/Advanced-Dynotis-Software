using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Advanced_Dynotis_Software.Models.Serial;
using Advanced_Dynotis_Software.ViewModels.Device;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class MultiTestViewModel : INotifyPropertyChanged
    {
        private SerialPortsManager _serialPortsManager;
        private ObservableCollection<DeviceViewModel> _devicesViewModel;
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

        public ObservableCollection<DeviceViewModel> DevicesViewModel
        {
            get => _devicesViewModel;
            set
            {
                _devicesViewModel = value;
                OnPropertyChanged();
            }
        }

        public MultiTestViewModel()
        {
            _serialPortsManager = new SerialPortsManager();
            AvailablePorts = new ObservableCollection<string>(_serialPortsManager.GetSerialPorts());
            DevicesViewModel = new ObservableCollection<DeviceViewModel>();
            _serialPortsManager.SerialPortsEvent += OnSerialPortsChanged;
        }

        public void OnNavigatedTo()
        {
            InitializeDevices();
        }

        private void OnSerialPortsChanged()
        {
            AvailablePorts = new ObservableCollection<string>(_serialPortsManager.GetSerialPorts());
            OnPropertyChanged(nameof(AvailablePorts));
            InitializeDevices();
        }

        private void InitializeDevices()
        {
            foreach (var port in AvailablePorts)
            {
                if (!DevicesViewModel.Any(device => device.Device.PortName == port))
                {
                    var deviceViewModel = new DeviceViewModel(port);
                    deviceViewModel.Device.OpenPortAsync();  // Cihazı otomatik olarak bağla
                    DevicesViewModel.Add(deviceViewModel);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
