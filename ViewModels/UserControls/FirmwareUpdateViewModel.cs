using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{    public class FirmwareUpdateViewModel : INotifyPropertyChanged
    {

        private Dynotis _device;

        private string _bootloader_Mode;
        private string _mode;
        public FirmwareUpdateViewModel(Dynotis device)
        {
            _device = device;
            FirmwareUpdateCommand = new RelayCommand(param => FirmwareUpdate());
        }
        private void FirmwareUpdate()
        {
            // Mode = "6";
            // _device.Mode = "6";
            MessageBox.Show($"PortName:{_device.PortName};Mode:{_device.Mode};");
        }
        public string Bootloader_Mode
        {
            get => _bootloader_Mode;
            set
            {
                if (SetProperty(ref _bootloader_Mode, value))
                {
                    OnPropertyChanged(nameof(Bootloader_Mode));
                }
            }
        }
        public string Mode
        {
            get => _mode;
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    OnPropertyChanged(nameof(Mode));
                }
            }
        }
        public ICommand FirmwareUpdateCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
