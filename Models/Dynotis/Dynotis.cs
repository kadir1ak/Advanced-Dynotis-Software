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
            Port = new SerialPort(_name , 115200);
            SensorData = new SensorData();
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                string indata = sp.ReadLine();
                var dataReceived = JsonConvert.DeserializeObject<SensorData>(indata);
                if (dataReceived != null)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        SensorData.Time = dataReceived.Time;
                        SensorData.AmbientTemp = dataReceived.AmbientTemp;
                        SensorData.MotorTemp = dataReceived.MotorTemp;
                        SensorData.MotorSpeed = dataReceived.MotorSpeed;
                        SensorData.Thrust = dataReceived.Thrust;
                        SensorData.Torque = dataReceived.Torque;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void OpenPort()
        {
            if (Port != null && Port.IsOpen == false)
            {
                Port.DataReceived += DataReceivedHandler;
                Port.Open();
                MessageBox.Show("(OpenPort): " + Port.PortName);
            }
        }

        public void ClosePort()
        {
            if (Port != null && Port.IsOpen == true)
            {
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
