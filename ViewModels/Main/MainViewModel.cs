using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advanced_Dynotis_Software.ViewModels.Main;
using Advanced_Dynotis_Software.Services;

namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DeviceViewModel> DevicesViewModel { get; }

        public MainViewModel()
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
                OnPropertyChanged(nameof(DevicesViewModel));
            }
        }

        private void OnDeviceDisconnected(DeviceViewModel device)
        {
            if (DevicesViewModel.Contains(device))
            {
                DevicesViewModel.Remove(device);
                OnPropertyChanged(nameof(DevicesViewModel));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
