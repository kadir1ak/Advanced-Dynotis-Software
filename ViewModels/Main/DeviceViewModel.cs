using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Advanced_Dynotis_Software.Models.Dynotis;
using LiveCharts;
using LiveCharts.Wpf;

namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class DeviceViewModel : INotifyPropertyChanged, IDisposable
    {
        public InterfaceVariables InterfaceVariables { get; set; }
        private bool _isUpdatingInterfaceVariables;

        public SeriesCollection VibrationSeriesCollection { get; set; }
        public SeriesCollection MotorSpeedSeriesCollection { get; set; }
        public SeriesCollection VoltageSeriesCollection { get; set; }
        public SeriesCollection CurrentSeriesCollection { get; set; }
        public SeriesCollection ThrustSeriesCollection { get; set; }
        public SeriesCollection TorqueSeriesCollection { get; set; }
        public ObservableCollection<string> TimeLabels { get; set; }
        public string DeviceDisplayName => $"{Device.Model} - {Device.SeriNo}";
        public Func<double, string> XAxisFormatter { get; set; }
        public Func<double, string> YAxisFormatter { get; set; }
        public double CurrentXAxisStep { get; set; }
        public double CurrentYAxisStep { get; set; }
        public double VoltageXAxisStep { get; set; }
        public double VoltageYAxisStep { get; set; }
        public double VibrationXAxisStep { get; set; }
        public double VibrationYAxisStep { get; set; }
        public double MotorSpeedXAxisStep { get; set; }
        public double MotorSpeedYAxisStep { get; set; }
        public double ThrustXAxisStep { get; set; }
        public double ThrustYAxisStep { get; set; }
        public double TorqueXAxisStep { get; set; }
        public double TorqueYAxisStep { get; set; }

        private Dynotis _device;
        private Queue<DynotisData> _dynotisDataBuffer;
        private readonly object _bufferLock = new();
        private const int BufferLimit = 10;
        private const int MaxDataPoints = 100;
        private bool _isUpdatingChart;
        private Task _chartUpdateTask;
        private Task _interfaceUpdateTask;
        private bool _isRunning;

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
                InterfaceVariables = new InterfaceVariables();
                Device = new Dynotis(portName);
                InitializeDeviceAsync();
                InitializeCharts();
                InitializeTasks();
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

            XAxisFormatter = value => value.ToString("0");
            YAxisFormatter = value => value.ToString("0.00");
        }

        private void InitializeDefaultChartData()
        {
            const double defaultValue = 100;
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

            UpdateChartSteps();
        }

        private static SeriesCollection CreateSeriesCollection(string title, Color color)
        {
            return new SeriesCollection
            {
                new LineSeries
                {
                    Title = title,
                    FontSize = 14,
                    Values = new ChartValues<double>(),
                    PointGeometrySize = 0,
                    LineSmoothness = 0, // Smoothing off
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 1, // Reduce stroke thickness
                    Fill = Brushes.Transparent, // No fill
                    PointForeground = Brushes.Transparent // No points
                }
            };
        }

        private void InitializeTasks()
        {
            _isRunning = true;
            _chartUpdateTask = Task.Run(() => UpdateChartDataLoop());
            _interfaceUpdateTask = Task.Run(() => UpdateInterfaceVariablesLoop());
        }

        private async void UpdateChartDataLoop()
        {
            while (_isRunning)
            {
                await Task.Delay(1); // Chart update interval

                DynotisData dynotisData = null;
                lock (_bufferLock)
                {
                    if (_dynotisDataBuffer.Count > 0)
                    {
                        dynotisData = _dynotisDataBuffer.Dequeue();
                    }
                }

                if (dynotisData != null && !_isUpdatingChart)
                {
                    _isUpdatingChart = true;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateChartData(dynotisData);
                        _isUpdatingChart = false;
                    });
                }
            }
        }

        private async void UpdateInterfaceVariablesLoop()
        {
            while (_isRunning)
            {
                await Task.Delay(20); // Interface update interval (50Hz)

                if (!_isUpdatingInterfaceVariables)
                {
                    DynotisData sensorData = null;
                    lock (_bufferLock)
                    {
                        if (_dynotisDataBuffer.Count > 0)
                        {
                            sensorData = _dynotisDataBuffer.Dequeue();
                        }
                    }

                    if (sensorData != null)
                    {
                        _isUpdatingInterfaceVariables = true;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            InterfaceVariables.UpdateFrom(sensorData);
                            _isUpdatingInterfaceVariables = false;
                        });
                    }
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

            UpdateChartSteps();
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

        private void UpdateSeries(SeriesCollection seriesCollection, double value)
        {
            var values = ((LineSeries)seriesCollection[0]).Values;
            if (values.Count >= MaxDataPoints)
            {
                values.RemoveAt(0);
            }
            values.Add(value);
        }

        private void UpdateChartSteps()
        {
            CurrentXAxisStep = CalculateXAxisStep(CurrentSeriesCollection);
            CurrentYAxisStep = CalculateYAxisStep(CurrentSeriesCollection);

            VoltageXAxisStep = CalculateXAxisStep(VoltageSeriesCollection);
            VoltageYAxisStep = CalculateYAxisStep(VoltageSeriesCollection);

            VibrationXAxisStep = CalculateXAxisStep(VibrationSeriesCollection);
            VibrationYAxisStep = CalculateYAxisStep(VibrationSeriesCollection);

            MotorSpeedXAxisStep = CalculateXAxisStep(MotorSpeedSeriesCollection);
            MotorSpeedYAxisStep = CalculateYAxisStep(MotorSpeedSeriesCollection);

            ThrustXAxisStep = CalculateXAxisStep(ThrustSeriesCollection);
            ThrustYAxisStep = CalculateYAxisStep(ThrustSeriesCollection);

            TorqueXAxisStep = CalculateXAxisStep(TorqueSeriesCollection);
            TorqueYAxisStep = CalculateYAxisStep(TorqueSeriesCollection);

            OnPropertyChanged(nameof(CurrentXAxisStep));
            OnPropertyChanged(nameof(CurrentYAxisStep));
            OnPropertyChanged(nameof(VoltageXAxisStep));
            OnPropertyChanged(nameof(VoltageYAxisStep));
            OnPropertyChanged(nameof(VibrationXAxisStep));
            OnPropertyChanged(nameof(VibrationYAxisStep));
            OnPropertyChanged(nameof(MotorSpeedXAxisStep));
            OnPropertyChanged(nameof(MotorSpeedYAxisStep));
            OnPropertyChanged(nameof(ThrustXAxisStep));
            OnPropertyChanged(nameof(ThrustYAxisStep));
            OnPropertyChanged(nameof(TorqueXAxisStep));
            OnPropertyChanged(nameof(TorqueYAxisStep));
        }

        private double CalculateXAxisStep(SeriesCollection seriesCollection)
        {
            var values = ((LineSeries)seriesCollection[0]).Values;
            return values.Count / 5.0; // X ekseninde 10 noktaya bölmek için
        }

        private double CalculateYAxisStep(SeriesCollection seriesCollection)
        {
            var values = ((LineSeries)seriesCollection[0]).Values;
            var max = values.Cast<double>().Max();
            var min = values.Cast<double>().Min();
            return (max - min) / 10.0; // Y ekseninde 20 noktaya bölmek için
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _isRunning = false;
        }
    }
}
