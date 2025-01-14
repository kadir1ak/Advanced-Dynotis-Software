using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OxyPlot;
using System.Windows;
using System.Reflection;
using Advanced_Dynotis_Software.Models.Devices.Device;
using Advanced_Dynotis_Software.Models.Plot;
using Advanced_Dynotis_Software.Services.BindableBase;
using Advanced_Dynotis_Software.Models.Devices.Sensors;
using LiveCharts.Wpf;

namespace Advanced_Dynotis_Software.Models.Interface
{
    public class InterfaceData : BindableBase
    {
        public InterfaceData(DeviceModel device)
        {
            Device = device;

            StartUpdateDataLoop();
            StartUpdatePlotLoop();
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
            Device.CurrentSensor.SetValue(device.CurrentSensor.GetValue());
            Device.VoltageSensor.SetValue(device.VoltageSensor.GetValue());
            Device.ThrustSensor.SetValue(device.ThrustSensor.GetValue());
            Device.TorqueSensor.SetValue(device.TorqueSensor.GetValue());
            Device.MotorSpeedSensor.SetValue(device.MotorSpeedSensor.GetValue());
            Device.AnemometerSensor.WindSpeedSensor.SetValue(device.AnemometerSensor.WindSpeedSensor.GetValue());
            Device.AnemometerSensor.WindDirectionSensor.SetValue(device.AnemometerSensor.WindDirectionSensor.GetValue());
            Device.VibrationSensor.SetVibrationValues(Device.VibrationSensor.X, Device.VibrationSensor.Y, Device.VibrationSensor.Z);
            Device.EnvironmentalConditions.AmbientTempSensor.SetValue(device.EnvironmentalConditions.AmbientTempSensor.GetValue());
            Device.EnvironmentalConditions.MotorTempSensor.SetValue(device.EnvironmentalConditions.MotorTempSensor.GetValue());
            Device.EnvironmentalConditions.PressureSensor.SetValue(device.EnvironmentalConditions.PressureSensor.GetValue());
            Device.EnvironmentalConditions.HumiditySensor.SetValue(device.EnvironmentalConditions.HumiditySensor.GetValue());
        }

        #region Update Data Loop (Veri Güncelleme Döngüsü)

        private CancellationTokenSource _updateDataLoopCancellationTokenSource;
        private int DataUpdateTimeMillisecond = 100; // 10 Hz (100ms)

        private async Task UpdateDataLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(DataUpdateTimeMillisecond, token);

                    // Verileri güncelle
                    UpdateDeviceData(Device);
                }
            }
            catch (TaskCanceledException)
            {
                // Döngü iptal edildiğinde hata atılmaz
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Data update loop error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StartUpdateDataLoop()
        {
            StopUpdateDataLoop();
            _updateDataLoopCancellationTokenSource = new CancellationTokenSource();
            var token = _updateDataLoopCancellationTokenSource.Token;
            _ = UpdateDataLoop(token);
        }

        public void StopUpdateDataLoop()
        {
            if (_updateDataLoopCancellationTokenSource != null && !_updateDataLoopCancellationTokenSource.IsCancellationRequested)
            {
                _updateDataLoopCancellationTokenSource.Cancel();
                _updateDataLoopCancellationTokenSource.Dispose();
                _updateDataLoopCancellationTokenSource = null;
            }
        }

        #endregion

        #region Update Plot Loop (Grafik Güncelleme Döngüsü)

        private CancellationTokenSource _updatePlotLoopCancellationTokenSource;
        private int PlotUpdateTimeMillisecond = 10; // 100 Hz (10ms)

        private async Task UpdatePlotLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(PlotUpdateTimeMillisecond, token);

                    // Grafik güncellemeleri
                    UpdateCharts(Device);
                }
            }
            catch (TaskCanceledException)
            {
                // Döngü iptal edildiğinde hata atılmaz
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Plot update loop error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StartUpdatePlotLoop()
        {
            StopUpdatePlotLoop();
            _updatePlotLoopCancellationTokenSource = new CancellationTokenSource();
            var token = _updatePlotLoopCancellationTokenSource.Token;
            _ = UpdatePlotLoop(token);
        }

        public void StopUpdatePlotLoop()
        {
            if (_updatePlotLoopCancellationTokenSource != null && !_updatePlotLoopCancellationTokenSource.IsCancellationRequested)
            {
                _updatePlotLoopCancellationTokenSource.Cancel();
                _updatePlotLoopCancellationTokenSource.Dispose();
                _updatePlotLoopCancellationTokenSource = null;
            }
        }

        #endregion
    }
}
