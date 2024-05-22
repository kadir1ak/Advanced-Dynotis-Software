using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Advanced_Dynotis_Software.Models.Dynotis
{
    public class Dynotis : INotifyPropertyChanged
    {
        public string Name { get; set; } // Cihazın adı
        public SerialPort Port { get; set; } // Seri port bağlantısı

        private CancellationTokenSource _cancellationTokenSource; // Veri alımını iptal etmek için kullanılan token kaynağı

        public event Action devicePortsEvent;

        private SensorData sensorData;
        public SensorData SensorData
        {
            get => sensorData;
            set
            {
                sensorData = value;
                OnPropertyChanged(nameof(SensorData)); // SensorData değiştiğinde UI'ı bilgilendir
            }
        }

        public Dynotis(string name)
        {
            Name = name;
            Port = new SerialPort(name, 921600); // 921600 baud hızında seri port oluştur
            SensorData = new SensorData();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        // Veri alımını başlatır
        public void StartReceivingData()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => DeviceDataReceived(_cancellationTokenSource.Token)); // Veri alımı için yeni bir görev başlatır
        }

        // Asenkron veri alım işlemi
        private async Task DeviceDataReceived(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (Port.IsOpen)
                    {
                        // Seri porttan satır okuma işlemi
                        string indata = await Task.Run(() => Port.ReadLine());

                        // Gelen veriyi virgülle ayırma
                        string[] dataParts = indata.Split(',');

                        // Beklenen tüm veri parçalarının alındığından emin olma
                        if (dataParts.Length == 6)
                        {
                            // Her parçayı ayrıştırma ve sensör verilerini güncelleme
                            var newData = new SensorData
                            {
                                Time = int.Parse(dataParts[0]),
                                AmbientTemp = int.Parse(dataParts[1]),
                                MotorTemp = int.Parse(dataParts[2]),
                                MotorSpeed = int.Parse(dataParts[3]),
                                Thrust = int.Parse(dataParts[4]),
                                Torque = int.Parse(dataParts[5])
                            };

                            // UI güncellemesini tek seferde yapma
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                SensorData = newData;
                            });
                        }
                        else
                        {
                            // Beklenen formatta veri gelmediyse hata fırlatma
                            //MessageBox.Show("sayı hatası");
                            //throw new Exception("Received data does not match the expected format.");
                        }
                    }

                    await Task.Delay(1); // 1000Hz frekansına uymak için gecikme
                }
            }
            catch (Exception ex)
            {
                // Hataları kullanıcıya gösterme
                //App.Current.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
                devicePortsEvent.Invoke();
            }
        }

        // Seri portu açar ve veri alımını başlatır
        public void OpenPort()
        {
            if (Port != null && !Port.IsOpen)
            {
                try
                {
                    Port.Open();
                    StartReceivingData();
                    //MessageBox.Show("(OpenPort): " + Port.PortName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open port: {ex.Message}");
                }
            }
        }

        // Seri portu kapatır ve veri alımını durdurur
        public void ClosePort()
        {
            if (Port != null && Port.IsOpen)
            {
                try
                {
                    _cancellationTokenSource.Cancel(); // Veri alımını iptal et
                    Port.Close();
                    //MessageBox.Show("(ClosePort): " + Port.PortName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to close port: {ex.Message}");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // Property değişikliklerini bildir
        }
    }
}
