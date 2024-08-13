using Advanced_Dynotis_Software.Services.Logger;
using Irony.Parsing;
using LiveCharts.Wpf;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Advanced_Dynotis_Software.Models.Dynotis
{
    public class Dynotis : INotifyPropertyChanged, IDisposable
    {
        private readonly object _dataLock = new object();
        bool deviceInfoReceived = false;
        private ManualResetEventSlim _dataReceivedEvent = new ManualResetEventSlim(false);

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
                    await DeviceDataReceivedAsync(_cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to open port: {ex.Message}");
                    Error = $"Failed to open port: {ex.Message}";
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
                    Error = $"Failed to close port: {ex.Message}";
                }
            }
        }

        private async Task DeviceDataReceivedAsync(CancellationToken token)
        {
            try
            {
                // Cihaz Tanımalama Alanı
                while (!token.IsCancellationRequested && Port.IsOpen && !deviceInfoReceived)
                {
                    string indata = await Task.Run(() => Port.ReadExisting(), token);
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
                                await Task.Run(() => Port.WriteLine($"Device_Status:{Mode};ESC:{DynotisData.ESCValue};"), token);
                               // await WriteLineAsync(Port, $"Device_Status:{Mode};ESC:{DynotisData.ESCValue};", token);                               
                            }
                        }
                        else
                        {
                            deviceInfoReceived = false;
                        }
                    }
                    await Task.Delay(100);
                }
                // Cihazdan Sensör Verilerini Ayrıştırma Alanı
                while (!token.IsCancellationRequested && Port.IsOpen)
                {
                    string indata = await Task.Run(() => Port.ReadExisting(), token);
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
                                TransferStoredData(DynotisData, newData);

                                VibrationCalculations();

                                TheoreticalCalculations();

                                OnPropertyChanged(nameof(DynotisData));
                            }
                        });

                        await Task.Run(() => Port.WriteLine($"Device_Status:{Mode};ESC:{DynotisData.ESCValue};"), token);
                       // await WriteLineAsync(Port, $"Device_Status:{Mode};ESC:{DynotisData.ESCValue};", token);
                    }
                    await Task.Delay(1);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred: {ex.Message}");
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

        private void TransferStoredData(DynotisData currentData, DynotisData newData)
        {
            newData.PropellerDiameter = currentData.PropellerDiameter;
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

            newData.Vibration.VibrationX = newData.VibrationX;
            newData.Vibration.VibrationY = newData.VibrationY;
            newData.Vibration.VibrationZ = newData.VibrationZ;

            newData.Vibration.TareBufferCount = currentData.Vibration.TareBufferCount;

            newData.Vibration.TareVibration = currentData.Vibration.TareVibration;
            newData.Vibration.TareVibrationBuffer = currentData.Vibration.TareVibrationBuffer;

            newData.Vibration.TareVibrationX = currentData.Vibration.TareVibrationX;
            newData.Vibration.TareVibrationY = currentData.Vibration.TareVibrationY;
            newData.Vibration.TareVibrationZ = currentData.Vibration.TareVibrationZ;
            newData.Vibration.TareVibrationXBuffer = currentData.Vibration.TareVibrationXBuffer;
            newData.Vibration.TareVibrationYBuffer = currentData.Vibration.TareVibrationYBuffer;
            newData.Vibration.TareVibrationZBuffer = currentData.Vibration.TareVibrationZBuffer;

            newData.Vibration.HighVibration = currentData.Vibration.HighVibration;
            newData.Vibration.HighVibrationBuffer = currentData.Vibration.HighVibrationBuffer;
          
            DynotisData = newData;
        }

           
        private void TheoreticalCalculations()
        {
            try
            {
                // Propeller Area Calculation
                if (DynotisData.PropellerDiameter > 0)
                {
                    double diameterInMeters = DynotisData.PropellerDiameter * 0.0254;
                    DynotisData.Theoric.PropellerArea = Math.PI * Math.Pow(diameterInMeters / 2, 2);
                }

                // Rotational Speed Calculation
                if (DynotisData.MotorSpeed.Value > 0)
                {
                    DynotisData.Theoric.RotationalSpeed = 2.0 * Math.PI * DynotisData.MotorSpeed.Value / 60.0;
                }

                // Power Calculation
                DynotisData.Theoric.Power = DynotisData.Current * DynotisData.Voltage;

                // Air Density Calculation
                if (DynotisData.Pressure.Value > 0 && DynotisData.AmbientTemp.Value + DynotisData.TheoricVariables.KelvinConst > 0)
                {
                    DynotisData.Theoric.AirDensity = DynotisData.Pressure.Value /
                        (DynotisData.TheoricVariables.AirGasConstant * (DynotisData.AmbientTemp.Value + DynotisData.TheoricVariables.KelvinConst));
                }

                // Motor Efficiency Calculation
                if (DynotisData.Voltage > 0 && DynotisData.Current > 0)
                {
                    double voltageDrop = DynotisData.Current * (DynotisData.MotorInner / 1000);
                    double motorEfficiencyFactor = (1 - voltageDrop / DynotisData.Voltage) * (1 - DynotisData.NoLoadCurrents / DynotisData.Current);
                    DynotisData.Theoric.MotorEfficiency = motorEfficiencyFactor > 0 ? 100.0 * motorEfficiencyFactor : 0;
                }

                // Propeller Efficiency Calculation
                if (DynotisData.Theoric.AirDensity > 0 && DynotisData.Theoric.PropellerArea > 0 && DynotisData.Torque.Value > 0 && DynotisData.Theoric.RotationalSpeed > 0)
                {
                    double thrustFactor = Math.Sqrt(Math.Pow((DynotisData.Thrust.Value * 0.001 * 9.81), 3) /
                        (2 * DynotisData.Theoric.AirDensity * DynotisData.Theoric.PropellerArea));
                    DynotisData.Theoric.PropellerEfficiency = 100.0 * thrustFactor / ((DynotisData.Torque.Value / 1000.0) * DynotisData.Theoric.RotationalSpeed);
                }

                // PropSysEfficiencyI Calculation
                if (DynotisData.Theoric.MotorEfficiency > 0)
                {
                    DynotisData.Theoric.PropSysEfficiencyI = (DynotisData.Theoric.MotorEfficiency / 100.0) *
                        DynotisData.TheoricVariables.EscEffConst * DynotisData.Theoric.PropellerEfficiency;
                }

                // PropSysEfficiencyII Calculation
                if (DynotisData.Theoric.Power > 0)
                {
                    DynotisData.Theoric.PropSysEfficiencyII = DynotisData.Thrust.Value / DynotisData.Theoric.Power;
                }

                // IPS Calculation
                if (DynotisData.Vibration.Value > 0 && DynotisData.MotorSpeed.Value > 0)
                {
                    DynotisData.Theoric.IPS = (3685.1) * (DynotisData.Vibration.HighVibration) / (DynotisData.MotorSpeed.Value);
                }
                else
                {
                    DynotisData.Theoric.IPS = 0;
                }

                // J Calculation
                if (DynotisData.Theoric.AirDensity > 0 && DynotisData.PropellerDiameter > 0 && DynotisData.MotorSpeed.Value > 0)
                {
                    DynotisData.Theoric.J = (DynotisData.Theoric.AirDensity) / (DynotisData.MotorSpeed.Value / 60.0) * (DynotisData.PropellerDiameter * 0.0254);
                }

                // Ct Calculation   
                // Cq Calculation 
                if (DynotisData.Theoric.AirDensity > 0 && DynotisData.MotorSpeed.Value > 0 && DynotisData.PropellerDiameter > 0)
                {
                    double motorSpeedSquared = Math.Pow(DynotisData.MotorSpeed.Value / 60, 2);
                    double diameterToTheFourth = Math.Pow(DynotisData.PropellerDiameter * 0.0254, 4);
                    DynotisData.Theoric.Ct = (DynotisData.Thrust.Value * 9.81 * 0.001) /
                        (DynotisData.Theoric.AirDensity * motorSpeedSquared * diameterToTheFourth);

                    DynotisData.Theoric.Cq = (DynotisData.Torque.Value * 0.001) /
                        (DynotisData.Theoric.AirDensity * motorSpeedSquared * Math.Pow(DynotisData.PropellerDiameter * 0.0254, 5));
                    // Cp Calculation
                    DynotisData.Theoric.Cp = 2 * Math.PI * DynotisData.Theoric.Cq;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred during theoretical calculations: {ex.Message}");
            }
        }

        private void VibrationCalculations()
        {
            DynotisData.Vibration.Value = Math.Abs(DynotisData.VibrationY);
            DynotisData.Vibration.HighVibrationBuffer.Add(DynotisData.Vibration.Value);
            DynotisData.Vibration.TareVibrationBuffer.Add(DynotisData.Vibration.Value);

            DynotisData.Vibration.TareVibrationXBuffer.Add(DynotisData.Vibration.VibrationX);
            DynotisData.Vibration.TareVibrationYBuffer.Add(DynotisData.Vibration.VibrationY);
            DynotisData.Vibration.TareVibrationZBuffer.Add(DynotisData.Vibration.VibrationZ);

            DynotisData.Vibration.TareBufferCount++;

            if (DynotisData.Vibration.TareBufferCount > 25)
            {
                DynotisData.Vibration.TareBufferCount = 0;

                DynotisData.Vibration.HighVibration = CalculateHighVibrations(DynotisData.Vibration.HighVibrationBuffer);
                DynotisData.Vibration.HighVibrationBuffer.Clear();

                DynotisData.Vibration.TareVibration = CalculateAverage(DynotisData.Vibration.TareVibrationBuffer);
                DynotisData.Vibration.TareVibrationBuffer.Clear();

                DynotisData.Vibration.TareVibrationX = CalculateAverage(DynotisData.Vibration.TareVibrationXBuffer);
                DynotisData.Vibration.TareVibrationXBuffer.Clear();

                DynotisData.Vibration.TareVibrationY = CalculateAverage(DynotisData.Vibration.TareVibrationYBuffer);
                DynotisData.Vibration.TareVibrationYBuffer.Clear();

                DynotisData.Vibration.TareVibrationZ = CalculateAverage(DynotisData.Vibration.TareVibrationZBuffer);
                DynotisData.Vibration.TareVibrationZBuffer.Clear();
            }
        }

        private double CalculateAverage(List<double> buffer)
        {
            double sum = 0;
            foreach (var value in buffer)
            {
                sum += value;
            }
            return buffer.Count > 0 ? sum / buffer.Count : 0;
        }

        private double CalculateHighVibrations(List<double> buffer)
        {
            // Buffer'ı sırala ve en büyük 2 değeri al
            var topValues = buffer.OrderByDescending(x => x).Take(2);

            // En büyük 2 değerin ortalamasını hesapla
            return topValues.Average();
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
            _dataReceivedEvent?.Dispose();
        }
    }
}
