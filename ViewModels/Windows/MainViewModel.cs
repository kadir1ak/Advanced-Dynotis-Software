using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Advanced_Dynotis_Software.ViewModels.Device;
using Advanced_Dynotis_Software.Services;

namespace Advanced_Dynotis_Software.ViewModels.Windows
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DeviceViewModel> DevicesViewModel { get; set; }

        public MainViewModel()
        {
            DevicesViewModel = DeviceManager.Instance.GetAllDevices();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
