using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using Advanced_Dynotis_Software.Models.Dynotis;
using System.Windows.Media;

namespace Advanced_Dynotis_Software.ViewModels.Device
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        public SeriesCollection AmbientTempSeriesCollection { get; set; }
        public SeriesCollection MotorTempSeriesCollection { get; set; }
        public SeriesCollection MotorSpeedSeriesCollection { get; set; }
        public SeriesCollection ThrustSeriesCollection { get; set; }
        public SeriesCollection TorqueSeriesCollection { get; set; }
        public ObservableCollection<string> TimeLabels { get; set; }

        private Dynotis device;
        private List<SensorData> _sensorDataBuffer;
        private object _bufferLock = new object();
        private DispatcherTimer _dispatcherTimer;
        private const int BufferLimit = 100;

        public Dynotis Device
        {
            get => device;
            set
            {
                device = value;
                OnPropertyChanged(nameof(Device));
            }
        }

        public DeviceViewModel(string portName)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Device = new Dynotis(portName);
                Device.OpenPort();

                AmbientTempSeriesCollection = new SeriesCollection
                {
                   new LineSeries
                   {
                        Title = "Ambient Temperature",
                        Values = new ChartValues<double>(),
                        PointGeometrySize = 0,
                        LineSmoothness = 0,
                        Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("orange"))
                   }

                };
                MotorTempSeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Motor Temperature",
                        Values = new ChartValues<double>(),
                        PointGeometrySize = 0,
                        LineSmoothness = 0,
                        Stroke = new SolidColorBrush(Colors.HotPink)
                    }
                };
                MotorSpeedSeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Motor Speed",
                        Values = new ChartValues<double>(),
                        PointGeometrySize = 0,
                        LineSmoothness = 0,
                        Stroke = new SolidColorBrush(Colors.PaleVioletRed)
                    }
                };
                ThrustSeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Thrust",
                        Values = new ChartValues<double>(),
                        PointGeometrySize = 0,
                        LineSmoothness = 0,
                        Stroke = new SolidColorBrush(Colors.DarkOliveGreen)
                    }
                };
                TorqueSeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Torque",
                        Values = new ChartValues<double>(),
                        PointGeometrySize = 0,
                        LineSmoothness = 0,
                        Stroke = new SolidColorBrush(Colors.IndianRed)
                    }
                };

                TimeLabels = new ObservableCollection<string>();
                _sensorDataBuffer = new List<SensorData>();

                Device.PropertyChanged += Device_PropertyChanged;

                _dispatcherTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(10) // 10 ms'de bir UI güncelle (100 Hz)
                };
                _dispatcherTimer.Tick += DispatcherTimer_Tick;
                _dispatcherTimer.Start();
            }
        }

        private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Dynotis.SensorData))
            {
                lock (_bufferLock)
                {
                    _sensorDataBuffer.Add(Device.SensorData);

                    if (_sensorDataBuffer.Count > BufferLimit)
                    {
                        _sensorDataBuffer.RemoveAt(0);
                    }
                }
            }
        }

        private async void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            List<SensorData> bufferCopy;

            lock (_bufferLock)
            {
                bufferCopy = new List<SensorData>(_sensorDataBuffer);
                _sensorDataBuffer.Clear();
            }

            await Task.Run(() =>
            {
                foreach (var sensorData in bufferCopy)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TimeLabels.Add(sensorData.Time.ToString());
                        AmbientTempSeriesCollection[0].Values.Add(sensorData.AmbientTemp);
                        MotorTempSeriesCollection[0].Values.Add(sensorData.MotorTemp);
                        MotorSpeedSeriesCollection[0].Values.Add(sensorData.MotorSpeed);
                        ThrustSeriesCollection[0].Values.Add(sensorData.Thrust);
                        TorqueSeriesCollection[0].Values.Add(sensorData.Torque);

                        if (TimeLabels.Count > 100)
                        {
                            TimeLabels.RemoveAt(0);
                            AmbientTempSeriesCollection[0].Values.RemoveAt(0);
                            MotorTempSeriesCollection[0].Values.RemoveAt(0);
                            MotorSpeedSeriesCollection[0].Values.RemoveAt(0);
                            ThrustSeriesCollection[0].Values.RemoveAt(0);
                            TorqueSeriesCollection[0].Values.RemoveAt(0);
                        }
                    });
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
