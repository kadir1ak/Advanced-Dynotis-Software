using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Advanced_Dynotis_Software.Models.Dynotis;
using System.Management;
using System.Windows;


namespace Advanced_Dynotis_Software.ViewModels.Device
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private Dynotis device;
        public Dynotis Device
        {
            get => device;
            set
            {
                device = value;
                OnPropertyChanged(nameof(Device));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DeviceViewModel(string port)
        {
            Device = new Dynotis(port);
            Device.OpenPort();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
