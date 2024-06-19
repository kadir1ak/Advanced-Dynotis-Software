using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Advanced_Dynotis_Software.Models.Dynotis
{
    public class DynotisData : INotifyPropertyChanged
    {
        private int _time;
        private double _sampleRate;
        private double _ambientTemp;
        private double _motorTemp;
        private double _motorSpeed;
        private double _thrust;
        private double _torque;
        private double _voltage;
        private double _current;
        private double _power;
        private double _pressure;
        private double _vibrationX;
        private double _vibrationY;
        private double _vibrationZ;
        private double _vibration;
        private double _windSpeed;
        private double _windDirection;
        private double _airDensity;

        private double _propellerArea;
        private double _motorInner;
        private double _noLoadCurrents;
        private double _maxCurrent;
        private double _batteryLevel;
        private double _escValue;
        private string _csvFile;
        private string _csvFileDirectory;
        private int _saveSpeed;
        private string _testMode;

        public double PropellerArea
        {
            get => _propellerArea;
            set
            {
                if (_propellerArea != value)
                {
                    _propellerArea = value;
                    OnPropertyChanged();
                }
            }
        }

        public double MotorInner
        {
            get => _motorInner;
            set
            {
                if (_motorInner != value)
                {
                    _motorInner = value;
                    OnPropertyChanged();
                }
            }
        }

        public double NoLoadCurrents
        {
            get => _noLoadCurrents;
            set
            {
                if (_noLoadCurrents != value)
                {
                    _noLoadCurrents = value;
                    OnPropertyChanged();
                }
            }
        }

        public double MaxCurrent
        {
            get => _maxCurrent;
            set
            {
                if (_maxCurrent != value)
                {
                    _maxCurrent = value;
                    OnPropertyChanged();
                }
            }
        }

        public double BatteryLevel
        {
            get => _batteryLevel;
            set
            {
                if (_batteryLevel != value)
                {
                    _batteryLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        public double ESCValue
        {
            get => _escValue;
            set
            {
                if (_escValue != value)
                {
                    _escValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CsvFile
        {
            get => _csvFile;
            set
            {
                if (_csvFile != value)
                {
                    _csvFile = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CsvFileDirectory
        {
            get => _csvFileDirectory;
            set
            {
                if (_csvFileDirectory != value)
                {
                    _csvFileDirectory = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SaveSpeed
        {
            get => _saveSpeed;
            set
            {
                if (_saveSpeed != value)
                {
                    _saveSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TestMode
        {
            get => _testMode;
            set
            {
                if (_testMode != value)
                {
                    _testMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public double AirDensity
        {
            get => _airDensity;
            set
            {
                if (_airDensity != value)
                {
                    _airDensity = value;
                    OnPropertyChanged();
                }
            }
        }
        public double SampleRate
        {
            get => _sampleRate;
            set
            {
                if (_sampleRate != value)
                {
                    _sampleRate = value;
                    OnPropertyChanged();
                }
            }
        }

        public double WindSpeed
        {
            get => _windSpeed;
            set
            {
                if (_windSpeed != value)
                {
                    _windSpeed = value;
                    OnPropertyChanged();
                }
            }
        }
        public double WindDirection
        {
            get => _windDirection;
            set
            {
                if (_windDirection != value)
                {
                    _windDirection = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Time
        {
            get => _time;
            set
            {
                if (_time != value)
                {
                    _time = value;
                    OnPropertyChanged();
                }
            }
        }

        public double AmbientTemp
        {
            get => _ambientTemp;
            set
            {
                if (_ambientTemp != value)
                {
                    _ambientTemp = value;
                    OnPropertyChanged();
                }
            }
        }

        public double MotorTemp
        {
            get => _motorTemp;
            set
            {
                if (_motorTemp != value)
                {
                    _motorTemp = value;
                    OnPropertyChanged();
                }
            }
        }

        public double MotorSpeed
        {
            get => _motorSpeed;
            set
            {
                if (_motorSpeed != value)
                {
                    _motorSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Thrust
        {
            get => _thrust;
            set
            {
                if (_thrust != value)
                {
                    _thrust = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Torque
        {
            get => _torque;
            set
            {
                if (_torque != value)
                {
                    _torque = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Voltage
        {
            get => _voltage;
            set
            {
                if (_voltage != value)
                {
                    _voltage = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Current
        {
            get => _current;
            set
            {
                if (_current != value)
                {
                    _current = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Power
        {
            get => _power;
            set
            {
                if (_power != value)
                {
                    _power = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Pressure
        {
            get => _pressure;
            set
            {
                if (_pressure != value)
                {
                    _pressure = value;
                    OnPropertyChanged();
                }
            }
        }

        public double VibrationX
        {
            get => _vibrationX;
            set
            {
                if (_vibrationX != value)
                {
                    _vibrationX = value;
                    OnPropertyChanged();
                }
            }
        }

        public double VibrationY
        {
            get => _vibrationY;
            set
            {
                if (_vibrationY != value)
                {
                    _vibrationY = value;
                    OnPropertyChanged();
                }
            }
        }

        public double VibrationZ
        {
            get => _vibrationZ;
            set
            {
                if (_vibrationZ != value)
                {
                    _vibrationZ = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Vibration
        {
            get => _vibration;
            set
            {
                if (_vibration != value)
                {
                    _vibration = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
