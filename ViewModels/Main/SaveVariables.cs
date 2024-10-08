using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Views.UserControls;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.InkML;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows;
using System.IO;

namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class SaveVariables : INotifyPropertyChanged
    {
        private InterfaceVariables _saveVariables;

        public SaveVariables()
        {
            _saveVariables = new InterfaceVariables();
            RecordStatus = false;
            RecordFileCreate = false;

            // Dosya adını oluştur (zaman damgalı)
           // string fileName = $"test_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
           // RecordFilePath = Path.Combine("C:\\Users\\kadir\\Downloads", fileName);
        }

        public void UpdateSaveVariables(DynotisData data)
        {
            RecordStatus = data.RecordStatus;

            Time = data.Time;
            AmbientTemp = _saveVariables.AmbientTempUnitSet(AmbientTemp.Value, AmbientTemp.UnitName, AmbientTemp.UnitSymbol, data.AmbientTemp.Value, data.AmbientTemp.UnitName, data.AmbientTemp.UnitSymbol);
            MotorTemp = _saveVariables.MotorTempUnitSet(MotorTemp.Value, MotorTemp.UnitName, MotorTemp.UnitSymbol, data.MotorTemp.Value, data.MotorTemp.UnitName, data.MotorTemp.UnitSymbol);
            MotorSpeed = _saveVariables.MotorSpeedTareSet(_saveVariables.MotorSpeedUnitSet(MotorSpeed.Value, MotorSpeed.UnitName, MotorSpeed.UnitSymbol, data.MotorSpeed.Value, data.MotorSpeed.UnitName, data.MotorSpeed.UnitSymbol));
            Thrust = _saveVariables.ThrustTareSet(_saveVariables.ThrustUnitSet(Thrust.Value, Thrust.UnitName, Thrust.UnitSymbol, data.Thrust.Value, data.Thrust.UnitName, data.Thrust.UnitSymbol));
            Torque = _saveVariables.TorqueTareSet(_saveVariables.TorqueUnitSet(Torque.Value, Torque.UnitName, Torque.UnitSymbol, data.Torque.Value, data.Torque.UnitName, data.Torque.UnitSymbol));
            Voltage = data.Voltage;
            Current = _saveVariables.CurrentTareSet(data.Current);
            Pressure = _saveVariables.PressureUnitSet(Pressure.Value, Pressure.UnitName, Pressure.UnitSymbol, data.Pressure.Value, data.Pressure.UnitName, data.Pressure.UnitSymbol);

            WindSpeed = _saveVariables.WindSpeedUnitSet(WindSpeed.Value, WindSpeed.UnitName, WindSpeed.UnitSymbol, data.WindSpeed.Value, data.WindSpeed.UnitName, data.WindSpeed.UnitSymbol);
            WindDirection = data.WindDirection;
            PropellerDiameter = data.PropellerDiameter;
            MotorInner = data.MotorInner;
            NoLoadCurrents = data.NoLoadCurrents;
            MaxCurrent = data.MaxCurrent;
            BatteryLevel = data.BatteryLevel;

            // Veriyi CSV'ye eklemek için satır oluşturma
            DataRow = new string[]
            {
                Time.ToString(),
                Current.ToString(),
                Voltage.ToString(),
                MotorSpeed.Value.ToString(),
                Thrust.Value.ToString(),
                Torque.Value.ToString(),
                Pressure.Value.ToString(),
                AmbientTemp.Value.ToString(),
                MotorTemp.Value.ToString()
            };
        }

        private bool _recordStatus;
        private string _recordFilePath;
        private bool _recordFileCreate;

        private string[] _dataRow;

        private double _time;
        private InterfaceVariables.Unit _ambientTemp;
        private InterfaceVariables.Unit _motorTemp;
        private InterfaceVariables.Unit _motorSpeed;
        private InterfaceVariables.Unit _thrust;
        private InterfaceVariables.Unit _torque;
        private double _voltage;
        private double _current;
        private InterfaceVariables.Unit _pressure;
        private InterfaceVariables.Unit _windSpeed;
        private double _windDirection;

        private double _propellerDiameter;
        private double _motorInner;
        private double _noLoadCurrents;
        private double _maxCurrent;
        private double _batteryLevel;

        public bool RecordStatus
        {
            get => _recordStatus;
            set => SetProperty(ref _recordStatus, value);
        }  
        public string RecordFilePath
        {
            get => _recordFilePath;
            set => SetProperty(ref _recordFilePath, value);
        }  
        public bool RecordFileCreate
        {
            get => _recordFileCreate;
            set => SetProperty(ref _recordFileCreate, value);
        }
        public string[] DataRow
        {
            get => _dataRow;
            set => SetProperty(ref _dataRow, value);
        }
        public double Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }
        public InterfaceVariables.Unit AmbientTemp
        {
            get => _ambientTemp;
            set => SetProperty(ref _ambientTemp, value);
        }

        public InterfaceVariables.Unit MotorTemp
        {
            get => _motorTemp;
            set => SetProperty(ref _motorTemp, value);
        }

        public InterfaceVariables.Unit MotorSpeed
        {
            get => _motorSpeed;
            set => SetProperty(ref _motorSpeed, value);
        }

        public InterfaceVariables.Unit Thrust
        {
            get => _thrust;
            set => SetProperty(ref _thrust, value);
        }

        public InterfaceVariables.Unit Torque
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

        public InterfaceVariables.Unit Pressure
        {
            get => _pressure;
            set => SetProperty(ref _pressure, value);
        }
        public InterfaceVariables.Unit WindSpeed
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
