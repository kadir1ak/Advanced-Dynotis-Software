using Advanced_Dynotis_Software.Services.Logger;
using Irony.Parsing;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        private string _binFilePath;
        public string BinFilePath
        {
            get => _binFilePath;
            set
            {
                if (_binFilePath != value)
                {
                    _binFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _bootloader_Mode;
        public string Bootloader_Mode
        {
            get => _bootloader_Mode;
            set
            {
                if (_bootloader_Mode != value)
                {
                    _bootloader_Mode = value;
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

        private string _firmwareVersion;
        public string FirmwareVersion
        {
            get => _firmwareVersion;
            set
            {
                if (_firmwareVersion != value)
                {
                    _firmwareVersion = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _firmwareUpdateStatus;
        public string FirmwareUpdateStatus
        {
            get => _firmwareUpdateStatus;
            set
            {
                if (_firmwareUpdateStatus != value)
                {
                    _firmwareUpdateStatus = value;
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
            Mode = "0";
            Bootloader_Mode = "0";
            FirmwareUpdateStatus = "0";
            FirmwareVersion = "v.1.1.1";
            PortName = portName;
            DynotisData = new DynotisData();
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
        public async Task SendCommandAsync(string command)
        {
            Port.WriteLine(command);
        }

        public async Task<string> ReceiveBootloaderResponseAsync()
        {
            return await Task.Run(() => Port.ReadLine());
        }

        public async Task SendDataAsync(byte[] data)
        {
            await Task.Run(() => Port.Write(data, 0, data.Length));
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

                            OnPropertyChanged(nameof(DynotisData));

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
                    else if (indata.Contains("Bootloader_Mode"))
                    {
                        string[] dataParts = indata.Split(':'); // Bootloader_Mode kısmını ayır
                        if (dataParts.Length >= 2)
                        {
                            string modeAndVersion = dataParts[1]; // Bootloader_Mode:0+v.1.1.1 -> "0+v.1.1.1"
                            string[] modeVersionParts = modeAndVersion.Split('+'); // "0+v.1.1.1" -> ["0", "v.1.1.1"]

                            if (modeVersionParts.Length >= 2)
                            {
                                // Bootloader_Mode bilgilerini al
                                string bootloaderMode = modeVersionParts[0]; // "0"
                                // Gelen değeri kontrol et
                                if (int.TryParse(bootloaderMode, out int modeValue))
                                {
                                    Bootloader_Mode = bootloaderMode;
                                }
                            }
                        }

                        if (Bootloader_Mode == "0")
                        {
                            if (FirmwareUpdateStatus == "0") // Kontrol et
                            {
                                // Firmware Version bilgilerini al
                                if (dataParts.Length >= 2)
                                {
                                    string modeAndVersion = dataParts[1]; // Bootloader_Mode:0+v.1.1.1 -> "0+v.1.1.1"
                                    string[] modeVersionParts = modeAndVersion.Split('+'); // "0+v.1.1.1" -> ["0", "v.1.1.1"]

                                    if (modeVersionParts.Length >= 2)
                                    {
                                        // Bootloader_Mode ve versiyon bilgilerini al
                                        string bootloaderMode = modeVersionParts[0]; // "0"
                                        string version = modeVersionParts[1]; // "v.1.1.1Bootloader_Mode" gibi hatalı olabilir

                                        // Versiyon bilgilerini Regex kullanarak doğru formatta çek
                                        var versionMatch = System.Text.RegularExpressions.Regex.Match(version, @"v\.(\d+)\.(\d+)\.(\d+)");

                                        if (versionMatch.Success)
                                        {
                                            // Version bilgilerini ayıkla
                                            string major = versionMatch.Groups[1].Value; // Major version
                                            string minor = versionMatch.Groups[2].Value; // Minor version
                                            string svnCommit = versionMatch.Groups[3].Value; // Commit number

                                            // Bootloader_Mode değerini ayarla
                                            Bootloader_Mode = bootloaderMode;

                                            // Firmware versiyonu kontrolü
                                            string trimmedFirmwareVersion = FirmwareVersion.Trim();
                                            string receivedVersion = ("v." + major + "." + minor + "." + svnCommit).Trim();

                                            FirmwareUpdateStatus = (trimmedFirmwareVersion == receivedVersion) ? "1" : "2";
                                            if (FirmwareUpdateStatus == "1") // Uygun, "GO" komutu gönder
                                            {
                                                await Task.Run(() => Port.WriteLine($"GO"), token);
                                                Port.Close(); // Portu kapat
                                            }
                                            else if (FirmwareUpdateStatus == "2") // Uygun değil, güncelleme modu
                                            {
                                                await Task.Run(() => Port.WriteLine($"UPDATE_MODE"), token);
                                            }
                                            // Firmware Version bilgisi cihaza gitsin.
                                            await Task.Run(() => Port.WriteLine(FirmwareVersion), token);

                                        }
                                    }
                                }
                            }
                            else if(FirmwareUpdateStatus == "1") // Uygun, "GO" komutu gönder
                            { 
                                await Task.Run(() => Port.WriteLine($"GO"), token);
                                Port.Close(); // Portu kapat
                            }
                        }
                        else if (Bootloader_Mode == "1")
                        {
                            // bin dosyasının boyutunu ilet
                            string binFilePath = @"D:\ST\Dynotis-ST-Firmware\Dynotis-ST-Firmware-Device\Debug\Dynotis-ST-Firmware-Device.bin";

                            // Dosya boyutunu al
                            FileInfo binFileInfo = new FileInfo(binFilePath);
                            int binFileSize = (int)binFileInfo.Length; // Byte cinsinden dosya boyutu
                            // Dosya boyutunu cihaza gönder
                            await Task.Run(() => Port.WriteLine(binFileSize.ToString()), token);
                            //await Task.Run(() => Port.WriteLine($"105696"), token);
                        }
                        else if (Bootloader_Mode == "2")
                        {
                            // binFilePath değişkenini tanımla
                            //D:\ST\Dynotis-ST-Firmware\Dynotis-ST-Firmware-Device\Debug
                            string binFilePath = @"D:\ST\Dynotis-ST-Firmware\Dynotis-ST-Firmware-Device\Debug\Dynotis-ST-Firmware-Device.bin";
                            // .bin dosyasını cihaza gönder
                            await SendFirmwareAsync(binFilePath, token);
                        }
                    }

                    // Version bilgilerini kontrol edebilirsin
                   // MessageBox.Show($"Bootloader_Mode:{Bootloader_Mode} \r\n FirmwareUpdateStatus:{FirmwareUpdateStatus}");

                    await Task.Delay(100);
                }
                // Cihazdan Gelen Sensör Verilerini Ayrıştırma Alanı
                while (!token.IsCancellationRequested && Port.IsOpen)
                {
                    // Test için mod seçiniz
                    // Mode = "6";
                    await Task.Run(() => Port.WriteLine($"Device_Status:{Mode};ESC:{DynotisData.ESCValue};"), token);
                    switch (Mode)
                    {
                        case "2":
                            await Dynotis_Mod_2(token);
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

        public async Task SendFirmwareAsync(string binFilePath, CancellationToken token)
        {
            try
            {
                if (!File.Exists(binFilePath))
                {
                    Logger.Log("Bin file does not exist at the specified path.");
                    return;
                }

                byte[] firmwareData = await File.ReadAllBytesAsync(binFilePath, token);
                int totalSize = firmwareData.Length;
                int bufferSize = 256;
                int totalChunks = (int)Math.Ceiling((double)totalSize / bufferSize);

                Logger.Log($"Firmware size: {totalSize} bytes. Sending in {totalChunks} chunks.");

                for (int i = 0; i < totalChunks; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger.Log("Firmware sending was cancelled.");
                        break;
                    }

                    int currentChunkSize = Math.Min(bufferSize, totalSize - (i * bufferSize));
                    byte[] buffer = new byte[currentChunkSize];
                    Array.Copy(firmwareData, i * bufferSize, buffer, 0, currentChunkSize);

                    Port.Write(buffer, 0, currentChunkSize);
                    await Task.Delay(1, token);
                    Logger.Log($"Chunk {i + 1}/{totalChunks} sent.");
                }

                Logger.Log("Firmware update completed.");
                Mode = "0";
                Bootloader_Mode = "0";
                FirmwareUpdateStatus = "0";
            }
            catch (Exception ex)
            {
                Logger.Log($"An error occurred while sending firmware: {ex.Message}");
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

            newData.BalancerParameterMotor = currentData.BalancerParameterMotor;           
            newData.BalancerParameterBasePropeller = currentData.BalancerParameterBasePropeller;           
            newData.BalancerParameterFirstBladePropeller = currentData.BalancerParameterFirstBladePropeller;          
            newData.BalancerParameterSecondBladePropeller = currentData.BalancerParameterSecondBladePropeller;          
            newData.BalancerParameterBalancedPropeller = currentData.BalancerParameterBalancedPropeller;       

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


            newData.Vibration.VibrationX = newData.VibrationX;
            newData.Vibration.VibrationY = newData.VibrationY;
            newData.Vibration.VibrationZ = newData.VibrationZ;

            newData.Vibration.TareBufferCount = currentData.Vibration.TareBufferCount;
            newData.Vibration.IPSBufferCount = currentData.Vibration.IPSBufferCount;
            newData.Vibration.BufferCount = currentData.Vibration.BufferCount;

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
        private const int WindowSize = 25; // kayan pencere boyutu örneğin 25

        private void VibrationCalculations()
        {
            // Yeni değeri al
            DynotisData.Vibration.Value = Math.Abs(DynotisData.VibrationY);

            // HighVibrationBuffer'a yeni değeri ekle
            DynotisData.Vibration.HighVibrationBuffer.Add(DynotisData.Vibration.Value);

            // Eğer pencere boyutunu aştıysa en eski veriyi kaldır
            if (DynotisData.Vibration.HighVibrationBuffer.Count > WindowSize)
            {
                DynotisData.Vibration.HighVibrationBuffer.RemoveAt(0);
            }

            // Her seferinde RMS hesapla (isterseniz daha seyrek de hesaplayabilirsiniz)
            //DynotisData.Vibration.HighVibration = CalculateRMS(DynotisData.Vibration.HighVibrationBuffer);
            DynotisData.Vibration.HighVibration = CalculateHighVibrations(DynotisData.Vibration.HighVibrationBuffer);

            // Aşağıda diğer işlemler aynı kalabilir.
            // Örnek: TareVibration, TareVibrationX, Y, Z bufferlarını da aynı mantıkta kayan pencere yapabilirsiniz.
            // Eğer bu bufferlar da kayan pencere ile takip edilecekse, benzer yaklaşımı orada da kullanın.

            DynotisData.Vibration.TareVibrationBuffer.Add(DynotisData.Vibration.Value);
            if (DynotisData.Vibration.TareVibrationBuffer.Count > WindowSize)
                DynotisData.Vibration.TareVibrationBuffer.RemoveAt(0);

            DynotisData.Vibration.TareVibrationXBuffer.Add(DynotisData.Vibration.VibrationX);
            if (DynotisData.Vibration.TareVibrationXBuffer.Count > WindowSize)
                DynotisData.Vibration.TareVibrationXBuffer.RemoveAt(0);

            DynotisData.Vibration.TareVibrationYBuffer.Add(DynotisData.Vibration.VibrationY);
            if (DynotisData.Vibration.TareVibrationYBuffer.Count > WindowSize)
                DynotisData.Vibration.TareVibrationYBuffer.RemoveAt(0);

            DynotisData.Vibration.TareVibrationZBuffer.Add(DynotisData.Vibration.VibrationZ);
            if (DynotisData.Vibration.TareVibrationZBuffer.Count > WindowSize)
                DynotisData.Vibration.TareVibrationZBuffer.RemoveAt(0);

            // Motor hızıyla ilgili kısım olduğu gibi kalabilir.
            DynotisData.Vibration.IPSBufferCount++;

            if (DynotisData.MotorSpeed.Value != 0)
            {
                DynotisData.Vibration.HighIPSVibrationBuffer.Add(DynotisData.Theoric.IPS);

                int dataPointsPerRevolution = (int)(60.0 / DynotisData.MotorSpeed.Value);
                dataPointsPerRevolution = Math.Max(10, Math.Min(dataPointsPerRevolution, 1000));

                if (DynotisData.Vibration.IPSBufferCount >= dataPointsPerRevolution)
                {
                    DynotisData.Vibration.IPSBufferCount = 0;
                    DynotisData.Vibration.HighIPSVibration = CalculateRMS(DynotisData.Vibration.HighIPSVibrationBuffer);
                    DynotisData.Vibration.HighIPSVibrationBuffer.Clear();
                }
            }
        }

        // RMS Hesaplama Fonksiyonu Örneği
        private double CalculateRMS(List<double> values)
        {
            if (values.Count == 0)
                return 0.0;

            double sumOfSquares = 0.0;
            foreach (var v in values)
            {
                sumOfSquares += v * v;
            }

            double mean = sumOfSquares / values.Count;
            return Math.Sqrt(mean);
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

        private double CalculatePeakToPeak(List<double> buffer)
        {
            // Maksimum ve minimum değerleri bul
            double max = buffer.Max();
            double min = buffer.Min();

            // Tepe noktalar arasındaki farkı hesapla
            return max - min;
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
