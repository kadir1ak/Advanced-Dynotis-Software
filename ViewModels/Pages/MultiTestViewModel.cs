using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advanced_Dynotis_Software.ViewModels.Device;
using Advanced_Dynotis_Software.Services;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class MultiTestViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DeviceViewModel> DevicesViewModel { get; set; }

        public MultiTestViewModel()
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
