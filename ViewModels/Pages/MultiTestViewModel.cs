using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advanced_Dynotis_Software.Services;
using Advanced_Dynotis_Software.ViewModels.Device;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class MultiTestViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<DeviceViewModel> _devicesViewModel;

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
            DevicesViewModel = DeviceManager.Instance.GetAllDevices();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
