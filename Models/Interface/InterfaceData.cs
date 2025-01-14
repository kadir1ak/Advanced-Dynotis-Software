using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OxyPlot;
using System.Windows;
using System.Reflection;
using Advanced_Dynotis_Software.Models.Device.Sensors;
using DeviceModel = Advanced_Dynotis_Software.Models.Device.Device;
using Advanced_Dynotis_Software.Models.Plot;
using Advanced_Dynotis_Software.Services.BindableBase;

namespace Advanced_Dynotis_Software.Models.Interface
{
    public class InterfaceData : BindableBase
    {
        public InterfaceData()
        {
            Device = new DeviceModel();
        }

        private DeviceModel _device;
        public DeviceModel Device
        {
            get => _device;
            set => SetProperty(ref _device, value);
        }

        private Dictionary<string, Chart> _sensorCharts = new();
        public Dictionary<string, Chart> SensorCharts
        {
            get => _sensorCharts;
            set => SetProperty(ref _sensorCharts, value);
        }

        public void InitializeCharts(DeviceModel device)
        {
            AddChartForSensor(device.VoltageSensor, OxyColors.Red);
            AddChartForSensor(device.CurrentSensor, OxyColors.Blue);
            AddChartForSensor(device.TorqueSensor, OxyColors.Green);
            AddChartForSensor(device.ThrustSensor, OxyColors.Orange);
            AddChartForSensor(device.VibrationSensor, OxyColors.Purple);
            AddChartForSensor(device.MotorSpeedSensor, OxyColors.Brown);
        }

        private void AddChartForSensor(SensorBase sensor, OxyColor color)
        {
            if (sensor != null)
            {
                var chart = new Chart(sensor.Name, sensor.UnitName, color);
                SensorCharts[sensor.Name] = chart;
            }
        }

        public void UpdateCharts(DeviceModel device)
        {
            foreach (var chart in SensorCharts.Values)
            {
                // Sensör adını kullanarak cihazın ilgili sensörünü bul
                var sensorProperty = device.GetType().GetProperty(chart.SensorName);
                if (sensorProperty != null)
                {
                    // Sensör verisini al
                    var sensor = sensorProperty.GetValue(device) as SensorBase;
                    if (sensor != null)
                    {
                        // Grafiği güncelle
                        chart.UpdateSensorData(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond, sensor.Value);
                        chart.RefreshPlot();
                    }
                }
            }
        }

        public void UpdateDeviceData(DeviceModel device)
        {
            Device.PortReadData = device.PortReadData;
            Device.PortReadTime = device.PortReadTime;
            Device.ThrustSensor.SetValue(device.ThrustSensor.GetValue());
            Device.TorqueSensor.SetValue(device.TorqueSensor.GetValue());
            Device.CurrentSensor.SetValue(device.CurrentSensor.GetValue());
            Device.VoltageSensor.SetValue(device.VoltageSensor.GetValue());
        }
    }
}
