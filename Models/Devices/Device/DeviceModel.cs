using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Advanced_Dynotis_Software.Models.Devices.Sensors;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.BindableBase;
using Advanced_Dynotis_Software.Services.Logger;
using Irony.Parsing;
using static Advanced_Dynotis_Software.Models.Devices.Sensors.Anemometer;

namespace Advanced_Dynotis_Software.Models.Devices.Device
{
    public class DeviceModel : BindableBase
    {
        public DeviceModel()
        {
            // Cihaz özelliklerini başlat
            CurrentSensor = new Current();
            VoltageSensor = new Voltage();
            TorqueSensor = new Torque();
            ThrustSensor = new Thrust();
            MotorSpeedSensor = new MotorSpeed();
            AnemometerSensor = new Anemometer();
            VibrationSensor = new Vibration();
            EnvironmentalConditions = new EnvironmentConditions();
            ESCSensor = new ESC();
            Info = new DeviceInfo();
            SerialPort = new SerialPort();
        }

        // Cihaz özellikleri
        public Current CurrentSensor { get; set; }
        public Voltage VoltageSensor { get; set; }
        public Thrust ThrustSensor { get; set; }
        public Torque TorqueSensor { get; set; }
        public MotorSpeed MotorSpeedSensor { get; set; }
        public Anemometer AnemometerSensor { get; set; }
        public Vibration VibrationSensor { get; set; }
        public EnvironmentConditions EnvironmentalConditions { get; set; }
        public ESC ESCSensor { get; set; }
        public DeviceInfo Info { get; set; }

        // Seri Port Özelliği
        private string _sampleCount;
        public string SampleCount
        {
            get => _sampleCount;
            set => SetProperty(ref _sampleCount, value);
        }

        private double sampleCount = 0;

        private DateTime lastUpdate = DateTime.Now;

        private string _portReadMessag;
        public string PortReadMessage
        {
            get => _portReadMessag;
            set => SetProperty(ref _portReadMessag, value);
        }      
        
        private PortData _portReadData;
        public PortData PortReadData
        {
            get => _portReadData;
            set => SetProperty(ref _portReadData, value);
        }

        private double _portReadTime = 0;
        public double PortReadTime
        {
            get => _portReadTime;
            set => SetProperty(ref _portReadTime, value);
        }

