using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows;


namespace Advanced_Dynotis_Software.Models.Dynotis
{
    public class Dynotis : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public SerialPort Port { get; set; }

        private Thread DeviceThread;

        private SensorData sensorData;
        public SensorData SensorData
        {
            get => sensorData;
            set
            {
                sensorData = value;
                OnPropertyChanged(nameof(SensorData));
            }
        }
        
        public Dynotis(string _name)
        {
            Name = _name;
            Port = new SerialPort(_name, 921600);
            SensorData = new SensorData();
            DeviceThread = new Thread(DeviceDataReceived);
            DeviceThread.IsBackground = true;
            DeviceThread.Start();
        }
        private void DeviceDataReceived()
        {
            try
            {
                while (true)
                {
                    if (Port.IsOpen)
                    {
                        string indata = Port.ReadLine();

                        // Split the received CSV string by comma
                        string[] dataParts = indata.Split(',');

                        // Ensure that we have received all expected data parts
                        if (dataParts.Length == 6)
                        {
                            // Parse each part and update the sensor data
                            int time = int.Parse(dataParts[0]);
                            int ambientTemp = int.Parse(dataParts[1]);
                            int motorTemp = int.Parse(dataParts[2]);
                            int motorSpeed = int.Parse(dataParts[3]);
                            int thrust = int.Parse(dataParts[4]);
                            int torque = int.Parse(dataParts[5]);

                            // Use a temporary variable to avoid repeated UI thread dispatching
                            var newData = new SensorData
                            {
                                Time = time,
                                AmbientTemp = ambientTemp,
                                MotorTemp = motorTemp,
                                MotorSpeed = motorSpeed,
                                Thrust = thrust,
                                Torque = torque
                            };

                            // Dispatch the UI update only once after all properties are set
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                SensorData = newData;
                            });
                        }
                        else
                        {
                            // If the received data does not match the expected format, handle the error
                            throw new Exception("Received data does not match the expected format.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions gracefully
                MessageBox.Show(ex.Message);
            }
        }

        public void OpenPort()
        {
            if (Port != null && !Port.IsOpen)
            {
                //Port.DataReceived += DataReceivedHandler;
                Port.Open();
                MessageBox.Show("(OpenPort): " + Port.PortName);
            }
        }

        public void ClosePort()
        {
            if (Port != null && Port.IsOpen)
            {
                //Port.DataReceived -= DataReceivedHandler; // Unsubscribe from DataReceived event
                Port.Close();
                MessageBox.Show("(ClosePort): " + Port.PortName);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
