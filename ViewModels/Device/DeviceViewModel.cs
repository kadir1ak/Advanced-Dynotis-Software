using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Advanced_Dynotis_Software.Models.Dynotis;
using System.Management;
using System.Windows;
using LiveCharts.Wpf;
using LiveCharts;


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
        public DeviceViewModel(string portName)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Device = new Dynotis(portName);
                Device.OpenPort();
            }
            else
            {
                // Tasarım zamanı için örnek veri
                //Device = new Dynotis("DesignModePort");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
