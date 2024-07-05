using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Advanced_Dynotis_Software.Models.Dynotis
{
    public class DynotisData : INotifyPropertyChanged
    {
        private double _time;
        private double _sampleRate;
        private double _ambientTemp;
        private double _motorTemp;
        private double _motorSpeed;
        private double _thrust;
        private double _torque;
        private double _voltage;
        private double _current;
        private double _power;
        private double _temperature;
        private double _pressure;
        private double _humidity;
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
        private string _securityStatus;
        private double _escValue;
        private string _escStatus;
        private string _saveFile;
        private string _saveStatus;
        private string _testMode;

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

        public double AmbientTemp
        {
            get => _ambientTemp;
            set => SetProperty(ref _ambientTemp, value);
        }

        public double MotorTemp
        {
            get => _motorTemp;
            set => SetProperty(ref _motorTemp, value);
        }

        public double MotorSpeed
        {
            get => _motorSpeed;
            set => SetProperty(ref _motorSpeed, value);
        }

        public double Thrust
        {
            get => _thrust;
            set => SetProperty(ref _thrust, value);
        }

        public double Torque
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

        public double Power
        {
            get => _power;
            set => SetProperty(ref _power, value);
        }
        public double Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }
        public double Pressure
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

        public double Vibration
        {
            get => _vibration;
            set => SetProperty(ref _vibration, value);
        }

        public double WindSpeed
        {
            get => _windSpeed;
            set => SetProperty(ref _windSpeed, value);
        }

        public double WindDirection
        {
            get => _windDirection;
            set => SetProperty(ref _windDirection, value);
        }

        public double AirDensity
        {
            get => _airDensity;
            set => SetProperty(ref _airDensity, value);
        }

        public double PropellerArea
        {
            get => _propellerArea;
            set => SetProperty(ref _propellerArea, value);
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

        public string SecurityStatus
        {
            get => _securityStatus;
            set => SetProperty(ref _securityStatus, value);
        }

        public double ESCValue
        {
            get => _escValue;
            set => SetProperty(ref _escValue, value);
        }

        public string ESCStatus
        {
            get => _escStatus;
            set => SetProperty(ref _escStatus, value);
        }

        public string SaveFile
        {
            get => _saveFile;
            set => SetProperty(ref _saveFile, value);
        }

        public string SaveStatus
        {
            get => _saveStatus;
            set => SetProperty(ref _saveStatus, value);
        }

        public string TestMode
        {
            get => _testMode;
            set => SetProperty(ref _testMode, value);
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
