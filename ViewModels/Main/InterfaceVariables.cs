using Advanced_Dynotis_Software.Models.Dynotis;
using DocumentFormat.OpenXml;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

public class InterfaceVariables : INotifyPropertyChanged
{
    private static InterfaceVariables _instance;
    public static InterfaceVariables Instance => _instance ??= new InterfaceVariables();
    public InterfaceVariables() 
    {
        // Default system values
        _selectedIsEnglishChecked = true;
        _selectedIsTurkishChecked = false;
        _selectedTorqueUnitIndex = 0;
        _selectedThrustUnitIndex = 0;
        _selectedMotorSpeedUnitIndex = 0;
        _selectedTemperatureUnitIndex = 0;
        _selectedWindSpeedUnitIndex = 0;
        _selectedPressureUnitIndex = 1;

        TestMode = "fuzzy";
        IsRecording = false;
        Duration = TimeSpan.Zero;
        FileName = null;
    }
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

    private string _testMode;
    private bool _isRecording;
    private string _fileName;
    private TimeSpan _duration;


    private double _tareTorqueBaseValue;
    private double _tareThrustBaseValue;
    private double _tareCurrentBaseValue;
    private double _tareMotorSpeedBaseValue;

    private int _selectedTorqueUnitIndex;
    private int _selectedThrustUnitIndex;
    private int _selectedMotorSpeedUnitIndex;
    private int _selectedTemperatureUnitIndex;
    private int _selectedWindSpeedUnitIndex;
    private int _selectedPressureUnitIndex;

    private bool _selectedIsTurkishChecked;
    private bool _selectedIsEnglishChecked;

    private int _referenceMotorSpeed;
    private double _referenceWeight;
    private double _referencePropellerArea;
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

