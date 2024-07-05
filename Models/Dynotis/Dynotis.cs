using Advanced_Dynotis_Software.Services.Logger;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Advanced_Dynotis_Software.Models.Dynotis
{
    public class Dynotis : INotifyPropertyChanged, IDisposable
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
                    OnPropertyChanged();
                }
            }
        }

        private string _error;
        public string Error
        {
            get => _error;
            set
            {
                if (_error != value)
                {
                    _error = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _workingStatus;
        public string WorkingStatus
        {
            get => _workingStatus;
            set
            {
                if (_workingStatus != value)
                {
                    _workingStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _connectionStatus;
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _firmware;
        public string Firmware
        {
            get => _firmware;
            set
            {
                if (_firmware != value)
                {
                    _firmware = value;
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                }
            }
        }

        private DynotisData _dynotisData;
        public DynotisData DynotisData
        {
            get => _dynotisData;
            set
            {
                if (_dynotisData != value)
                {
                    _dynotisData = value;
                    OnPropertyChanged();
                }
            }
        }

        public Dynotis(string portName)
        {
            Port = new SerialPort(portName, 921600)
            {
                Encoding = Encoding.UTF8,
                NewLine = "\r\n"
            };
            _portName = portName;
            _dynotisData = new DynotisData();
        }

        public async Task OpenPortAsync()
        {
            if (Port != null && !Port.IsOpen)
            {
                try
                {
                    Port.Open();
                    if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource = new CancellationTokenSource();
                    }
                    await StartReceivingDataAsync();
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to open port: {ex.Message}");
                    Error = $"Failed to open port: {ex.Message}"; // Kullanıcıya hata mesajı gösterme
                }
            }
        }

        public async Task ClosePortAsync()
        {
            if (Port != null && Port.IsOpen)
            {
                try
                {
                    _cancellationTokenSource?.Cancel();
                    Port.Close();
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to close port: {ex.Message}");
                    Error = $"Failed to close port: {ex.Message}"; // Kullanıcıya hata mesajı gösterme
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
                    string indata = await ReadLineAsync(Port, token);
                    Logger.Log($"Received data: {indata}");

                    if (indata.Contains("Semai Aviation Ltd."))
                    {
                        string[] parts = indata.Split(';');
                        if (parts.Length >= 2)
                        {
                            Model = parts[1];
                            SeriNo = parts[2];
                            Firmware = parts[4];
                            Error = "Null";
                            WorkingStatus = "Unknown";
                            ConnectionStatus = "True";
                            DynotisData.ESCStatus = true;
                            DynotisData.ESCValue = 0;
                            DynotisData.BatteryLevel = 1;
                            DynotisData.MaxCurrent = 0;
                            DynotisData.SecurityStatus = false;

                            if (!string.IsNullOrEmpty(Model) && !string.IsNullOrEmpty(SeriNo))
                            {
                                deviceInfoReceived = true;
                                Mode = "2";
                                await WriteLineAsync(Port, $"Device_Status:{Mode};ESC:{DynotisData.ESCValue};", token);
                                await DeviceDataReceivedAsync(token);
                            }
                        }
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
                    string indata = await ReadLineAsync(Port, token);
                    Logger.Log($"Received data: {indata}");

                    string[] dataParts = indata.Split(',');

                    if (dataParts.Length == 15)
                    {
                        var newData = new DynotisData
                        {
                            Time = ParseDouble(dataParts[0]),
                            Current = ParseDouble(dataParts[1]),
                            Voltage = ParseDouble(dataParts[2]),
                            Thrust = ParseDouble(dataParts[3]),
                            Torque = ParseDouble(dataParts[4]),
                            MotorSpeed = ParseDouble(dataParts[5]),
                            WindSpeed = ParseDouble(dataParts[6]),
                            VibrationX = ParseDouble(dataParts[7]),
                            VibrationY = ParseDouble(dataParts[8]),
                            VibrationZ = ParseDouble(dataParts[9]),
                            AmbientTemp = ParseDouble(dataParts[10]),
                            MotorTemp = ParseDouble(dataParts[11]),
                            Temperature = ParseDouble(dataParts[12]),
                            Pressure = ParseDouble(dataParts[13]),
                            Humidity = ParseDouble(dataParts[14])
                        };

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            var currentData = DynotisData;
                            newData.PropellerArea = currentData.PropellerArea;
                            newData.MotorInner = currentData.MotorInner;
                            newData.NoLoadCurrents = currentData.NoLoadCurrents;
                            newData.ESCValue = currentData.ESCValue;
                            newData.ESCStatus = currentData.ESCStatus;
                            newData.TestMode = currentData.TestMode;
                            newData.SaveFile = currentData.SaveFile;
                            newData.SaveStatus = currentData.SaveStatus;
                            newData.BatteryLevel = currentData.BatteryLevel;
                            newData.MaxCurrent = currentData.MaxCurrent;
                            newData.SecurityStatus = currentData.SecurityStatus;

                            DynotisData = newData;

                            DynotisData.Vibration = CalculateMagnitude(newData.VibrationX, newData.VibrationY, newData.VibrationZ);
                            DynotisData.Power = newData.Current * newData.Voltage;
                            DynotisData.WindDirection = 275 * newData.Voltage;
                            DynotisData.AirDensity = 10.0 * newData.Voltage;
                        });

                        await WriteLineAsync(Port, $"Device_Status:{Mode};ESC:{DynotisData.ESCValue};", token);
                    }
                    else
                    {
                        Logger.Log($"Unexpected data format: {indata}");
                    }

                    await Task.Delay(1);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred: {ex.Message}");
            }
        }

        private async Task<string> ReadLineAsync(SerialPort port, CancellationToken token)
        {
            try
            {
                return await Task.Run(() => port.ReadLine(), token);
            }
            catch (Exception ex)
            {
                Logger.Log($"ReadLine error: {ex.Message}");
                throw;
            }
        }

        private async Task WriteLineAsync(SerialPort port, string message, CancellationToken token)
        {
            try
            {
                await Task.Run(() => port.WriteLine(message), token);
                Logger.Log($"Transmit data: {message}");
            }
            catch (Exception ex)
            {
                Logger.Log($"WriteLine error: {ex.Message}");
                throw;
            }
        }

        private static double CalculateMagnitude(double x, double y, double z)
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
        }

        private static double ParseDouble(string value)
        {
            if (double.TryParse(value, out double result))
            {
                return result;
            }
            return double.NaN;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (Port.IsOpen)
            {
                Port.Close();
            }
            Port.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
