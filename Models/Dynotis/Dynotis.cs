﻿using Advanced_Dynotis_Software.Services.Logger;
using System;
using System.ComponentModel;
using System.Globalization;
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
        private readonly object _dataLock = new object();

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
                            DynotisData.ESCStatus = false;
                            DynotisData.ESCValue = 800;
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
                    //Logger.Log($"Received data: {indata}");

                    string[] dataParts = indata.Split(',');

                    if (dataParts.Length == 16)
                    {
                        var newData = new DynotisData
                        {
                            Time = TryParseDouble(dataParts[0], out double time) ? time : double.NaN,
                            Current = TryParseDouble(dataParts[1], out double current) ? current : double.NaN,
                            Voltage = TryParseDouble(dataParts[2], out double voltage) ? voltage : double.NaN,
                            Thrust = TryParseDouble(dataParts[3], out double thrust) ? new DynotisData.Unit(thrust, "Gram-force", "gf") : new DynotisData.Unit(double.NaN, "Unknown", "Unknown"),
                            Torque = TryParseDouble(dataParts[4], out double torque) ? new DynotisData.Unit(torque, "Newton millimeter", "N.mm") : new DynotisData.Unit(double.NaN, "Unknown", "Unknown"),
                            MotorSpeed = TryParseDouble(dataParts[5], out double motorSpeed) ? new DynotisData.Unit(motorSpeed, "Revolutions per minute", "RPM") : new DynotisData.Unit(double.NaN, "Unknown", "Unknown"),
                            WindSpeed = TryParseDouble(dataParts[6], out double windSpeed) ? new DynotisData.Unit(windSpeed, "Meters per second", "m/s") : new DynotisData.Unit(double.NaN, "Unknown", "Unknown"),
                            WindDirection = TryParseDouble(dataParts[7], out double windDirection) ? windDirection : double.NaN,
                            VibrationX = TryParseDouble(dataParts[8], out double vibrationX) ? vibrationX : double.NaN,
                            VibrationY = TryParseDouble(dataParts[9], out double vibrationY) ? vibrationY : double.NaN,
                            VibrationZ = TryParseDouble(dataParts[10], out double vibrationZ) ? vibrationZ : double.NaN,
                            AmbientTemp = TryParseDouble(dataParts[11], out double ambientTemp) ? new DynotisData.Unit(ambientTemp, "Celsius", "°C") : new DynotisData.Unit(double.NaN, "Unknown", "Unknown"),
                            MotorTemp = TryParseDouble(dataParts[12], out double motorTemp) ? new DynotisData.Unit(motorTemp, "Celsius", "°C") : new DynotisData.Unit(double.NaN, "Unknown", "Unknown"),
                            Temperature = TryParseDouble(dataParts[13], out double temp) ? new DynotisData.Unit(temp, "Celsius", "°C") : new DynotisData.Unit(double.NaN, "Unknown", "Unknown"),
                            Pressure = TryParseDouble(dataParts[14], out double pressure) ? new DynotisData.Unit(pressure, "Pascal", "Pa") : new DynotisData.Unit(double.NaN, "Unknown", "Unknown"),
                            Humidity = TryParseDouble(dataParts[15], out double humidity) ? humidity : double.NaN
                        };

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            lock (_dataLock)
                            {
                                var currentData = DynotisData;
                                newData.PropellerArea = currentData.PropellerArea;
                                newData.MotorInner = currentData.MotorInner;
                                newData.NoLoadCurrents = currentData.NoLoadCurrents;
                                newData.ESCValue = currentData.ESCValue;
                                newData.ESCStatus = currentData.ESCStatus;
                                newData.TestMode = currentData.TestMode;
                                newData.FileName = currentData.FileName;
                                newData.IsRecording = currentData.IsRecording;
                                newData.BatteryLevel = currentData.BatteryLevel;
                                newData.MaxCurrent = currentData.MaxCurrent;
                                newData.SecurityStatus = currentData.SecurityStatus;
                                newData.TareThrustValue = currentData.TareThrustValue;
                                newData.TareTorqueValue = currentData.TareTorqueValue;
                                newData.TareCurrentValue = currentData.TareCurrentValue;
                                newData.TareMotorSpeedValue = currentData.TareMotorSpeedValue;

                                newData.HighVibration = currentData.HighVibration;
                                newData.AVGVibration = currentData.AVGVibration;
                                newData.TareVibration = currentData.TareVibration;
                                newData.TareVibrationX = currentData.TareVibrationX;
                                newData.TareVibrationY = currentData.TareVibrationY;
                                newData.TareVibrationZ = currentData.TareVibrationZ;

                                newData.BufferCount = currentData.BufferCount;

                                newData.HighVibrationBuffer = currentData.HighVibrationBuffer;
                                newData.AVGVibrationBuffer = currentData.AVGVibrationBuffer;
                                newData.TareVibrationBuffer = currentData.TareVibrationBuffer;
                                newData.TareVibrationXBuffer = currentData.TareVibrationXBuffer;
                                newData.TareVibrationYBuffer = currentData.TareVibrationYBuffer;
                                newData.TareVibrationZBuffer = currentData.TareVibrationZBuffer;

                                DynotisData = newData;

                                //DynotisData.VibrationX = Math.Abs(newData.VibrationX);
                                //DynotisData.VibrationY = Math.Abs(newData.VibrationY);
                                //DynotisData.VibrationZ = Math.Abs(newData.VibrationZ);
                                //DynotisData.Vibration = newData.VibrationY;

                                DynotisData.Vibration = Math.Sqrt(Math.Pow(newData.VibrationY, 2));
                               

                                DynotisData.HighVibrationBuffer.Add(DynotisData.Vibration);

                                DynotisData.AVGVibrationBuffer.Add(DynotisData.Vibration);

                                DynotisData.TareVibrationBuffer.Add(DynotisData.Vibration);                                

                                DynotisData.TareVibrationXBuffer.Add(DynotisData.VibrationX);
                                DynotisData.TareVibrationYBuffer.Add(DynotisData.VibrationY);
                                DynotisData.TareVibrationZBuffer.Add(DynotisData.VibrationZ);

                                DynotisData.BufferCount++;
                                if (DynotisData.BufferCount > 100)
                                {
                                    DynotisData.BufferCount = 0;

                                    DynotisData.HighVibration = CalculateHighVibrations(DynotisData.HighVibrationBuffer);
                                    DynotisData.HighVibrationBuffer.Clear();

                                    DynotisData.AVGVibration = DynotisData.AVGVibrationBuffer.Sum() / DynotisData.AVGVibrationBuffer.Count;
                                    DynotisData.AVGVibrationBuffer.Clear();

                                    DynotisData.TareVibration = DynotisData.TareVibrationBuffer.Sum() / DynotisData.TareVibrationBuffer.Count;
                                    DynotisData.TareVibrationBuffer.Clear();

                                    DynotisData.TareVibrationX = DynotisData.TareVibrationXBuffer.Sum() / DynotisData.TareVibrationXBuffer.Count;
                                    DynotisData.TareVibrationXBuffer.Clear();

                                    DynotisData.TareVibrationY = DynotisData.TareVibrationYBuffer.Sum() / DynotisData.TareVibrationYBuffer.Count;
                                    DynotisData.TareVibrationYBuffer.Clear();

                                    DynotisData.TareVibrationZ = DynotisData.TareVibrationZBuffer.Sum() / DynotisData.TareVibrationZBuffer.Count;
                                    DynotisData.TareVibrationZBuffer.Clear();
                                }
                                

                                DynotisData.Power = newData.Current * newData.Voltage;
                                DynotisData.AirDensity = 10.0 * newData.Voltage;

                                OnPropertyChanged(nameof(DynotisData));
                            }
                        });


                        await WriteLineAsync(Port, $"Device_Status:{Mode};ESC:{DynotisData.ESCValue};", token);
                    }
                    else
                    {
                        //Logger.Log($"Unexpected data format: {indata}");
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
                return await Task.Run(() => port.ReadExisting(), token);
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
                //Logger.Log($"Transmit data: {message}");
            }
            catch (Exception ex)
            {
                Logger.Log($"WriteLine error: {ex.Message}");
                throw;
            }
        }

        public static bool TryParseDouble(string value, out double result)
        {
            // İlk olarak, '.' karakteri için deneme yapıyoruz.
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                return true;
            }

            // Eğer '.' karakteri başarısız olursa, ',' karakteri ile deneme yapıyoruz.
            return double.TryParse(value.Replace('.', ','), out result);
        }
        private double CalculateHighVibrations(List<double> buffer)
        {
            // Buffer'ı sırala ve en büyük 10 değeri al
            var topValues = buffer.OrderByDescending(x => x).Take(10);

            // En büyük 10 değerin ortalamasını hesapla
            double highVibrations = topValues.Average();

            return highVibrations;
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
