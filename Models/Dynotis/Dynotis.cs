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
                            DynotisData.RecordStatus = true;
                            DynotisData.ESCValue = 800;
                            DynotisData.BatteryLevel = 1;
                            DynotisData.MaxCurrent = 0;
                            DynotisData.SecurityStatus = false;

                            if (!string.IsNullOrEmpty(Model) && !string.IsNullOrEmpty(SeriNo))
                            {
                                deviceInfoReceived = true;
                                Mode = "2";
                                await Task.Run(() => Port.WriteLine($"Device_Status:{Mode};ESC:{DynotisData.ESCValue};"), token);                         
                            }
                        }
                        else
                        {
                            deviceInfoReceived = false;
                        }
                    }
                    await Task.Delay(100);
                }
                // Cihazdan Gelen Sensör Verilerini Ayrıştırma Alanı
                while (!token.IsCancellationRequested && Port.IsOpen)
                {
                    // Test için mod seçiniz
                    Mode = "5";
                    await Task.Run(() => Port.WriteLine($"Device_Status:{Mode};ESC:{DynotisData.ESCValue};"), token);
                    switch (Mode)
                    {
                        case "2":
                            await Dynotis_Mod_2(token);
                            break;
                        case "5":
                            await Dynotis_Mod_5(token);
                            break;
                        default:
                            Logger.Log("Unknown test mode.");
                            break;
                    }
                    await Task.Delay(1);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred: {ex.Message}");
            }
        }
        private async Task Dynotis_Mod_2(CancellationToken token)
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
            }
        }
        private async Task Dynotis_Mod_5(CancellationToken token)
        {
            string indata = await Task.Run(() => Port.ReadExisting(), token);
            string[] dataParts = indata.Split(',');
            if (dataParts.Length == 15)
            {

                var newData = new DynotisData
                {
                    Time = TryParseDouble(dataParts[0], out double time) ? time : double.NaN,
                    MotorSpeed = TryParseDouble(dataParts[1], out double motorSpeed) ? new DynotisData.Unit(motorSpeed, "Revolutions per minute", "RPM") : new DynotisData.Unit(double.NaN, "Unknown", "Unknown"),
                    VibrationY = TryParseDouble(dataParts[2], out double vibrationY) ? vibrationY : double.NaN,
                };

                
                DynotisData.VibrationDynamicBalancer360[0] = TryParseDouble(dataParts[3], out double vibrationY0) ? vibrationY0 : double.NaN;
                DynotisData.VibrationDynamicBalancer360[1] = TryParseDouble(dataParts[4], out double vibrationY30) ? vibrationY30 : double.NaN;
                DynotisData.VibrationDynamicBalancer360[2] = TryParseDouble(dataParts[5], out double vibrationY60) ? vibrationY60 : double.NaN;
                DynotisData.VibrationDynamicBalancer360[3] = TryParseDouble(dataParts[6], out double vibrationY90) ? vibrationY90 : double.NaN;
                DynotisData.VibrationDynamicBalancer360[4] = TryParseDouble(dataParts[7], out double vibrationY120) ? vibrationY120 : double.NaN;
                DynotisData.VibrationDynamicBalancer360[5] = TryParseDouble(dataParts[8], out double vibrationY150) ? vibrationY150 : double.NaN; ;
                DynotisData.VibrationDynamicBalancer360[6] = TryParseDouble(dataParts[9], out double vibrationY180) ? vibrationY180 : double.NaN;
                DynotisData.VibrationDynamicBalancer360[7] = TryParseDouble(dataParts[10], out double vibrationY210) ? vibrationY210 : double.NaN;
                DynotisData.VibrationDynamicBalancer360[8] = TryParseDouble(dataParts[11], out double vibrationY240) ? vibrationY240 : double.NaN;
                DynotisData.VibrationDynamicBalancer360[9] = TryParseDouble(dataParts[12], out double vibrationY270) ? vibrationY270 : double.NaN;
                DynotisData.VibrationDynamicBalancer360[10] = TryParseDouble(dataParts[13], out double vibrationY300) ? vibrationY300 : double.NaN;
                DynotisData.VibrationDynamicBalancer360[11] = TryParseDouble(dataParts[14], out double vibrationY330) ? vibrationY330 : double.NaN;
                

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
            newData.RecordStatus = currentData.RecordStatus;
            newData.DynamicBalancerStatus = currentData.DynamicBalancerStatus;
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


            /*
             * 
            if ((newData.VibrationX + newData.VibrationY + newData.VibrationZ)>100) 
            {
                newData.DynamicBalancerStatus = true;
                newData.VibrationX = newData.VibrationX - 100;
                newData.VibrationY = newData.VibrationY - 100;
                newData.VibrationZ = newData.VibrationZ - 100;
            }
            else
            {
                newData.DynamicBalancerStatus = false;
            }

            */
            double temp = DynotisData.VibrationDynamicBalancer360.Sum();
            if ((temp) > 100)
            {
                newData.DynamicBalancerStatus = true;
          
                for (int i = 0; i < DynotisData.VibrationDynamicBalancer360.Length; i++)
                {
                    newData.VibrationDynamicBalancer360[i] = Math.Abs(DynotisData.VibrationDynamicBalancer360[i] - 100);
                }
               
            }
            else
            {
                newData.DynamicBalancerStatus = false;
            }

            newData.Vibration.VibrationX = newData.VibrationX;
            newData.Vibration.VibrationY = newData.VibrationY;
            newData.Vibration.VibrationZ = newData.VibrationZ;

            newData.Vibration.TareBufferCount = currentData.Vibration.TareBufferCount;

            newData.Vibration.TareVibration = currentData.Vibration.TareVibration;
            newData.Vibration.TareVibrationBuffer = currentData.Vibration.TareVibrationBuffer;

            newData.Vibration.TareVibrationX = currentData.Vibration.TareVibrationX;
            newData.Vibration.TareVibrationY = currentData.Vibration.TareVibrationY;
            newData.Vibration.TareVibrationZ = currentData.Vibration.TareVibrationZ;

            newData.Vibration.TareCurrentVibration = currentData.Vibration.TareCurrentVibration;

            newData.Vibration.TareVibrationXBuffer = currentData.Vibration.TareVibrationXBuffer;
            newData.Vibration.TareVibrationYBuffer = currentData.Vibration.TareVibrationYBuffer;
            newData.Vibration.TareVibrationZBuffer = currentData.Vibration.TareVibrationZBuffer;

            newData.Vibration.HighVibration = currentData.Vibration.HighVibration;
            newData.Vibration.HighIPSVibration = currentData.Vibration.HighIPSVibration;
            newData.Vibration.HighVibrationBuffer = currentData.Vibration.HighVibrationBuffer;
            newData.Vibration.HighIPSVibrationBuffer = currentData.Vibration.HighIPSVibrationBuffer;
          
            DynotisData = newData;
        }


        /*
            @@@@@@@        SABİTLER        @@@@@@@
            Air_Gas_Constant = 287.058;  //Individual Gas Constant - R Unit: [J/kg K]
            Esc_Eff_Const = 0.95;
            @@@@@@@      STM'DEN GELEN VERİ PAKETİ      @@@@@@@
            Motor Speed: RPM
            Thrust: gf
            Torque: Nmm
            Voltage: V
            Current: A
            Pressure: Pa
            Ambient Temperature: °C
            Motor Temperature: °C
            Vibration: g
            @@@@@@@        ARAYÜZDEN OKUNAN PARAMETRELER        @@@@@@@
            Propeller Diameter: inch
            Motor Internal Resistance: ohm
            Motor No Load Current: A
            Battery Cell: S
            Torque: Sensor Data Unit N.mm
            Thrust: Sensor Data Unit gf
            Motor Speed: Sensor Data Unit RPM
            Temperature: Sensor Data Unit °C
            Speed: Sensor Data Unit m/s 
            Pressure: Sensor Data Unit Pa 
            Power : Sensor Data Unit W
            Voltage : Sensor Data Unit V
            Current : Sensor Data Unit A
            Air Density : Sensor Data Unit kg/m³
            Wind Speed: Sensor Data Unit m/s 
            Wind Direction: Sensor Data Unit Degre
            Ambient Temperature: Sensor Data Unit °C
            Motor Temperature: Sensor Data Unit °C
            Vibration : Sensor Data Unit g      
         */
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
                else
                {
                    DynotisData.Theoric.PropellerArea = 0;
                }

                // Rotational Speed Calculation
                if (DynotisData.MotorSpeed.Value > 0)
                {
                    DynotisData.Theoric.RotationalSpeed = 2.0 * Math.PI * DynotisData.MotorSpeed.Value / 60.0;
                }
                else
                {
                    DynotisData.Theoric.RotationalSpeed = 0;
                }

                // Power Calculation
                DynotisData.Theoric.Power = DynotisData.Current * DynotisData.Voltage;

                // Air Density Calculation
                if (DynotisData.Pressure.Value > 0 && DynotisData.AmbientTemp.Value + DynotisData.TheoricVariables.KelvinConst > 0)
                {
                    DynotisData.Theoric.AirDensity = DynotisData.Pressure.Value /
                        (DynotisData.TheoricVariables.AirGasConstant * (DynotisData.AmbientTemp.Value + DynotisData.TheoricVariables.KelvinConst));
                }
                else
                {
                    DynotisData.Theoric.AirDensity = 0;
                }

                // Motor Efficiency Calculation
                if (DynotisData.Voltage > 0 && DynotisData.Current > 0)
                {
                    double voltageDrop = DynotisData.Current * (DynotisData.MotorInner / 1000);
                    double motorEfficiencyFactor = (1 - voltageDrop / DynotisData.Voltage) * (1 - DynotisData.NoLoadCurrents / DynotisData.Current);
                    DynotisData.Theoric.MotorEfficiency = motorEfficiencyFactor > 0 ? 100.0 * motorEfficiencyFactor : 0;
                }
                else
                {
                    DynotisData.Theoric.MotorEfficiency = 0;
                }

                // Figure of Merit (FOM)
                if (DynotisData.Theoric.Cp > 0)
                {
                    DynotisData.Theoric.FOM = Math.Pow(DynotisData.Theoric.Ct, 3 / 2) / Math.Sqrt(2) * DynotisData.Theoric.Cp;
                }
                else
                {
                    DynotisData.Theoric.FOM = 0;
                }

                // Propeller Efficiency Calculation
                if (DynotisData.Theoric.AirDensity > 0 && DynotisData.Theoric.PropellerArea > 0 && DynotisData.Torque.Value > 0 && DynotisData.Theoric.RotationalSpeed > 0)
                {
                    double thrustFactor = Math.Sqrt(Math.Pow((DynotisData.Thrust.Value * 0.001 * 9.81), 3) /
                        (2 * DynotisData.Theoric.AirDensity * DynotisData.Theoric.PropellerArea));
                    DynotisData.Theoric.PropellerEfficiency = 100.0 * thrustFactor / ((DynotisData.Torque.Value / 1000.0) * DynotisData.Theoric.RotationalSpeed);
                }
                else
                {
                    DynotisData.Theoric.PropellerEfficiency = 0;
                }

                // PropSysEfficiencyI Calculation
                if (DynotisData.Theoric.MotorEfficiency > 0)
                {
                    DynotisData.Theoric.PropSysEfficiencyI = (DynotisData.Theoric.MotorEfficiency / 100.0) *
                        DynotisData.TheoricVariables.EscEffConst * DynotisData.Theoric.PropellerEfficiency;
                }
                else
                {
                    DynotisData.Theoric.PropSysEfficiencyI = 0;
                }

                // PropSysEfficiencyII Calculation
                if (DynotisData.Theoric.Power > 0)
                {
                    DynotisData.Theoric.PropSysEfficiencyII = DynotisData.Thrust.Value / DynotisData.Theoric.Power;
                }
                else
                {
                    DynotisData.Theoric.PropSysEfficiencyII = 0;
                }

                // IPS Calculation
                if (DynotisData.MotorSpeed.Value > 0)
                {
                    DynotisData.Theoric.IPS = Math.Min(1.5, (3685.1) * (DynotisData.Vibration.HighVibration) / (DynotisData.MotorSpeed.Value));
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
                else
                {
                    DynotisData.Theoric.J = 0;
                }

                // Ct Calculation   
                // Cq Calculation 
                // Cp Calculation
                if (DynotisData.Theoric.AirDensity > 0 && DynotisData.MotorSpeed.Value > 0 && DynotisData.PropellerDiameter > 0)
                {
                    double motorSpeedSquared = Math.Pow(DynotisData.MotorSpeed.Value / 60, 2);
                    double diameterToTheFourth = Math.Pow(DynotisData.PropellerDiameter * 0.0254, 4);
                    DynotisData.Theoric.Ct = (DynotisData.Thrust.Value * 9.81 * 0.001) /
                        (DynotisData.Theoric.AirDensity * motorSpeedSquared * diameterToTheFourth);

                    DynotisData.Theoric.Cq = (DynotisData.Torque.Value * 0.001) /
                        (DynotisData.Theoric.AirDensity * motorSpeedSquared * Math.Pow(DynotisData.PropellerDiameter * 0.0254, 5));

                    DynotisData.Theoric.Cp = 2 * Math.PI * DynotisData.Theoric.Cq;
                }
                else
                {
                    DynotisData.Theoric.Ct = 0;
                    DynotisData.Theoric.Cq = 0;
                    DynotisData.Theoric.Cp = 0;
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

            DynotisData.Vibration.HighVibrationBuffer.Add(Math.Abs(DynotisData.Vibration.Value - DynotisData.Vibration.TareCurrentVibration));
            DynotisData.Vibration.HighIPSVibrationBuffer.Add(DynotisData.Theoric.IPS);

            DynotisData.Vibration.TareVibrationBuffer.Add(DynotisData.Vibration.Value);

            DynotisData.Vibration.TareVibrationXBuffer.Add(DynotisData.Vibration.VibrationX);
            DynotisData.Vibration.TareVibrationYBuffer.Add(DynotisData.Vibration.VibrationY);
            DynotisData.Vibration.TareVibrationZBuffer.Add(DynotisData.Vibration.VibrationZ);

            DynotisData.Vibration.TareBufferCount++;

            int dataPointsPerRevolution = (int)(60.0 / DynotisData.MotorSpeed.Value);

            // Değeri 10 ile 1000 arasında sınırlandır
            dataPointsPerRevolution = Math.Max(10, Math.Min(dataPointsPerRevolution, 1000));

            if (DynotisData.Vibration.TareBufferCount >= dataPointsPerRevolution)
            {
                DynotisData.Vibration.TareBufferCount = 0;

                DynotisData.Vibration.HighVibration = CalculateHighVibrations(DynotisData.Vibration.HighVibrationBuffer);
                DynotisData.Vibration.HighVibrationBuffer.Clear(); 
                
                DynotisData.Vibration.HighIPSVibration = CalculateHighVibrations(DynotisData.Vibration.HighIPSVibrationBuffer);
                DynotisData.Vibration.HighIPSVibrationBuffer.Clear();

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