    public bool IsRecording
    {
        get => _isRecording;
        set => SetProperty(ref _isRecording, value);
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

    public double TareTorqueBaseValue
    {
        get => _tareTorqueBaseValue;
        set => SetProperty(ref _tareTorqueBaseValue, value);
    }

    public double TareThrustBaseValue
    {
        get => _tareThrustBaseValue;
        set => SetProperty(ref _tareThrustBaseValue, value);
    }

    public double TareCurrentBaseValue
    {
        get => _tareCurrentBaseValue;
        set => SetProperty(ref _tareCurrentBaseValue, value);
    }

    public double TareMotorSpeedBaseValue
    {
        get => _tareMotorSpeedBaseValue;
        set => SetProperty(ref _tareMotorSpeedBaseValue, value);
    }

    public int SelectedTorqueUnitIndex
    {
        get => _selectedTorqueUnitIndex;
        set
        {
            if (SetProperty(ref _selectedTorqueUnitIndex, value))
            {
                UpdateTorqueUnit();
            }
        }
    }

    public int SelectedThrustUnitIndex
    {
        get => _selectedThrustUnitIndex;
        set
        {
            if (SetProperty(ref _selectedThrustUnitIndex, value))
            {
                UpdateThrustUnit();
            }
        }
    }

    public int SelectedMotorSpeedUnitIndex
    {
        get => _selectedMotorSpeedUnitIndex;
        set
        {
            if (SetProperty(ref _selectedMotorSpeedUnitIndex, value))
            {
                UpdateMotorSpeedUnit();
            }
        }
    }

    public int SelectedTemperatureUnitIndex
    {
        get => _selectedTemperatureUnitIndex;
        set
        {
            if (SetProperty(ref _selectedTemperatureUnitIndex, value))
            {
                UpdateTemperatureUnit();
            }
        }
    }

    public int SelectedWindSpeedUnitIndex
    {
        get => _selectedWindSpeedUnitIndex;
        set
        {
            if (SetProperty(ref _selectedWindSpeedUnitIndex, value))
            {
                UpdateWindSpeedUnit();
            }
        }
    }

    public int SelectedPressureUnitIndex
    {
        get => _selectedPressureUnitIndex;
        set
        {
            if (SetProperty(ref _selectedPressureUnitIndex, value))
            {
                UpdatePressureUnit();
            }
        }
    }

    private void UpdateTorqueUnit()
    {
        Torque = TorqueUnitSet(Torque.Value, Torque.UnitName, Torque.UnitSymbol, Torque.BaseValue, Torque.BaseUnitName, Torque.BaseUnitSymbol);
      
    }

    private void UpdateThrustUnit()
    {
        Thrust = ThrustUnitSet(Thrust.Value, Thrust.UnitName, Thrust.UnitSymbol, Thrust.BaseValue, Thrust.BaseUnitName, Thrust.BaseUnitSymbol);
    }

    private void UpdateMotorSpeedUnit()
    {
        MotorSpeed = MotorSpeedUnitSet(MotorSpeed.Value, MotorSpeed.UnitName, MotorSpeed.UnitSymbol, MotorSpeed.BaseValue, MotorSpeed.BaseUnitName, MotorSpeed.BaseUnitSymbol);
    }

    private void UpdateTemperatureUnit()
    {
        AmbientTemp = AmbientTempUnitSet(AmbientTemp.Value, AmbientTemp.UnitName, AmbientTemp.UnitSymbol, AmbientTemp.BaseValue, AmbientTemp.BaseUnitName, AmbientTemp.BaseUnitSymbol);
        MotorTemp = MotorTempUnitSet(MotorTemp.Value, MotorTemp.UnitName, MotorTemp.UnitSymbol, MotorTemp.BaseValue, MotorTemp.BaseUnitName, MotorTemp.BaseUnitSymbol);
    }

    private void UpdateWindSpeedUnit()
    {
        WindSpeed = WindSpeedUnitSet(WindSpeed.Value, WindSpeed.UnitName, WindSpeed.UnitSymbol, WindSpeed.BaseValue, WindSpeed.BaseUnitName, WindSpeed.BaseUnitSymbol);
    }

    private void UpdatePressureUnit()
    {
        Pressure = PressureUnitSet(Pressure.Value, Pressure.UnitName, Pressure.UnitSymbol, Pressure.BaseValue, Pressure.BaseUnitName, Pressure.BaseUnitSymbol);
    }


    public bool SelectedIsTurkishChecked
    {
        get => _selectedIsTurkishChecked;
        set => SetProperty(ref _selectedIsTurkishChecked, value);
    }

    public bool SelectedIsEnglishChecked
    {
        get => _selectedIsEnglishChecked;
        set => SetProperty(ref _selectedIsEnglishChecked, value);
    }

    public int ReferenceMotorSpeed
    {
        get => _referenceMotorSpeed;
        set => SetProperty(ref _referenceMotorSpeed, value);
    }
    public double ReferenceWeight
    {
        get => _referenceWeight;
        set => SetProperty(ref _referenceWeight, value);
    }   
    public double ReferencePropellerArea
    {
        get => _referencePropellerArea;
        set => SetProperty(ref _referencePropellerArea, value);
    }
    public void UpdateFrom(DynotisData data)
    {
        // Synchronization of updated values
        Time = data.Time;
        SampleRate = data.SampleRate;
        AmbientTemp = AmbientTempUnitSet(AmbientTemp.Value, AmbientTemp.UnitName, AmbientTemp.UnitSymbol, data.AmbientTemp.Value, data.AmbientTemp.UnitName, data.AmbientTemp.UnitSymbol);
        MotorTemp = MotorTempUnitSet(MotorTemp.Value, MotorTemp.UnitName, MotorTemp.UnitSymbol, data.MotorTemp.Value, data.MotorTemp.UnitName, data.MotorTemp.UnitSymbol);
        MotorSpeed = MotorSpeedTareSet(MotorSpeedUnitSet(MotorSpeed.Value, MotorSpeed.UnitName, MotorSpeed.UnitSymbol, data.MotorSpeed.Value, data.MotorSpeed.UnitName, data.MotorSpeed.UnitSymbol));
        Thrust = ThrustTareSet(ThrustUnitSet(Thrust.Value, Thrust.UnitName, Thrust.UnitSymbol, data.Thrust.Value, data.Thrust.UnitName, data.Thrust.UnitSymbol));
        Torque = TorqueTareSet(TorqueUnitSet(Torque.Value, Torque.UnitName, Torque.UnitSymbol, data.Torque.Value, data.Torque.UnitName, data.Torque.UnitSymbol));
        Voltage = data.Voltage;
        Current = CurrentTareSet(data.Current);
        Power = data.Power;
        Pressure = PressureUnitSet(Pressure.Value, Pressure.UnitName, Pressure.UnitSymbol, data.Pressure.Value, data.Pressure.UnitName, data.Pressure.UnitSymbol);
        VibrationX = data.VibrationX;
        VibrationY = data.VibrationY;
        VibrationZ = data.VibrationZ;
        Vibration = data.Vibration;
        WindSpeed = WindSpeedUnitSet(WindSpeed.Value, WindSpeed.UnitName, WindSpeed.UnitSymbol, data.WindSpeed.Value, data.WindSpeed.UnitName, data.WindSpeed.UnitSymbol);
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

        // Ensure your values ​​are protected
        SelectedIsTurkishChecked = InterfaceVariables.Instance.SelectedIsTurkishChecked;
        SelectedIsEnglishChecked = InterfaceVariables.Instance.SelectedIsEnglishChecked;
        SelectedTorqueUnitIndex = InterfaceVariables.Instance.SelectedTorqueUnitIndex;
        SelectedThrustUnitIndex = InterfaceVariables.Instance.SelectedThrustUnitIndex;
        SelectedMotorSpeedUnitIndex = InterfaceVariables.Instance.SelectedMotorSpeedUnitIndex;
        SelectedTemperatureUnitIndex = InterfaceVariables.Instance.SelectedTemperatureUnitIndex;
        SelectedWindSpeedUnitIndex = InterfaceVariables.Instance.SelectedWindSpeedUnitIndex;
        SelectedPressureUnitIndex = InterfaceVariables.Instance.SelectedPressureUnitIndex;


    }
    public Unit ThrustTareSet(Unit data)
    {
        Unit temp = ThrustUnitSet(data.Value, data.UnitName,data.UnitSymbol, TareThrustBaseValue, data.BaseUnitName, data.BaseUnitSymbol);
        double newValue = data.Value - temp.Value;
        return new Unit(newValue, data.UnitName, data.UnitSymbol, data.BaseValue, data.BaseUnitName, data.BaseUnitSymbol);
    }

    public Unit TorqueTareSet(Unit data)
    {
        Unit temp = TorqueUnitSet(data.Value, data.UnitName, data.UnitSymbol, TareTorqueBaseValue, data.BaseUnitName, data.BaseUnitSymbol);
        double newValue = data.Value - temp.Value;
        return new Unit(newValue, data.UnitName, data.UnitSymbol, data.BaseValue, data.BaseUnitName, data.BaseUnitSymbol);
    }

    public double CurrentTareSet(double value)
    {
        return value - TareCurrentBaseValue;
    }

    public Unit MotorSpeedTareSet(Unit data)
    {
        Unit temp = MotorSpeedUnitSet(data.Value, data.UnitName, data.UnitSymbol, TareMotorSpeedBaseValue, data.BaseUnitName, data.BaseUnitSymbol);
        double newValue = data.Value - temp.Value;
        return new Unit(newValue, data.UnitName, data.UnitSymbol, data.BaseValue, data.BaseUnitName, data.BaseUnitSymbol);
    }


    public Unit AmbientTempUnitSet(double value, string unitName, string unitSymbol, double baseValue, string baseUnitName, string baseUnitSymbol)
    {
        /*  Ambient Temperature: Sensor Data Unit °C
            0 Celsius [°C]                      1 °C = 1 °C
            1 Fahrenheit [°F]                   1 °C = (°C * 9/5) + 32 °F
            2 Kelvin [K]                        1 °C = °C + 273.15 K
        */
        if (SelectedTemperatureUnitIndex == 0) // 0 Celsius [°C]
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }
        else if (SelectedTemperatureUnitIndex == 1) // 1 Fahrenheit [°F]
        {
            value = (baseValue * 9 / 5) + 32;
            unitName = "Fahrenheit";
            unitSymbol = "°F";
        }
        else if (SelectedTemperatureUnitIndex == 2) // 2 Kelvin [K]
        {
            value = baseValue + 273.15;
            unitName = "Kelvin";
            unitSymbol = "K";
        }
        else // 0 Celsius [°C]
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }
       
