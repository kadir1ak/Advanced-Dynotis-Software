using System.ComponentModel;

namespace Advanced_Dynotis_Software.Models.Dynotis
{
    public class SensorData : INotifyPropertyChanged
    {
        private int time;
        private double ambientTemp;
        private double motorTemp;
        private double motorSpeed;
        private double thrust;
        private double torque;
        private double voltage;
        private double current;
        private double pressure;
        private double vibration;
        private double windSpeed;
        private double windDirection;

        public int Time
        {
            get => time;
            set
            {
                if (time != value)
                {
                    time = value;
                    OnPropertyChanged(nameof(Time));
                }
            }
        }

        public double AmbientTemp
        {
            get => ambientTemp;
            set
            {
                if (ambientTemp != value)
                {
                    ambientTemp = value;
                    OnPropertyChanged(nameof(AmbientTemp));
                }
            }
        }

        public double MotorTemp
        {
            get => motorTemp;
            set
            {
                if (motorTemp != value)
                {
                    motorTemp = value;
                    OnPropertyChanged(nameof(MotorTemp));
                }
            }
        }

        public double MotorSpeed
        {
            get => motorSpeed;
            set
            {
                if (motorSpeed != value)
                {
                    motorSpeed = value;
                    OnPropertyChanged(nameof(MotorSpeed));
                }
            }
        }

        public double Thrust
        {
            get => thrust;
            set
            {
                if (thrust != value)
                {
                    thrust = value;
                    OnPropertyChanged(nameof(Thrust));
                }
            }
        }

        public double Torque
        {
            get => torque;
            set
            {
                if (torque != value)
                {
                    torque = value;
                    OnPropertyChanged(nameof(Torque));
                }
            }
        }

        public double Voltage
        {
            get => voltage;
            set
            {
                if (voltage != value)
                {
                    voltage = value;
                    OnPropertyChanged(nameof(Voltage));
                }
            }
        }

        public double Current
        {
            get => current;
            set
            {
                if (current != value)
                {
                    current = value;
                    OnPropertyChanged(nameof(Current));
                }
            }
        }

        public double Pressure
        {
            get => pressure;
            set
            {
                if (pressure != value)
                {
                    pressure = value;
                    OnPropertyChanged(nameof(Pressure));
                }
            }
        }

        public double Vibration
        {
            get => vibration;
            set
            {
                if (vibration != value)
                {
                    vibration = value;
                    OnPropertyChanged(nameof(Vibration));
                }
            }
        }

        public double WindSpeed
        {
            get => windSpeed;
            set
            {
                if (windSpeed != value)
                {
                    windSpeed = value;
                    OnPropertyChanged(nameof(WindSpeed));
                }
            }
        }

        public double WindDirection
        {
            get => windDirection;
            set
            {
                if (windDirection != value)
                {
                    windDirection = value;
                    OnPropertyChanged(nameof(WindDirection));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
