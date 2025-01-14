using Advanced_Dynotis_Software.Services.BindableBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.Models.Device.Sensors
{
    public abstract class SensorBase : BindableBase
    {
        // Constructor: Ortak özellikleri ayarlar
        protected SensorBase(string name, string unitName, string unitSymbol)
        {
            Name = name;        // Varsayılan başlangıç değeri
            Value = 0.0;       
            UnitName = unitName;
            UnitSymbol = unitSymbol;
        }

        // Private alanlar
        private string _name;
        private double _value;
        private string _unitName;
        private string _unitSymbol;

        // Public özellikler (PropertyChanged için uygun)
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public double Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string UnitName
        {
            get => _unitName;
            set => SetProperty(ref _unitName, value);
        }

        public string UnitSymbol
        {
            get => _unitSymbol;
            set => SetProperty(ref _unitSymbol, value);
        }

        // Metotlar
        public virtual double GetValue() => Value;
        public virtual void SetValue(double newValue) => Value = newValue;

        public virtual string GetUnitName() => UnitName;
        public virtual void SetUnitName(string newUnitName) => UnitName = newUnitName;

        public virtual string GetUnitSymbol() => UnitSymbol;
        public virtual void SetUnitSymbol(string newUnitSymbol) => UnitSymbol = newUnitSymbol;
    }
}
