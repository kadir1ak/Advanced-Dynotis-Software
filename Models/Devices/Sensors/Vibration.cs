using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.Models.Devices.Sensors
{
    public class Vibration : SensorBase
    {
        // Constructor
        public Vibration() : base("Vibration", "g", "g") // Titreşim birimi genelde m/s² veya g (yerçekimi ivmesi) olur
        {
            X = 0.0;
            Y = 0.0;
            Z = 0.0;
            UpdateValue(); // base.Value'yu güncelle
        }

        // Private alanlar
        private double _x;
        private double _y;
        private double _z;

        // Public özellikler (PropertyChanged için uygun)
        public double X
        {
            get => _x;
            set
            {
                if (SetProperty(ref _x, value))
                {
                    UpdateValue();
                }
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                if (SetProperty(ref _y, value))
                {
                    UpdateValue(); // Y değiştiğinde base.Value güncelle
                }
            }
        }

        public double Z
        {
            get => _z;
            set
            {
                if (SetProperty(ref _z, value))
                {
                    UpdateValue();
                }
            }
        }

        // Value güncelleyen metot
        private void UpdateValue()
        {
            double newValue = Math.Sqrt(Y * Y);
            if (!newValue.Equals(Value))
            {
                Value = newValue;
            }
        }

        // Titreşim değerini getiren metot
        public double GetVibration()
        {
            UpdateValue();
            return Value;
        }

        // Titreşimi m/s² cinsinden döndüren bir metot
        public double GetValueInAcceleration()
        {
            return GetVibration() * 9.80665; // g -> m/s² dönüşümü
        }

        // Titreşim değerlerini ayarlayan bir metot
        public void SetVibrationValues(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            UpdateValue();
        }
    }
}
