using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Advanced_Dynotis_Software.Models.Dynotis;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media;

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
        public string DeviceDisplayName => $"{Device.Model} - {Device.SeriNo}";

        private Dynotis _device;
        private Queue<DynotisData> _dynotisDataBuffer;
        private readonly object _bufferLock = new();
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
                InitializeCharts();
            }
        }

        private async void InitializeDeviceAsync()
        {
            await Device.OpenPortAsync();
        }

        private void InitializeCharts()
        {
            VibrationSeriesCollection = CreateSeriesCollection("Vibration", Colors.IndianRed);
            CurrentSeriesCollection = CreateSeriesCollection("Current", Colors.DarkOliveGreen);
            MotorSpeedSeriesCollection = CreateSeriesCollection("Motor Speed", Colors.PaleVioletRed);
            VoltageSeriesCollection = CreateSeriesCollection("Voltage", Colors.Orange);
            ThrustSeriesCollection = CreateSeriesCollection("Thrust", Colors.DarkOliveGreen);
            TorqueSeriesCollection = CreateSeriesCollection("Torque", Colors.HotPink);

            TimeLabels = new ObservableCollection<string>();
            _dynotisDataBuffer = new Queue<DynotisData>();

            Device.PropertyChanged += Device_PropertyChanged;

            InitializeDefaultChartData();
            _ = UpdateChartDataAsync();
        }

        private void InitializeDefaultChartData()
        {
            const double defaultValue = 50;
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

        private static SeriesCollection CreateSeriesCollection(string title, Color color)
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
                DynotisData dynotisData = null;
                lock (_bufferLock)
                {
                    if (_dynotisDataBuffer.Count > 0)
                    {
                        dynotisData = _dynotisDataBuffer.Dequeue();
                    }
                }

                if (dynotisData != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        UpdateChartData(dynotisData);
                    });
                }

                await Task.Delay(10);
            }
        }

        private void Device_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Dynotis.DynotisData))
            {
                lock (_bufferLock)
                {
                    if (_dynotisDataBuffer.Count >= BufferLimit)
                    {
                        _dynotisDataBuffer.Dequeue();
                    }
                    _dynotisDataBuffer.Enqueue(Device.DynotisData);
                }
            }
        }

        private void UpdateChartData(DynotisData sensorData)
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

        private static void UpdateSeries(SeriesCollection seriesCollection, double value)
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
