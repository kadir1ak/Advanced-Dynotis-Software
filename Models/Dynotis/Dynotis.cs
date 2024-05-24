using System.ComponentModel;
using System.IO.Ports;
using System.Windows;

namespace Advanced_Dynotis_Software.Models.Dynotis
{
    public class Dynotis : INotifyPropertyChanged
    {
        public string PortName { get; set; } // Cihazın adı
        public string DeviceMode { get; set; } // Cihazın modu
        public string Model { get; set; } // Cihazın modeli
        public string SeriNo { get; set; } // Cihazın seri numarası
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

        public Dynotis(string portName)
        {
            PortName = portName;
            DeviceMode = "DEVICE_INFO";
            Port = new SerialPort(portName, 921600); // 921600 baud hızında seri port oluştur
            SensorData = new SensorData();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        // Veri alımını başlatır
        public void StartReceivingData()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => WaitForKeyMessage(_cancellationTokenSource.Token));
        }

        // KEY mesajını bekler ve DEVICE_INFO mesajını gönderir
        private async Task WaitForKeyMessage(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (Port.IsOpen)
                    {
                        // Seri porttan satır okuma işlemi
                        string indata = await Task.Run(() => Port.ReadLine());

                        // Gelen mesajı kontrol et
                        if (indata.Trim().StartsWith("KEY:"))
                        {
                            // KEY mesajını ayrıştırma
                            string[] keyParts = indata.Trim().Split(':');
                            if (keyParts.Length == 3)
                            {
                                Model = keyParts[1];
                                SeriNo = keyParts[2];

                                // SENSOR_DATA mesajını gönder
                                await Task.Run(() => Port.WriteLine("SENSOR_DATA"));
                                DeviceMode = "SENSOR_DATA";
                                // Sensör verilerini almaya başla
                                await DeviceDataReceived(token);
                            }
                        }
                        if(DeviceMode == "DEVICE_INFO")
                        {
                            // DEVICE_INFO mesajını gönder
                            await Task.Run(() => Port.WriteLine("DEVICE_INFO"));
                        }
                    }

                    await Task.Delay(1); // Kısa bir gecikme
                }
            }
            catch (Exception ex)
            {
                devicePortsEvent?.Invoke();
            }
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
                        if (DeviceMode == "DEVICE_INFO")
                        {
                            // DEVICE_INFO mesajını gönder
                            await Task.Run(() => Port.WriteLine("DEVICE_INFO"));
                        }
                        else if (DeviceMode == "SENSOR_DATA")
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
                        }
                    }

                    await Task.Delay(10); // 10ms gecikme, 100Hz frekansı
                }
            }
            catch (Exception ex)
            {
                devicePortsEvent?.Invoke();
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
