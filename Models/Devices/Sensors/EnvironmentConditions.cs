using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advanced_Dynotis_Software.Models.Devices.Sensors
{
    public class EnvironmentConditions
    {
        // Sensörler
        public AmbientTemp AmbientTempSensor { get; private set; }
        public MotorTemp MotorTempSensor { get; private set; }
        public Temperature TemperatureSensor { get; private set; }
        public Pressure PressureSensor { get; private set; }
        public Humidity HumiditySensor { get; private set; }

        // Constructor
        public EnvironmentConditions()
        {
            AmbientTempSensor = new AmbientTemp();
            MotorTempSensor = new MotorTemp();
            TemperatureSensor = new Temperature();
            PressureSensor = new Pressure();
            HumiditySensor = new Humidity();
        }

        // Ortam Sıcaklığı Sensörü
        public class AmbientTemp : SensorBase
        {
            public AmbientTemp() : base("AmbientTemp", "Celsius", "°C") { }
        }

        // Motor Sıcaklığı Sensörü
        public class MotorTemp : SensorBase
        {
            public MotorTemp() : base("MotorTemp", "Celsius", "°C") { }
        }

        // Genel Sıcaklık Sensörü
        public class Temperature : SensorBase
        {
            public Temperature() : base("Temperature", "Celsius", "°C") { }
        }

        // Basınç Sensörü
        public class Pressure : SensorBase
        {
            public Pressure() : base("Pressure", "Pascal", "Pa") { }
        }

        // Nem Sensörü
        public class Humidity : SensorBase
        {
            public Humidity() : base("Humidity", "Percentage", "%") { }
        }
    }
}
