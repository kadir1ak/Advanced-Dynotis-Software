using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Advanced_Dynotis_Software.Models.Dynotis;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Threading;
using System.Windows;

namespace Advanced_Dynotis_Software.ViewModels.Device
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        public SeriesCollection VibrationSeriesCollection { get; set; }
        public SeriesCollection MotorSpeedSeriesCollection { get; set; }
        public SeriesCollection VoltageSeriesCollection { get; set; }
        public SeriesCollection CurrentSeriesCollection { get; set; }
        public SeriesCollection ThrustSeriesCollection { get; set; }
        public SeriesCollection TorqueSeriesCollection { get; set; }
        public ObservableCollection<string> TimeLabels { get; set; }

        private Dynotis _device;
        private Queue<SensorData> _sensorDataBuffer;
        private object _bufferLock = new object();
        private const int BufferLimit = 1;
        private const int MaxDataPoints = 100;

        public Dynotis Device
        {
            get => _device;
            set
            {
                if (_device != value)
                {
                    _device = value;
                    OnPropertyChanged();
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
            await Device.OpenPortAsync();
        }

        private void Charts_InitializeComponent()
        {
            VibrationSeriesCollection = CreateSeriesCollection("Vibration", System.Windows.Media.Colors.IndianRed);
            CurrentSeriesCollection = CreateSeriesCollection("Current", System.Windows.Media.Colors.DarkOliveGreen);
            MotorSpeedSeriesCollection = CreateSeriesCollection("Motor Speed", System.Windows.Media.Colors.PaleVioletRed);
            VoltageSeriesCollection = CreateSeriesCollection("Voltage", System.Windows.Media.Colors.Orange);
            ThrustSeriesCollection = CreateSeriesCollection("Thrust", System.Windows.Media.Colors.DarkOliveGreen);
            TorqueSeriesCollection = CreateSeriesCollection("Torque", System.Windows.Media.Colors.HotPink);

            TimeLabels = new ObservableCollection<string>();
            _sensorDataBuffer = new Queue<SensorData>();

            Device.PropertyChanged += Device_PropertyChanged;

            InitializeDefaultChartData();
            _ = UpdateChartDataAsync();
        }

        private void InitializeDefaultChartData()
        {
            double defaultValue = 50;
            for (int i = 0; i < MaxDataPoints; i++)
            {
                TimeLabels.Add(i.ToString());
                UpdateSeries(VibrationSeriesCollection, defaultValue);
                UpdateSeries(CurrentSeriesCollection, defaultValue);
                UpdateSeries(MotorSpeedSeriesCollection, defaultValue);
                UpdateSeries(VoltageSeriesCollection, defaultValue);
                UpdateSeries(ThrustSeriesCollection, defaultValue);
                UpdateSeries(TorqueSeriesCollection, defaultValue);
            }
        }

        private SeriesCollection CreateSeriesCollection(string title, System.Windows.Media.Color color)
        {
            return new SeriesCollection
            {
                new LineSeries
                {
                    Title = title,
                    Values = new ChartValues<double>(),
                    PointGeometrySize = 0,
                    LineSmoothness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(color),
                    StrokeThickness = 2,
                    Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(10, color.R, color.G, color.B)),
                    PointForeground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black),
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
                        _sensorDataBuffer.Dequeue();
                    }
                    _sensorDataBuffer.Enqueue(Device.SensorData);
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
            UpdateSeries(VibrationSeriesCollection, sensorData.Vibration);
            UpdateSeries(CurrentSeriesCollection, sensorData.Current);
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
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
