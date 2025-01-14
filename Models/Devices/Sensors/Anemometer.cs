using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.Models.Devices.Sensors
{
    public class Anemometer
    {
        // Sensörler
        public WindSpeed WindSpeedSensor { get; private set; }
        public WindDirection WindDirectionSensor { get; private set; }

        // Constructor
        public Anemometer()
        {
            WindSpeedSensor = new WindSpeed();
            WindDirectionSensor = new WindDirection();
        }

        // Rüzgar Hızı Sensörü
        public class WindSpeed : SensorBase
        {
            public WindSpeed() : base("WindSpeed", "Meters per second", "m/s") { }
        }

        // Rüzgar Yönü Sensörü
        public class WindDirection : SensorBase
        {
            public WindDirection() : base("WindDirection", "Angle", "°") { }
        }
    }
}
