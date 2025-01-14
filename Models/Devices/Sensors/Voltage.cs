using System;
using Advanced_Dynotis_Software.Services.BindableBase;

namespace Advanced_Dynotis_Software.Models.Devices.Sensors
{
    public class Voltage : SensorBase
    {
        public Voltage() : base("Voltage", "V", "V") { }

        // Voltajı mV cinsinden döndüren bir metot
        public double GetValueInMilliVolts()
        {
            return GetValue() * 1000.0; // V -> mV dönüşümü
        }

        // Voltajın güvenli olup olmadığını kontrol eden bir metot
        public string CheckVoltageStatus(double min, double max)
        {
            double value = GetValue();

            if (value < min)
            {
                return "Uyarı: Voltaj düşük! Bataryayı kontrol edin.";
            }
            else if (value > max)
            {
                return "Uyarı: Voltaj yüksek! Sistemi kontrol edin.";
            }
            else
            {
                return "Batarya durumu: Güvenli.";
            }
        }
    }
}
