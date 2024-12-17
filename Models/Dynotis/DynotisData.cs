using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Advanced_Dynotis_Software.Models.Dynotis.DynotisData;
using static InterfaceVariables;

namespace Advanced_Dynotis_Software.Models.Dynotis
{
    public class DynotisData : INotifyPropertyChanged
    {
        public struct Unit
        {
            public double Value { get; set; }
            public string UnitName { get; set; }
            public string UnitSymbol { get; set; }

            public Unit(double value, string unitName, string unitSymbol)
            {
                Value = value;
                UnitName = unitName;
                UnitSymbol = unitSymbol;
            }
        }

        public DynotisData()
        {
            _vibrationVariables = new VibrationVariables();
            _theoricVariables = new TheoricVariables();
        }

        private double _time;
        private double _sampleRate;
        private Unit _ambientTemp;
        private Unit _motorTemp;
        private Unit _motorSpeed;
        private Unit _thrust;
        private Unit _torque;
        private double _voltage;
        private double _current;
        private Unit _temperature;
        private Unit _pressure;
        private double _humidity;
        private double _vibrationX;
        private double _vibrationY;
        private double _vibrationZ;

        private Unit _windSpeed;
        private double _windDirection;
        private double _propellerDiameter;
        private double _motorInner;
        private double _noLoadCurrents;
        private double _maxCurrent;
        private double _batteryLevel;
        private bool _securityStatus;
        private int _escValue;
        private bool _escStatus;
        private bool _dynamicBalancerStatus;


        private double[] _vibrationDynamicBalancer360 = new double[12];

        private VibrationVariables _vibrationVariables;
        private TheoricVariables _theoricVariables;

        private bool _recordStatus;

        private string _pageName;

        private string _testMode;
        private bool _isRecording;
        private string _fileName;
        private TimeSpan _duration;



        private double _tareTorqueValue;
        private double _tareThrustValue;
        private double _tareCurrentValue;
        private double _tareMotorSpeedValue;
        public double TareMotorSpeedValue
        {
            get => _tareMotorSpeedValue;
            set => SetProperty(ref _tareMotorSpeedValue, value);
        }
        public double TareCurrentValue
        {
            get => _tareCurrentValue;
            set => SetProperty(ref _tareCurrentValue, value);
        }
        public double TareThrustValue
        {
            get => _tareThrustValue;
            set => SetProperty(ref _tareThrustValue, value);
        }

        public double TareTorqueValue
        {
            get => _tareTorqueValue;
            set => SetProperty(ref _tareTorqueValue, value);
        }
        public double Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        public double SampleRate
        {
            get => _sampleRate;
            set => SetProperty(ref _sampleRate, value);
        }

        public Unit AmbientTemp
        {
            get => _ambientTemp;
            set => SetProperty(ref _ambientTemp, value);
        }

        public Unit MotorTemp
        {
            get => _motorTemp;
            set => SetProperty(ref _motorTemp, value);
        }

        public Unit MotorSpeed
        {
            get => _motorSpeed;
            set => SetProperty(ref _motorSpeed, value);
        }

        public Unit Thrust
        {
            get => _thrust;
            set => SetProperty(ref _thrust, value);
        }

        public Unit Torque
        {
            get => _torque;
            set => SetProperty(ref _torque, value);
        }

        public double Voltage
        {
            get => _voltage;
            set => SetProperty(ref _voltage, value);
        }

