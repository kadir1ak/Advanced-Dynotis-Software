using Advanced_Dynotis_Software.Models.Dynotis;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class InterfaceVariables : INotifyPropertyChanged
{
    public struct Unit
    {
        public double Value { get; set; }
        public string UnitName { get; set; }
        public string UnitSymbol { get; set; }
        public double BaseValue { get; set; }
        public string BaseUnitName { get; set; }
        public string BaseUnitSymbol { get; set; }

        public Unit(double value, string unitName, string unitSymbol, double baseValue, string baseUnitName, string baseUnitSymbol)
        {
            Value = value;
            UnitName = unitName;
            UnitSymbol = unitSymbol;
            BaseValue = baseValue;
            BaseUnitName = baseUnitName;
            BaseUnitSymbol = baseUnitSymbol;
        }
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
    private double _power;
    private Unit _pressure;
    private double _vibrationX;
    private double _vibrationY;
    private double _vibrationZ;
    private double _vibration;
    private Unit _windSpeed;
    private double _windDirection;
    private double _airDensity;

    private double _propellerArea;
    private double _motorInner;
    private double _noLoadCurrents;
    private double _maxCurrent;
    private double _batteryLevel;
    private bool _securityStatus;
    private double _escValue;
    private bool _escStatus;
    private string _saveFile;
    private bool _saveStatus;
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

    public double Power
    {
        get => _power;
        set => SetProperty(ref _power, value);
    }

    public Unit Pressure
    {
        get => _pressure;
        set => SetProperty(ref _pressure, value);
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

    public bool SecurityStatus
    {
        get => _securityStatus;
        set => SetProperty(ref _securityStatus, value);
    }

    public double ESCValue
    {
        get => _escValue;
        set => SetProperty(ref _escValue, value);
    }

    public bool ESCStatus
    {
        get => _escStatus;
        set => SetProperty(ref _escStatus, value);
    }

    public string SaveFile
    {
        get => _saveFile;
        set => SetProperty(ref _saveFile, value);
    }

    public bool SaveStatus
    {
        get => _saveStatus;
        set => SetProperty(ref _saveStatus, value);
    }

    public string TestMode
    {
        get => _testMode;
        set => SetProperty(ref _testMode, value);
    }

    public void UpdateFrom(DynotisData data)
    {
        Time = data.Time;
        SampleRate = data.SampleRate;
        AmbientTemp = new Unit(AmbientTemp.Value, AmbientTemp.UnitName, AmbientTemp.UnitSymbol, data.AmbientTemp.Value, data.AmbientTemp.UnitName, data.AmbientTemp.UnitSymbol);
        MotorTemp = new Unit(MotorTemp.Value, MotorTemp.UnitName, MotorTemp.UnitSymbol, data.MotorTemp.Value, data.MotorTemp.UnitName, data.MotorTemp.UnitSymbol);
        MotorSpeed = new Unit(MotorSpeed.Value, MotorSpeed.UnitName, MotorSpeed.UnitSymbol, data.MotorSpeed.Value, data.MotorSpeed.UnitName, data.MotorSpeed.UnitSymbol);
        Thrust = new Unit(Thrust.Value, Thrust.UnitName, Thrust.UnitSymbol, data.Thrust.Value, data.Thrust.UnitName, data.Thrust.UnitSymbol);
        Torque = new Unit(Torque.Value, Torque.UnitName, Torque.UnitSymbol, data.Torque.Value, data.Torque.UnitName, data.Torque.UnitSymbol);
        Voltage = data.Voltage;
        Current = data.Current;
        Power = data.Power;
        Pressure = new Unit(Pressure.Value, Pressure.UnitName, Pressure.UnitSymbol, data.Pressure.Value, data.Pressure.UnitName, data.Pressure.UnitSymbol);
        VibrationX = data.VibrationX;
        VibrationY = data.VibrationY;
        VibrationZ = data.VibrationZ;
        Vibration = data.Vibration;
        WindSpeed = new Unit(WindSpeed.Value, WindSpeed.UnitName, WindSpeed.UnitSymbol, data.WindSpeed.Value, data.WindSpeed.UnitName, data.WindSpeed.UnitSymbol);
        WindDirection = data.WindDirection;
        AirDensity = data.AirDensity;
        PropellerArea = data.PropellerArea;
        MotorInner = data.MotorInner;
        NoLoadCurrents = data.NoLoadCurrents;
        MaxCurrent = data.MaxCurrent;
        BatteryLevel = data.BatteryLevel;
        SecurityStatus = data.SecurityStatus;
        ESCValue = data.ESCValue;
        ESCStatus = data.ESCStatus;
        SaveFile = data.SaveFile;
        SaveStatus = data.SaveStatus;
        TestMode = data.TestMode;
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
