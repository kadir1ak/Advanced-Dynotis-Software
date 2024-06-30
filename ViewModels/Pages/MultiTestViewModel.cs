using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advanced_Dynotis_Software.ViewModels.Main;
using Advanced_Dynotis_Software.Services;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class MultiTestViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<DeviceViewModel> _devicesViewModel;

        public ObservableCollection<DeviceViewModel> DevicesViewModel
        {
            get => _devicesViewModel;
            private set
            {
                if (_devicesViewModel != value)
                {
                    _devicesViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        public MultiTestViewModel()
        {
            DevicesViewModel = new ObservableCollection<DeviceViewModel>(DeviceManager.Instance.GetAllDevices());
            DeviceManager.Instance.DeviceConnected += OnDeviceConnected;
            DeviceManager.Instance.DeviceDisconnected += OnDeviceDisconnected;
        }

        private void OnDeviceConnected(DeviceViewModel device)
        {
            if (!DevicesViewModel.Contains(device))
            {
                DevicesViewModel.Add(device);
            }
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (DevicesViewModel.Contains(device))
            {
                DevicesViewModel.Remove(device);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