        public double Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }
        public Unit Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }
        public Unit Pressure
        {
            get => _pressure;
            set => SetProperty(ref _pressure, value);
        }
        public double Humidity
        {
            get => _humidity;
            set => SetProperty(ref _humidity, value);
        }
        public double VibrationX
        {
            get => _vibrationX;
            set => SetProperty(ref _vibrationX, value);
        }

        public double VibrationY
        {
            get => _vibrationY;
            set => SetProperty(ref _vibrationY, value);
        }

        public double VibrationZ
        {
            get => _vibrationZ;
            set => SetProperty(ref _vibrationZ, value);
        }

        public Unit WindSpeed
        {
            get => _windSpeed;
            set => SetProperty(ref _windSpeed, value);
        }

        public double WindDirection
        {
            get => _windDirection;
            set => SetProperty(ref _windDirection, value);
        }


        public double PropellerDiameter
        {
            get => _propellerDiameter;
            set => SetProperty(ref _propellerDiameter, value);
        }

        public double MotorInner
        {
            get => _motorInner;
            set => SetProperty(ref _motorInner, value);
        }

        public double NoLoadCurrents
        {
            get => _noLoadCurrents;
            set => SetProperty(ref _noLoadCurrents, value);
        }

        public double MaxCurrent
        {
            get => _maxCurrent;
            set => SetProperty(ref _maxCurrent, value);
        }

        public double BatteryLevel
        {
            get => _batteryLevel;
            set => SetProperty(ref _batteryLevel, value);
        }

        public bool SecurityStatus
        {
            get => _securityStatus;
            set => SetProperty(ref _securityStatus, value);
        }

        public int ESCValue
        {
            get => _escValue;
            set => SetProperty(ref _escValue, value);
        }

        public bool ESCStatus
        {
            get => _escStatus;
            set => SetProperty(ref _escStatus, value);
        }     
        public bool DynamicBalancerStatus
        {
            get => _dynamicBalancerStatus;
            set => SetProperty(ref _dynamicBalancerStatus, value);
        }

        public VibrationVariables Vibration
        {
            get => _vibrationVariables;
            set => SetProperty(ref _vibrationVariables, value);
        }     
        public TheoricVariables Theoric
        {
            get => _theoricVariables;
            set => SetProperty(ref _theoricVariables, value);
        }

        public bool IsRecording
        {
            get => _isRecording;
            set => SetProperty(ref _isRecording, value);
        }  
        public bool RecordStatus
        {
            get => _recordStatus;
            set => SetProperty(ref _recordStatus, value);
        }

        public string PageName
        {
            get => _pageName;
            set => SetProperty(ref _pageName, value);
        }
        public string TestMode
        {
            get => _testMode;
            set => SetProperty(ref _testMode, value);
        }
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }
        public TimeSpan Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public class VibrationVariables : INotifyPropertyChanged
        {
            public VibrationVariables()
            {
                _tareBufferCount = 0;

                _tareVibrationBuffer = new List<double>();
                _tareVibrationXBuffer = new List<double>();
                _tareVibrationYBuffer = new List<double>();
                _tareVibrationZBuffer = new List<double>();

                _highVibrationBuffer = new List<double>();
                _highIPSVibrationBuffer = new List<double>();
            }

            private double _value;

            private double _vibrationX;
            private double _vibrationY;
            private double _vibrationZ;

            private int _tareBufferCount;
            private double _tareVibration;
            private double _tareCurrentVibration;
            private List<double> _tareVibrationBuffer;
            private double _tareVibrationX;
            private List<double> _tareVibrationXBuffer;
            private double _tareVibrationY;
            private List<double> _tareVibrationYBuffer;
            private double _tareVibrationZ;
            private List<double> _tareVibrationZBuffer;

            private double _highVibration;
            private double _highIPSVibration;
            private List<double> _highVibrationBuffer;
            private List<double> _highIPSVibrationBuffer;

            public double Value
            {
                get => _value;
                set => SetProperty(ref _value, value);
            }

            public double VibrationX
            {
                get => _vibrationX;
                set => SetProperty(ref _vibrationX, value);
            }

            public double VibrationY
            {
                get => _vibrationY;
                set => SetProperty(ref _vibrationY, value);
            }

            public double VibrationZ
            {
                get => _vibrationZ;
                set => SetProperty(ref _vibrationZ, value);
            }

            public int TareBufferCount
            {
                get => _tareBufferCount;
                set => SetProperty(ref _tareBufferCount, value);
            }

            public double TareVibration
            {
                get => _tareVibration;
                set => SetProperty(ref _tareVibration, value);
            }
            public double TareCurrentVibration
            {
                get => _tareCurrentVibration;
                set => SetProperty(ref _tareCurrentVibration, value);
            }

            public List<double> TareVibrationBuffer
            {
                get => _tareVibrationBuffer;
                set => SetProperty(ref _tareVibrationBuffer, value);
            }

            public double TareVibrationX
            {
                get => _tareVibrationX;
                set => SetProperty(ref _tareVibrationX, value);
            }

            public List<double> TareVibrationXBuffer
            {
                get => _tareVibrationXBuffer;
                set => SetProperty(ref _tareVibrationXBuffer, value);
            }

            public double TareVibrationY
            {
                get => _tareVibrationY;
                set => SetProperty(ref _tareVibrationY, value);
            }

            public List<double> TareVibrationYBuffer
            {
                get => _tareVibrationYBuffer;
                set => SetProperty(ref _tareVibrationYBuffer, value);
            }

            public double TareVibrationZ
            {
                get => _tareVibrationZ;
                set => SetProperty(ref _tareVibrationZ, value);
            }

            public List<double> TareVibrationZBuffer
            {
                get => _tareVibrationZBuffer;
                set => SetProperty(ref _tareVibrationZBuffer, value);
            }

            public double HighVibration
            {
                get => _highVibration;
                set => SetProperty(ref _highVibration, value);
            }    
            public double HighIPSVibration
            {
                get => _highIPSVibration;
                set => SetProperty(ref _highIPSVibration, value);
            }

            public List<double> HighVibrationBuffer
            {
                get => _highVibrationBuffer;
                set => SetProperty(ref _highVibrationBuffer, value);
            }   
            public List<double> HighIPSVibrationBuffer
            {
                get => _highIPSVibrationBuffer;
                set => SetProperty(ref _highIPSVibrationBuffer, value);
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
            {
                if (Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }
        public class TheoricVariables : INotifyPropertyChanged
        {
            public const double AirGasConstant = 287.058; // Individual Gas Constant - R Unit: [J/kg K]
            public const double EscEffConst = 0.95;
            public const double KelvinConst = 273.15;
            private static double _propellerArea;
            private static double _rotationalSpeed;
            private static double _power;
            private static double _airDensity;
            private static double _propellerEfficiency;
            private static double _propSysEfficiencyI;
            private static double _propSysEfficiencyII;
            private static double _motorEfficiency;
            private static double _fom;
            private static double _ips; // Vibration - inches per second
            private static double _j;   // Advance Ratio
            private static double _ct;  // Thrust Coefficient
            private static double _cq;  // Torque Coefficient 
            private static double _cp;  // Power Coefficient

            public double PropellerArea
            {
                get => _propellerArea;
                set => SetProperty(ref _propellerArea, value);
            }

            public double RotationalSpeed
            {
                get => _rotationalSpeed;
                set => SetProperty(ref _rotationalSpeed, value);
            }

            public double Power
            {
                get => _power;
                set => SetProperty(ref _power, value);
            }
            public double AirDensity
            {
                get => _airDensity;
                set => SetProperty(ref _airDensity, value);
            }

            public double PropellerEfficiency
            {
                get => _propellerEfficiency;
                set => SetProperty(ref _propellerEfficiency, value);
            }

            public double PropSysEfficiencyI
            {
                get => _propSysEfficiencyI;
                set => SetProperty(ref _propSysEfficiencyI, value);
            }

            public double PropSysEfficiencyII
            {
                get => _propSysEfficiencyII;
                set => SetProperty(ref _propSysEfficiencyII, value);
            }

            public double MotorEfficiency
            {
                get => _motorEfficiency;
                set => SetProperty(ref _motorEfficiency, value);
            }      
            public double FOM
            {
                get => _fom;
                set => SetProperty(ref _fom, value);
            }

            public double J
            {
                get => _j;
                set => SetProperty(ref _j, value);
            }
            public double IPS
            {
                get => _ips;
                set => SetProperty(ref _ips, value);
            }
            public double Ct
            {
                get => _ct;
                set => SetProperty(ref _ct, value);
            }

            public double Cq
            {
                get => _cq;
                set => SetProperty(ref _cq, value);
            }

            public double Cp
            {
                get => _cp;
                set => SetProperty(ref _cp, value);
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
            {
                if (Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }
    }
}