        return new Unit(value, unitName, unitSymbol, baseValue, baseUnitName, baseUnitSymbol);
    }

    public Unit MotorTempUnitSet(double value, string unitName, string unitSymbol, double baseValue, string baseUnitName, string baseUnitSymbol)
    {
        /*  Motor Temperature: Sensor Data Unit °C
            0 Celsius [°C]                      1 °C = 1 °C
            1 Fahrenheit [°F]                   1 °C = (°C * 9/5) + 32 °F
            2 Kelvin [K]                        1 °C = °C + 273.15 K
        */
        if (SelectedTemperatureUnitIndex == 0) // 0 Celsius [°C]
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }
        else if (SelectedTemperatureUnitIndex == 1) // 1 Fahrenheit [°F]
        {
            value = (baseValue * 9 / 5) + 32;
            unitName = "Fahrenheit";
            unitSymbol = "°F";
        }
        else if (SelectedTemperatureUnitIndex == 2) // 2 Kelvin [K]
        {
            value = baseValue + 273.15;
            unitName = "Kelvin";
            unitSymbol = "K";
        }
        else // 0 Celsius [°C]
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }

        return new Unit(value, unitName, unitSymbol, baseValue, baseUnitName, baseUnitSymbol);
    }

    public Unit MotorSpeedUnitSet(double value, string unitName, string unitSymbol, double baseValue, string baseUnitName, string baseUnitSymbol)
    {
        /*  Motor Speed: Sensor Data Unit RPM
            0 Revolutions per minute [RPM]      1 RPM = 1 RPM
            1 Hertz [Hz]                        1 RPM = 0.016667 Hz
            2 Radian per second [rad/s]         1 RPM = 0.104719755 rad/s
        */
        if (SelectedMotorSpeedUnitIndex == 0) // 0 Revolutions per minute [RPM]
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }
        else if (SelectedMotorSpeedUnitIndex == 1) // 1 Hertz [Hz]   
        {
            value = baseValue * 0.016667;
            unitName = "Hertz";
            unitSymbol = "Hz";
        }
        else if (SelectedMotorSpeedUnitIndex == 2) //   2 Radian per second [rad/s]   
        {
            value = baseValue * 0.104719755;
            unitName = "Radian per second";
            unitSymbol = "rad/s";
        }
        else // 0 Revolutions per minute [RPM]
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }

        return new Unit(value, unitName, unitSymbol, baseValue, baseUnitName, baseUnitSymbol);
    }

    public Unit ThrustUnitSet(double value, string unitName, string unitSymbol, double baseValue, string baseUnitName, string baseUnitSymbol)
    {
        /*  Thrust: Sensor Data Unit gf
            0 Gram-force [gf]                   1 gf = 1 gf
            1 Kilogram-force [kgf]              1 gf = 0.001 kgf
            2 Ounce-force [ozf]                 1 gf = 0.0352739619 ozf
            3 Pound-force [lbf]                 1 gf = 0.0022046226 lbf
            4 Newton [N]                        1 gf = 0.00980665 N
        */
        if (SelectedThrustUnitIndex == 0) // 0 Gram-force [gf]    
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }
        else if (SelectedThrustUnitIndex == 1) // 1 Kilogram-force [kgf] 
        {
            value = baseValue * 0.001;
            unitName = "Kilogram-force";
            unitSymbol = "kgf";
        }
        else if (SelectedThrustUnitIndex == 2) // 2 Ounce-force [ozf]  
        {
            value = baseValue * 0.0352739619;
            unitName = "Ounce-force";
            unitSymbol = "ozf";
        }
        else if (SelectedThrustUnitIndex == 3) // 3 Pound-force [lbf]     
        {
            value = baseValue * 0.0022046226;
            unitName = "Pound-force";
            unitSymbol = "lbf";
        }
        else if (SelectedThrustUnitIndex == 4) // 4 Newton [N]       
        {
            value = baseValue * 0.00980665;
            unitName = "Newton";
            unitSymbol = "N";
        }
        else // 0 Gram-force [gf]
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }

        return new Unit(value, unitName, unitSymbol, baseValue, baseUnitName, baseUnitSymbol);
    }

    public Unit TorqueUnitSet(double value, string unitName, string unitSymbol, double baseValue, string baseUnitName, string baseUnitSymbol)
    {
        /*  Torque: Sensor Data Unit N.mm
            0 Newton millimeter [N.mm]          1 N.mm = 1 N.mm
            1 Newton meter [N.m]                1 N.mm = 0.001 N.m
            2 Ounce-force inch [ozf.in]         1 N.mm = 0.1416119289357 ozf.in
            3 Ounce-force foot [ozf.ft]         1 N.mm = 0.01180099407797 ozf.ft
            4 Pound-force inch [lbf.in]         1 N.mm = 0.008850745454036 lbf.in
            5 Pound-force foot [lbf.ft]         1 N.mm = 0.0007375621211697 lbf.ft
            6 Kilogram-force meter [kgf.m]      1 N.mm = 0.0001019716212978 kgf.m
            7 Gram-force meter [gf.m]           1 N.mm = 0.1019716212978 gf.m
        */
        if (SelectedTorqueUnitIndex == 0) // 0 Newton millimeter [N.mm] 
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }
        else if (SelectedTorqueUnitIndex == 1) // 1 Newton meter [N.m]  
        {
            value = baseValue * 0.001;
            unitName = "Newton meter";
            unitSymbol = "N.m";
        }
        else if (SelectedTorqueUnitIndex == 2) // 2 Ounce-force inch [ozf.in]  
        {
            value = baseValue * 0.1416119289357;
            unitName = "Ounce-force inch";
            unitSymbol = "ozf.in";
        }
        else if (SelectedTorqueUnitIndex == 3) // 3 Ounce-force foot [ozf.ft]   
        {
            value = baseValue * 0.01180099407797;
            unitName = "Ounce-force foot";
            unitSymbol = "ozf.ft";
        }
        else if (SelectedTorqueUnitIndex == 4) // 4 Pound-force inch [lbf.in] 
        {
            value = baseValue * 0.008850745454036;
            unitName = "Pound-force inch";
            unitSymbol = "lbf.in";
        }
        else if (SelectedTorqueUnitIndex == 5) // 5 Pound-force foot [lbf.ft]   
        {
            value = baseValue * 0.0007375621211697;
            unitName = "Pound-force foot";
            unitSymbol = "lbf.ft";
        }
        else if (SelectedTorqueUnitIndex == 6) // 6 Kilogram-force meter [kgf.m]   
        {
            value = baseValue * 0.0001019716212978;
            unitName = "Kilogram-force meter";
            unitSymbol = "kgf.m";
        }
        else if (SelectedTorqueUnitIndex == 7) // 7 Gram-force meter [gf.m]     
        {
            value = baseValue * 0.1019716212978;
            unitName = "Gram-force meter";
            unitSymbol = "gf.m";
        }
        else // 0 Newton millimeter [N.mm] 
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }

        return new Unit(value, unitName, unitSymbol, baseValue, baseUnitName, baseUnitSymbol);
    }

    public Unit PressureUnitSet(double value, string unitName, string unitSymbol, double baseValue, string baseUnitName, string baseUnitSymbol)
    {
        /*  Pressure: Sensor Data Unit Pa
            0 Pascal [Pa]                       1 Pa = 1 Pa
            1 Hectopascals [hPa]                1 Pa = 0.01 hPa
            2 Inches of water [in H2O]           1 Pa = 0.004014742133 in H2O (4°C)
            3 Millimeters of water [mm H2O]      1 Pa = 0.101974429 mm H2O (4°C)
        */
        if (SelectedPressureUnitIndex == 0) // 0 Pascal [Pa] 
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }
        else if (SelectedPressureUnitIndex == 1) // 1 Hectopascals [hPa]   
        {
            value = baseValue * 0.01;
            unitName = "Hectopascals";
            unitSymbol = "hPa";
        }
        else if (SelectedPressureUnitIndex == 2) // 2 Inches of water [in H2O]     
        {
            value = baseValue * 0.004014742133;
            unitName = "Inches of water";
            unitSymbol = "in H2O";
        }
        else if (SelectedPressureUnitIndex == 3) // 3 Millimeters of water [mm H2O] 
        {
            value = baseValue * 0.101974429;
            unitName = "Millimeters of water";
            unitSymbol = "mm H2O";
        }
        else
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }

        return new Unit(value, unitName, unitSymbol, baseValue, baseUnitName, baseUnitSymbol);
    }

    public Unit WindSpeedUnitSet(double value, string unitName, string unitSymbol, double baseValue, string baseUnitName, string baseUnitSymbol)
    {
        /*  WindSpeed: Sensor Data Unit m/s
            0 Meters per second [m/s]           1 m/s = 1 m/s
            1 Feet per second [ft/s]            1 m/s = 3.280839895013123 ft/s
            2 Kilometers per hour [km/h]        1 m/s = 3.6 km/h
            3 Miles per hour [mph]              1 m/s = 2.2369362920544025 mph
        */
        if (SelectedWindSpeedUnitIndex == 0) //  0 Meters per second [m/s] 
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }
        else if (SelectedWindSpeedUnitIndex == 1) // 1 Feet per second [ft/s]  
        {
            value = baseValue * 3.280839895013123;
            unitName = "Feet per second";
            unitSymbol = "ft/s";
        }
        else if (SelectedWindSpeedUnitIndex == 2) // 2 Kilometers per hour [km/h]
        {
            value = baseValue * 3.6;
            unitName = "Kilometers per hour";
            unitSymbol = "km/h";
        }
        else if (SelectedWindSpeedUnitIndex == 3) //  3 Miles per hour [mph]  
        {
            value = baseValue * 2.2369362920544025;
            unitName = "Miles per hour";
            unitSymbol = "mph";
        }
        else
        {
            value = baseValue;
            unitName = baseUnitName;
            unitSymbol = baseUnitSymbol;
        }

        return new Unit(value, unitName, unitSymbol, baseValue, baseUnitName, baseUnitSymbol);
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
