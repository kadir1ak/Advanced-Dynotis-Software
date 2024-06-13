using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Advanced_Dynotis_Software.Services.Logger;

namespace Advanced_Dynotis_Software.Models.Dynotis
{
    public class Dynotis : INotifyPropertyChanged
    {
        public readonly SerialPort Port;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public event PropertyChangedEventHandler PropertyChanged;

        private string _portName;
        public string PortName
        {
            get => _portName;
            set
            {
                if (_portName != value)
                {
                    _portName = value;
                    OnPropertyChanged(nameof(PortName));
                }
            }
        }

        private string _mode;
        public string Mode
        {
            get => _mode;
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    OnPropertyChanged(nameof(Mode));
                }
            }
        }

        private string _model;
        public string Model
        {
            get => _model;
            set
            {
                if (_model != value)
                {
                    _model = value;
                    OnPropertyChanged(nameof(Model));
                }
            }
        }

        private string _seriNo;
        public string SeriNo
        {
            get => _seriNo;
            set
            {
                if (_seriNo != value)
                {
                    _seriNo = value;
                    OnPropertyChanged(nameof(SeriNo));
                }
            }
        }

        private SensorData _sensorData;
        public SensorData SensorData
        {
            get => _sensorData;
            set
            {
                if (_sensorData != value)
                {
                    _sensorData = value;
                    OnPropertyChanged(nameof(SensorData));
                }
            }
        }

        public Dynotis(string portName)
        {
            Port = new SerialPort(portName, 921600);
            _portName = portName;
            _sensorData = new SensorData();
        }

        public async Task OpenPortAsync()
        {
            if (Port != null && !Port.IsOpen)
            {
                try
                {
                    Port.Open();
                    _cancellationTokenSource = new CancellationTokenSource(); // CancellationTokenSource nesnesini yeniden oluştur
                    await StartReceivingDataAsync();
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to open port: {ex.Message}");
                }
            }
        }

        public async Task ClosePortAsync()
        {
            if (Port != null && Port.IsOpen)
            {
                try
                {
                    _cancellationTokenSource.Cancel();
                    Port.Close();
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to close port: {ex.Message}");
                }
            }
        }

        private async Task StartReceivingDataAsync()
        {
            await Task.Run(async () =>
            {
                await WaitForKeyMessageAsync(_cancellationTokenSource.Token);
            });
        }

        private async Task WaitForKeyMessageAsync(CancellationToken token)
        {
            try
            {
                bool deviceInfoReceived = false;

                while (!token.IsCancellationRequested && Port.IsOpen && !deviceInfoReceived)
                {
                    string indata = await Task.Run(() => Port.ReadLine(), token);
                    Logger.Log($"Received data: {indata}");

                    if (indata.Trim().StartsWith("KEY:"))
                    {
                        string[] keyParts = indata.Trim().Split(':');
                        if (keyParts.Length == 3)
                        {
                            Model = keyParts[1];
                            SeriNo = keyParts[2];

                            if (!string.IsNullOrEmpty(Model) && !string.IsNullOrEmpty(SeriNo))
                            {
                                deviceInfoReceived = true;
                                await Task.Run(() => Port.WriteLine("SENSOR_DATA"), token);
                                Mode = "SENSOR_DATA";
                                await DeviceDataReceivedAsync(token);
                            }
                        }
                    }

                    if (!deviceInfoReceived)
                    {
                        await Task.Run(() => Port.WriteLine("DEVICE_INFO"), token);
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred: {ex.Message}");
            }
        }

        private async Task DeviceDataReceivedAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && Port.IsOpen)
                {
                    string indata = await Task.Run(() => Port.ReadLine(), token);
                    Logger.Log($"Received data: {indata}");

                    if (!indata.Trim().StartsWith("KEY:"))
                    {
                        string[] dataParts = indata.Split(',');

                        if (dataParts.Length == 13)
                        {
                            var newData = new SensorData
                            {
                                Time = int.Parse(dataParts[0]),
                                Current = double.Parse(dataParts[1]),
                                Voltage = double.Parse(dataParts[2]),
                                Thrust = double.Parse(dataParts[3]),
                                Torque = double.Parse(dataParts[4]),
                                MotorSpeed = double.Parse(dataParts[5]),
                                VibrationX = double.Parse(dataParts[6]),
                                VibrationY = double.Parse(dataParts[7]),
                                VibrationZ = double.Parse(dataParts[8]),
                                AmbientTemp = double.Parse(dataParts[9]),
                                MotorTemp = double.Parse(dataParts[10]),
                                Pressure = double.Parse(dataParts[11]),
                                WindSpeed = double.Parse(dataParts[12])
                            };

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                SensorData = newData;

                                SensorData.Vibration = CalculateMagnitude(newData.VibrationX, newData.VibrationY, newData.VibrationZ);
                                SensorData.Power = newData.Current * newData.Voltage;
                                SensorData.WindDirection = 275 * newData.Voltage;
                                SensorData.AirDensity = 10.0 * newData.Voltage;
                            });
                        }
                        else
                        {
                            Logger.Log($"Unexpected data format: {indata}");
                        }
                    }

                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred: {ex.Message}");
            }
        }

        private static double CalculateMagnitude(double x, double y, double z)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
