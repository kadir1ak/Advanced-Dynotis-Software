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
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Device = new Dynotis(port);
                Device.OpenPort();
                Device.devicePortsEvent += DevicePortsEvent;
            }
            else
            {
                // Tasarım zamanı için örnek veri
                //Device = new Dynotis("DesignModePort");
            }
        }
        public async void DevicePortsEvent()
        {
            try
            {
                // Sorun yaşanan portu kapat
                Device.ClosePort();

                // Belirli bir süre sonra portu tekrar açmaya çalış
                await Task.Delay(1000);

                // Cihazı tekrar aç ve güncelle
                Device.OpenPort();

                // Eğer hala bağlantı sorunu devam ediyorsa, kullanıcıya bilgi ver
                if (!Device.Port.IsOpen)
                {
                    // MessageBox.Show("DevicePortsEvent");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed DevicePortsEvent: {ex.Message}");
 
            }

        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