        private SerialPort _serialPort;
        public SerialPort SerialPort
        {
            get => _serialPort;
            set
            {
                if (_serialPort != value)
                {
                    _serialPort = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task SerialPortConnect(string portName)
        {
            try
            {
                // Mevcut seri port açıksa kapat
                StopSerialPort();

                // Yeni seri port ayarları
                SerialPort = new SerialPort
                {
                    PortName = portName,
                    BaudRate = 921600,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                SerialPort.DataReceived += SerialPort_DataReceived;
                SerialPort.Open();
                Logger.LogInfo($"Serial port {portName} successfully opened.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error connecting to port {portName}.", ex);
            }
        }
        private StringBuilder dataBuffer = new StringBuilder();
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (SerialPort == null || !SerialPort.IsOpen) return;

                string indata = SerialPort.ReadExisting();
                if (string.IsNullOrEmpty(indata)) return;

                dataBuffer.Append(indata);

                // Mesajın tamamlanıp tamamlanmadığını kontrol et (örnek: '\n' ile biten mesajlar)
                if (dataBuffer.ToString().Contains("\n"))
                {
                    string[] messages = dataBuffer.ToString().Split('\n');
                    for (int i = 0; i < messages.Length - 1; i++)
                    {
                        string message = messages[i].Trim();
                        if (!Info.DeviceIdentified)
                        {
                            _ = Task.Run(() => IdentifyDevice(message));
                        }
                        else
                        {
                            ProcessSensorData(message);
                        }
                    }
                    // Buffer'daki kalan veriyi sakla
                    dataBuffer.Clear();
                    dataBuffer.Append(messages[^1]);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error processing serial port data.", ex);
            }
        }

        private async Task IdentifyDevice(string indata)
        {
            try
            {
                // Veri formatını kontrol etmek için regex
                string devicePattern = @"^[^;]+;[^;]+;[^;]+;[^;]+;[^;]+;[^;]+$";
                if (!Regex.IsMatch(indata, devicePattern))
                {
                    Logger.LogError($"Invalid device data received: {indata}");
                    return;
                }

                // Cihaz bilgilerini ayır
                string[] infoParts = indata.Split(';');
                if (infoParts.Length == 6)
                {
                    // Şirket adı, cihaz adı, model adı, üretim tarihi, model numarası ve firmware versiyonu bilgilerini al
                    Info.CompanyName = infoParts[0].Trim();
                    Info.DeviceName = infoParts[1].Trim();
                    Info.Model = infoParts[2].Trim();
                    Info.ManufactureDate = infoParts[3].Trim();
                    Info.Model = infoParts[4].Trim();
                    Info.FirmwareVersion = infoParts[5].Trim();
                    Info.Mode = "2";
                    // Cihaz tanımlandı olarak işaretle
                    Info.DeviceIdentified = true;
                    Logger.LogInfo($"Device identified: {Info.DeviceName} ({Info.Model})");
                 
                    // Cihaza gönderilen mesaj taslağı: Device_Status:2;ESC:800;
                    await StartSendingMessagesAsync();
                }
                else
                {
                    Info.DeviceIdentified = false;
                    //throw new Exception("Cihaz bilgileri eksik veya hatalı.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error identifying device with data: {indata}", ex);
            }
        }

        private CancellationTokenSource _cancellationTokenSource;

        private async Task StartSendingMessagesAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                while (!Info.DeviceIdentified) // Cihaz tanımlanana kadar döngü devam eder
                {
                    if (SerialPort != null && SerialPort.IsOpen)
                    {
                        // Cihaza mesaj gönder
                        SerialPort.WriteLine($"Device_Status:{2};ESC:{800};");
                    }

                    // 10 ms bekle
                    await Task.Delay(10, token);
                }
            }
            catch (TaskCanceledException)
            {
                // Görev iptal edildiğinde buraya düşer, ekstra işlem yapmaya gerek yok
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mesaj Gönderme Hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopSendingMessages()
        {
            // Mesaj gönderimini durdurmak için token iptal edilir
            _cancellationTokenSource?.Cancel();
        }

        private void ProcessSensorData(string indata)
        {
            try
            {
                string[] dataParts = indata.Split(',');

                if (dataParts.Length != 16)
                {
                    Logger.LogError($"Invalid sensor data format: {indata}");
                    return;
                }

                // Sensör verisini doğrula ve ayrıştır
                if (dataParts.Length == 16 &&
                    double.TryParse(dataParts[0].Replace('.', ','), out double time) &&
                    double.TryParse(dataParts[1].Replace('.', ','), out double current) &&
                    double.TryParse(dataParts[2].Replace('.', ','), out double voltage) &&
                    double.TryParse(dataParts[3].Replace('.', ','), out double thrust) &&
                    double.TryParse(dataParts[4].Replace('.', ','), out double torque) &&
                    double.TryParse(dataParts[5].Replace('.', ','), out double motorSpeed) &&
                    double.TryParse(dataParts[6].Replace('.', ','), out double windSpeed) &&
                    double.TryParse(dataParts[7].Replace('.', ','), out double windDirection) &&
                    double.TryParse(dataParts[8].Replace('.', ','), out double vibrationX) &&
                    double.TryParse(dataParts[9].Replace('.', ','), out double vibrationY) &&
                    double.TryParse(dataParts[10].Replace('.', ','), out double vibrationZ) &&
                    double.TryParse(dataParts[11].Replace('.', ','), out double ambientTemp) &&
                    double.TryParse(dataParts[12].Replace('.', ','), out double motorTemp) &&
                    double.TryParse(dataParts[13].Replace('.', ','), out double temperature) &&
                    double.TryParse(dataParts[14].Replace('.', ','), out double pressure) &&
                    double.TryParse(dataParts[15].Replace('.', ','), out double humidity))
                {

                    var newData = new PortData
                    {
                        Time = time,
                        Current = current,
                        Voltage = voltage,
                        Thrust = thrust,
                        Torque = torque,
                        MotorSpeed = motorSpeed,
                        WindSpeed = windSpeed,
                        WindDirection = windDirection,
                        VibrationX = vibrationX,
                        VibrationY = vibrationY,
                        VibrationZ = vibrationZ,
                        AmbientTemp = ambientTemp,
                        MotorTemp = motorTemp,
                        Temperature = temperature,
                        Pressure = pressure,
                        Humidity = humidity
                    };


                    // UI iş parçacığında çalıştır
                    if (Application.Current?.Dispatcher.CheckAccess() == true)
                    {
                        UpdateData(indata, newData);
                        CalculateSampleRate();
                    }
                    else
                    {
                        // UI iş parçacığında değilse Dispatcher kullan
                        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            UpdateData(indata, newData);
                            CalculateSampleRate();
                        }));
                    }
                }
                else
                {
                    Logger.LogError($"Error parsing sensor data: {indata}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error processing sensor data: {indata}", ex);
            }
        }

        private void UpdateData(string indata,PortData newData)
        {
            // Verileri güncelle
            PortReadMessage = indata;
            PortReadTime = newData.Time;
            CurrentSensor.SetValue(newData.Current);
            VoltageSensor.SetValue(newData.Voltage);
            ThrustSensor.SetValue(newData.Thrust);
            TorqueSensor.SetValue(newData.Torque);
            MotorSpeedSensor.SetValue(newData.MotorSpeed);
            AnemometerSensor.WindSpeedSensor.SetValue(newData.WindSpeed);
            AnemometerSensor.WindDirectionSensor.SetValue(newData.WindDirection);
            VibrationSensor.SetVibrationValues(newData.VibrationX, newData.VibrationY, newData.VibrationZ);
            EnvironmentalConditions.AmbientTempSensor.SetValue(newData.AmbientTemp);
            EnvironmentalConditions.MotorTempSensor.SetValue(newData.MotorTemp);
            EnvironmentalConditions.TemperatureSensor.SetValue(newData.Temperature);
            EnvironmentalConditions.PressureSensor.SetValue(newData.Pressure);
            EnvironmentalConditions.HumiditySensor.SetValue(newData.Humidity);
        }

        private void CalculateSampleRate()
        {
            sampleCount++;
            var now = DateTime.Now;
            var elapsed = now - lastUpdate;

            if (elapsed.TotalSeconds >= 1) // Her saniyede bir hesapla
            {
                SampleCount = $"Saniyedeki veri örnekleme hızı: {sampleCount} Hz";
                sampleCount = 0;
                lastUpdate = now;
            }
        }
        public void StopSerialPort()
        {
            try
            {
                if (SerialPort?.IsOpen == true)
                {
                    SerialPort.Close();
                    SerialPort.DataReceived -= SerialPort_DataReceived;
                }
            }
            catch (Exception) { }
        }
        public class DeviceInfo : BindableBase
        {
            private string _id;
            public string ID
            {
                get => _id;
                set => SetProperty(ref _id, value);
            }

            private string _companyName;
            public string CompanyName
            {
                get => _companyName;
                set => SetProperty(ref _companyName, value);
            }

            private string _deviceName;
            public string DeviceName
            {
                get => _deviceName;
                set => SetProperty(ref _deviceName, value);
            }

            private string _manufactureDate;
            public string ManufactureDate
            {
                get => _manufactureDate;
                set => SetProperty(ref _manufactureDate, value);
            }

            private string _portName;
            public string PortName
            {
                get => _portName;
                set => SetProperty(ref _portName, value);
            }

            private string _model;
            public string Model
            {
                get => _model;
                set => SetProperty(ref _model, value);
            }

            private string _seriNo;
            public string SeriNo
            {
                get => _seriNo;
                set => SetProperty(ref _seriNo, value);
            }

            private string _firmwareVersion;
            public string FirmwareVersion
            {
                get => _firmwareVersion;
                set => SetProperty(ref _firmwareVersion, value);
            }

            private string _mode;
            public string Mode
            {
                get => _mode;
                set => SetProperty(ref _mode, value);
            }

            private string _connectionStatus;
            public string ConnectionStatus
            {
                get => _connectionStatus;
                set => SetProperty(ref _connectionStatus, value);
            } 
            
            private bool _deviceIdentified;
            public bool DeviceIdentified
            {
                get => _deviceIdentified;
                set => SetProperty(ref _deviceIdentified, value);
            }
        }
        public class PortData
        {
            public double Time { get; set; }
            public double Current { get; set; }
            public double Voltage { get; set; }
            public double Thrust { get; set; }
            public double Torque { get; set; }
            public double MotorSpeed { get; set; }
            public double WindSpeed { get; set; }
            public double WindDirection { get; set; }
            public double VibrationX { get; set; }
            public double VibrationY { get; set; }
            public double VibrationZ { get; set; }
            public double AmbientTemp { get; set; }
            public double MotorTemp { get; set; }
            public double Temperature { get; set; }
            public double Pressure { get; set; }
            public double Humidity { get; set; }
        }

    }
}
