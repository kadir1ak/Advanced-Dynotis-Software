using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Advanced_Dynotis_Software.Models.Dynotis;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;


namespace Advanced_Dynotis_Software.ViewModels.Main
{
    public class ChartViewModel : INotifyPropertyChanged
    {
        public SeriesCollection VibrationSeriesCollection { get; private set; }
        public SeriesCollection MotorSpeedSeriesCollection { get; private set; }
        public SeriesCollection VoltageSeriesCollection { get; private set; }
        public SeriesCollection CurrentSeriesCollection { get; private set; }
        public SeriesCollection ThrustSeriesCollection { get; private set; }
        public SeriesCollection TorqueSeriesCollection { get; private set; }

        public ObservableCollection<string> TimeLabels { get; private set; }

        public Func<double, string> XAxisFormatter { get; private set; }
        public Func<double, string> YAxisFormatter { get; private set; }

        public ChartViewModel()
        {
            InitializeCharts();
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

            InitializeDefaultChartData();

            XAxisFormatter = value => DateTime.FromOADate(value).ToString("HH:mm:ss");
            YAxisFormatter = value => value.ToString("0.00");
        }

        private void InitializeDefaultChartData()
        {
            const double defaultValue = 100;
            DateTime now = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                TimeLabels.Add(now.AddSeconds(i).ToString("HH:mm:ss"));
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
                FontSize = 14,
                Values = new ChartValues<double>(),
                PointGeometrySize = 0,
                LineSmoothness = 0,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(10, color.R, color.G, color.B)),
                PointForeground = Brushes.Transparent,
                LabelPoint = point => point.Y.ToString("N1")
            }
        };
        }

        public void UpdateChartData(DynotisData sensorData)
        {
            DateTime now = DateTime.Now;

            if (TimeLabels.Count >= 100)
            {
                TimeLabels.RemoveAt(0);
            }
            TimeLabels.Add(now.ToString("HH:mm:ss"));

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
            if (values.Count >= 100)
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
