using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.ViewModels.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.ViewModels.Windows
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DeviceViewModel> DevicesViewModel { get; }

        public MainWindowViewModel()
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
