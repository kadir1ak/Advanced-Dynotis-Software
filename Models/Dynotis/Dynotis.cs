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
        public readonly SerialPort Port;
        private CancellationTokenSource _cancellationTokenSource;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _portName;
        public string PortName
        {
            get => _portName;
            set
            {
                _portName = value;
                OnPropertyChanged(nameof(PortName));
            }
        }

        private string _mode;
        public string Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                OnPropertyChanged(nameof(Mode));
            }
        }

        private string _model;
        public string Model
        {
            get => _model;
            set
            {
                _model = value;
                OnPropertyChanged(nameof(Model));
            }
        }

        private string _seriNo;
        public string SeriNo
        {
            get => _seriNo;
            set
            {
                _seriNo = value;
                OnPropertyChanged(nameof(SeriNo));
            }
        }

        private SensorData _sensorData;
        public SensorData SensorData
        {
            get => _sensorData;
            set
            {
                _sensorData = value;
                OnPropertyChanged(nameof(SensorData));
            }
        }

        public Dynotis(string portName)
        {
            _portName = portName;
            _mode = "DEVICE_INFO";
            Port = new SerialPort(portName, 921600);
            _sensorData = new SensorData();
            _cancellationTokenSource = new CancellationTokenSource();
        }

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

        public void ClosePort()
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
                    MessageBox.Show($"Failed to close port: {ex.Message}");
                }
            }
        }

        private async void StartReceivingData()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                await WaitForKeyMessage(_cancellationTokenSource.Token);
            });
        }
        private async Task WaitForKeyMessage(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (Port.IsOpen)
                    {
                        string indata = await Task.Run(() => Port.ReadLine());

                        if (indata.Trim().StartsWith("KEY:") && _mode != "SENSOR_DATA")
                        {
                            string[] keyParts = indata.Trim().Split(':');
                            if (keyParts.Length == 3)
                            {
                                Model = keyParts[1];
                                SeriNo = keyParts[2];

                                await Task.Run(() => Port.WriteLine("SENSOR_DATA"));
                                Mode = "SENSOR_DATA";
                                await DeviceDataReceived(token);
                            }
                        }
                        else if (_mode == "DEVICE_INFO")
                        {
                            await Task.Run(() => Port.WriteLine("DEVICE_INFO"));
                        }
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private async Task DeviceDataReceived(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (Port.IsOpen && _mode == "SENSOR_DATA")
                    {
                        string indata = await Task.Run(() => Port.ReadLine());

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
                                    Humidity = double.Parse(dataParts[12])
                                };

                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    SensorData = newData;
                                });
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
