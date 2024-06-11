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
        public SeriesCollection VoltageSeriesCollection { get; set; }
        public SeriesCollection ThrustSeriesCollection { get; set; }
        public SeriesCollection TorqueSeriesCollection { get; set; }
        public ObservableCollection<string> TimeLabels { get; set; }

        private Dynotis device;
        private Queue<SensorData> _sensorDataBuffer;
        private object _bufferLock = new object();
        private const int BufferLimit = 1;
        private const int MaxDataPoints = 100;

        public Dynotis Device
        {
            get => device;
            set
            {
                if (device != value)
                {
                    device = value;
                    OnPropertyChanged(nameof(Device));
                }
            }
        }

        public DeviceViewModel(string portName)
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Device = new Dynotis(portName);
                InitializeDeviceAsync();
                Charts_InitializeComponent();
            }
        }

        private async void InitializeDeviceAsync()
        {
            await Device.OpenPortAsync();  // Correct usage of asynchronous method
        }

        private void Charts_InitializeComponent()
        {
            AmbientTempSeriesCollection = CreateSeriesCollection("Ambient Temperature", Colors.IndianRed);
            MotorTempSeriesCollection = CreateSeriesCollection("Motor Temperature", Colors.DarkOliveGreen);
            MotorSpeedSeriesCollection = CreateSeriesCollection("Motor Speed", Colors.PaleVioletRed);
            VoltageSeriesCollection = CreateSeriesCollection("Voltage", Colors.Orange);
            ThrustSeriesCollection = CreateSeriesCollection("Thrust", Colors.DarkOliveGreen);
            TorqueSeriesCollection = CreateSeriesCollection("Torque", Colors.HotPink);

            TimeLabels = new ObservableCollection<string>();
            _sensorDataBuffer = new Queue<SensorData>();

            Device.PropertyChanged += Device_PropertyChanged;

            InitializeDefaultChartData();
            _ = UpdateChartDataAsync();  // Asynchronous chart data update
        }

        private void InitializeDefaultChartData()
        {
            double defaultValue = 50;
            for (int i = 0; i < MaxDataPoints; i++)
            {
                TimeLabels.Add(i.ToString());
                UpdateSeries(AmbientTempSeriesCollection, defaultValue);
                UpdateSeries(MotorTempSeriesCollection, defaultValue);
                UpdateSeries(MotorSpeedSeriesCollection, defaultValue);
                UpdateSeries(VoltageSeriesCollection, defaultValue);
                UpdateSeries(ThrustSeriesCollection, defaultValue);
                UpdateSeries(TorqueSeriesCollection, defaultValue);
            }
        }

        private SeriesCollection CreateSeriesCollection(string title, Color color)
        {
            return new SeriesCollection
            {
                new LineSeries
                {
                    Title = title,
                    Values = new ChartValues<double>(),
                    PointGeometrySize = 0,
                    LineSmoothness = 1,
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(Color.FromArgb(10, color.R, color.G, color.B)),
                    PointForeground = new SolidColorBrush(Colors.Black),
                    LabelPoint = point => point.Y.ToString("N1")
                }
            };
        }

        private async Task UpdateChartDataAsync()
        {
            while (true)
            {
                SensorData sensorData = null;
                lock (_bufferLock)
                {
                    if (_sensorDataBuffer.Count > 0)
                    {
                        sensorData = _sensorDataBuffer.Dequeue();
                    }
                }

                if (sensorData != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        UpdateChartData(sensorData);
                    });
                }

                await Task.Delay(10);
            }
        }

        private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Dynotis.SensorData))
            {
                lock (_bufferLock)
                {
                    if (_sensorDataBuffer.Count >= BufferLimit)
                    {
                        _sensorDataBuffer.Dequeue();  // Remove the oldest data
                    }
                    _sensorDataBuffer.Enqueue(Device.SensorData);  // Add new data
                }
            }
        }

        private void UpdateChartData(SensorData sensorData)
        {
            if (TimeLabels.Count >= MaxDataPoints)
            {
                TimeLabels.RemoveAt(0);
            }
            TimeLabels.Add(sensorData.Time.ToString());

            UpdateSeries(AmbientTempSeriesCollection, sensorData.AmbientTemp);
            UpdateSeries(MotorTempSeriesCollection, sensorData.MotorTemp);
            UpdateSeries(MotorSpeedSeriesCollection, sensorData.MotorSpeed);
            UpdateSeries(VoltageSeriesCollection, sensorData.Voltage);
            UpdateSeries(ThrustSeriesCollection, sensorData.Thrust);
            UpdateSeries(TorqueSeriesCollection, sensorData.Torque);
        }

        private void UpdateSeries(SeriesCollection seriesCollection, double value)
        {
            var values = ((LineSeries)seriesCollection[0]).Values;
            if (values.Count >= MaxDataPoints)
            {
                values.RemoveAt(0);
            }
            values.Add(value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
