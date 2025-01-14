using Advanced_Dynotis_Software.Models.Device.Sensors;
using Advanced_Dynotis_Software.Services.BindableBase;
using System;
using System.IO.Ports;
using System.Windows;

namespace Advanced_Dynotis_Software.Models.Device
{
    public class Device : BindableBase
    {
        public Device()
        {
            // Sensörleri başlat
            VoltageSensor = new Voltage();
            CurrentSensor = new Current();
            TorqueSensor = new Torque();
            ThrustSensor = new Thrust();
            VibrationSensor = new Vibration();
            MotorSpeedSensor = new MotorSpeed();
            SerialPort = new SerialPort();
        }

        // Sensör özellikleri
        public Voltage VoltageSensor { get; set; }
        public Current CurrentSensor { get; set; }
        public Torque TorqueSensor { get; set; }
        public Thrust ThrustSensor { get; set; }
        public Vibration VibrationSensor { get; set; }
        public MotorSpeed MotorSpeedSensor { get; set; }

        // Seri Port Özelliği
        private string _sampleCount;
        public string SampleCount
        {
            get => _sampleCount;
            set => SetProperty(ref _sampleCount, value);
        }

        private double sampleCount = 0;

        private DateTime lastUpdate = DateTime.Now;

        private string _portReadData;
        public string PortReadData
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to port: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (SerialPort == null || !SerialPort.IsOpen) return;

                string indata = SerialPort.ReadExisting();
                if (string.IsNullOrEmpty(indata)) return;

                string[] dataParts = indata.Split(',');
                if (dataParts.Length == 5 &&
                    double.TryParse(dataParts[0].Replace('.', ','), out double time) &&
                    double.TryParse(dataParts[1].Replace('.', ','), out double thrust) &&
                    double.TryParse(dataParts[2].Replace('.', ','), out double torque) &&
                    double.TryParse(dataParts[3].Replace('.', ','), out double current) &&
                    double.TryParse(dataParts[4].Replace('.', ','), out double voltage))
                {
                    // Sensör verilerini güncelle
                    if (Application.Current?.Dispatcher.CheckAccess() == true)
                    {
                        // UI iş parçacığında isek doğrudan çalıştır
                        UpdateData(indata, time, thrust, torque, current, voltage);
                        CalculateSampleRate();
                    }
                    else
                    {
                        // UI iş parçacığında değilsek Dispatcher kullan
                        Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            UpdateData(indata, time, thrust, torque, current, voltage);
                            CalculateSampleRate();
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Serial Port Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateData(string indata, double time, double thrust, double torque, double current, double voltage)
        {
            PortReadData = indata;
            PortReadTime = time;
            ThrustSensor.SetValue(thrust);
            TorqueSensor.SetValue(torque);
            CurrentSensor.SetValue(current);
            VoltageSensor.SetValue(voltage);
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
            if (SerialPort?.IsOpen == true)
            {
                SerialPort.Close();
                SerialPort.DataReceived -= SerialPort_DataReceived;
            }
        }
    }
}
