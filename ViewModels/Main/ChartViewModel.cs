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
        public SeriesCollection VibrationXSeriesCollection { get; private set; }
        public SeriesCollection VibrationYSeriesCollection { get; private set; }
        public SeriesCollection VibrationZSeriesCollection { get; private set; }
        public SeriesCollection VibrationHighSeriesCollection { get; private set; }
        public SeriesCollection MotorSpeedSeriesCollection { get; private set; }
        public SeriesCollection VoltageSeriesCollection { get; private set; }
        public SeriesCollection CurrentSeriesCollection { get; private set; }
        public SeriesCollection ThrustSeriesCollection { get; private set; }
        public SeriesCollection TorqueSeriesCollection { get; private set; }

        public ObservableCollection<string> TimeLabels { get; private set; }

        public Func<double, string> CurrentXAxisFormatter { get; private set; }
        public Func<double, string> CurrentYAxisFormatter { get; private set; }   

        public Func<double, string> VoltageXAxisFormatter { get; private set; }
        public Func<double, string> VoltageYAxisFormatter { get; private set; }  

        public Func<double, string> VibrationXAxisFormatter { get; private set; }
        public Func<double, string> VibrationYAxisFormatter { get; private set; } 

        public Func<double, string> MotorSpeedXAxisFormatter { get; private set; }
        public Func<double, string> MotorSpeedYAxisFormatter { get; private set; } 

        public Func<double, string> ThrustXAxisFormatter { get; private set; }
        public Func<double, string> ThrustYAxisFormatter { get; private set; }

        public Func<double, string> TorqueXAxisFormatter { get; private set; }
        public Func<double, string> TorqueYAxisFormatter { get; private set; }

        public double CurrentYAxisMin { get; private set; }
        public double CurrentYAxisMax { get; private set; }

        public double VoltageYAxisMin { get; private set; }
        public double VoltageYAxisMax { get; private set; }

        public double VibrationYAxisMin { get; private set; }
        public double VibrationYAxisMax { get; private set; }

        public double VibrationXYAxisMin { get; private set; }
        public double VibrationXYAxisMax { get; private set; }

        public double VibrationYYAxisMin { get; private set; }
        public double VibrationYYAxisMax { get; private set; }

        public double VibrationZYAxisMin { get; private set; }
        public double VibrationZYAxisMax { get; private set; }

        public double VibrationHighYAxisMin { get; private set; }
        public double VibrationHighYAxisMax { get; private set; }

        public double MotorSpeedYAxisMin { get; private set; }
        public double MotorSpeedYAxisMax { get; private set; }

        public double ThrustYAxisMin { get; private set; }
        public double ThrustYAxisMax { get; private set; }

        public double TorqueYAxisMin { get; private set; }
        public double TorqueYAxisMax { get; private set; }

        public double CurrentXAxisStep { get; private set; }
        public double CurrentYAxisStep { get; private set; }
        public double VoltageXAxisStep { get; private set; }
        public double VoltageYAxisStep { get; private set; }
        public double VibrationXAxisStep { get; private set; }
        public double VibrationYAxisStep { get; private set; }
        public double VibrationXXAxisStep { get; private set; }
        public double VibrationXYAxisStep { get; private set; }
        public double VibrationYXAxisStep { get; private set; }
        public double VibrationYYAxisStep { get; private set; }
        public double VibrationZXAxisStep { get; private set; }
        public double VibrationZYAxisStep { get; private set; }
        public double VibrationHighXAxisStep { get; private set; }
        public double VibrationHighYAxisStep { get; private set; }
        public double MotorSpeedXAxisStep { get; private set; }
        public double MotorSpeedYAxisStep { get; private set; }
        public double ThrustXAxisStep { get; private set; }
        public double ThrustYAxisStep { get; private set; }
        public double TorqueXAxisStep { get; private set; }
        public double TorqueYAxisStep { get; private set; }

        
        private int seriesBufferSize = 100;

        const double defaultValue = 100;

        public ChartViewModel()
        {
            InitializeCharts();
        }

        private void InitializeCharts()
        {
            VibrationSeriesCollection = CreateSeriesCollection("Vibration", Colors.IndianRed);

            VibrationXSeriesCollection = CreateSeriesCollection("VibrationX", Colors.Blue);
            VibrationYSeriesCollection = CreateSeriesCollection("VibrationY", Colors.Green);
            VibrationZSeriesCollection = CreateSeriesCollection("VibrationZ", Colors.Purple);
            VibrationHighSeriesCollection = CreateSeriesCollection("VibrationHigh", Colors.Orange);

            CurrentSeriesCollection = CreateSeriesCollection("Current", Colors.DarkOliveGreen);
            MotorSpeedSeriesCollection = CreateSeriesCollection("Motor Speed", Colors.PaleVioletRed);
            VoltageSeriesCollection = CreateSeriesCollection("Voltage", Colors.Orange);
            ThrustSeriesCollection = CreateSeriesCollection("Thrust", Colors.DarkOliveGreen);
            TorqueSeriesCollection = CreateSeriesCollection("Torque", Colors.HotPink);
            
            TimeLabels = new ObservableCollection<string>();

            InitializeDefaultChartData();

            CurrentXAxisFormatter = value => value.ToString("0.0");
            CurrentYAxisFormatter = value => value.ToString("0.00");

            VoltageXAxisFormatter = value => value.ToString("0.0");
            VoltageYAxisFormatter = value => value.ToString("0.00");

            VibrationXAxisFormatter = value => value.ToString("0.0");
            VibrationYAxisFormatter = value => value.ToString("0.000");

            MotorSpeedXAxisFormatter = value => value.ToString("0.0");
            MotorSpeedYAxisFormatter = value => value.ToString("0");

            ThrustXAxisFormatter = value => value.ToString("0.0");
            ThrustYAxisFormatter = value => value.ToString("0.000");

            TorqueXAxisFormatter = value => value.ToString("0.0");
            TorqueYAxisFormatter = value => value.ToString("0.000");
        }

        private void InitializeDefaultChartData()
        {
            for (int i = 0; i < seriesBufferSize; i++)
            {
                TimeLabels.Add(i.ToString());
                UpdateSeries(VibrationSeriesCollection, defaultValue);
                UpdateSeries(VibrationXSeriesCollection, defaultValue);
                UpdateSeries(VibrationYSeriesCollection, defaultValue);
                UpdateSeries(VibrationZSeriesCollection, defaultValue);
                UpdateSeries(VibrationHighSeriesCollection, defaultValue);
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
                    LineSmoothness = 0,
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 1,
                    Fill = new SolidColorBrush(Color.FromArgb(10, color.R, color.G, color.B)),
                    PointForeground = Brushes.Transparent,
                    LabelPoint = point => point.Y.ToString("N1")
                }
            };
        }

        public void UpdateChartData(InterfaceVariables data)
        {
            if (TimeLabels.Count >= seriesBufferSize)
            {
                TimeLabels.RemoveAt(0);
            }
            TimeLabels.Add(data.Time.ToString());
            UpdateSeries(VibrationSeriesCollection, data.Vibration);
            UpdateSeries(VibrationXSeriesCollection, data.VibrationX);
            UpdateSeries(VibrationYSeriesCollection, data.VibrationY);
            UpdateSeries(VibrationZSeriesCollection, data.VibrationZ);
            UpdateSeries(VibrationHighSeriesCollection, data.HighVibration);
            UpdateSeries(CurrentSeriesCollection, data.Current);
            UpdateSeries(MotorSpeedSeriesCollection, data.MotorSpeed.Value);
            UpdateSeries(VoltageSeriesCollection, data.Voltage);
            UpdateSeries(ThrustSeriesCollection, data.Thrust.Value);
            UpdateSeries(TorqueSeriesCollection, data.Torque.Value);
            UpdateChartSteps();
            UpdateChartLimits();
        }


        private void UpdateSeries(SeriesCollection seriesCollection, double value)
        {
            var values = ((LineSeries)seriesCollection[0]).Values;
            if (values.Count >= seriesBufferSize)
            {
                values.RemoveAt(0);
            }
            values.Add(value);
        }

        private void UpdateChartSteps()
        {
            CurrentXAxisStep = ((LineSeries)CurrentSeriesCollection[0]).Values.Count / 6.0;
            CurrentYAxisStep = (CurrentYAxisMax == CurrentYAxisMin) ? 1 : (CurrentYAxisMax - CurrentYAxisMin) / 10.0;

            VoltageXAxisStep = ((LineSeries)VoltageSeriesCollection[0]).Values.Count / 6.0;
            VoltageYAxisStep = (VoltageYAxisMax == VoltageYAxisMin) ? 1 : (VoltageYAxisMax - VoltageYAxisMin) / 10.0;

            MotorSpeedXAxisStep = ((LineSeries)MotorSpeedSeriesCollection[0]).Values.Count / 6.0;
            MotorSpeedYAxisStep = (MotorSpeedYAxisMax == MotorSpeedYAxisMin) ? 1 : (MotorSpeedYAxisMax - MotorSpeedYAxisMin) / 10.0;

            ThrustXAxisStep = ((LineSeries)ThrustSeriesCollection[0]).Values.Count / 6.0;
            ThrustYAxisStep = (ThrustYAxisMax == ThrustYAxisMin) ? 1 : (ThrustYAxisMax - ThrustYAxisMin) / 10.0;

            TorqueXAxisStep = ((LineSeries)TorqueSeriesCollection[0]).Values.Count / 6.0;
            TorqueYAxisStep = (TorqueYAxisMax == TorqueYAxisMin) ? 1 : (TorqueYAxisMax - TorqueYAxisMin) / 10.0;

            VibrationXAxisStep = ((LineSeries)VibrationSeriesCollection[0]).Values.Count / 6.0;
            VibrationYAxisStep = (VibrationYAxisMax == VibrationYAxisMin) ? 1 : (VibrationYAxisMax - VibrationYAxisMin) / 10.0;

            VibrationXXAxisStep = ((LineSeries)VibrationXSeriesCollection[0]).Values.Count / 1.0;
            VibrationXYAxisStep = (VibrationXYAxisMax == VibrationXYAxisMin) ? 1 : (VibrationXYAxisMax - VibrationXYAxisMin) / 2.0;

            VibrationYXAxisStep = ((LineSeries)VibrationYSeriesCollection[0]).Values.Count / 1.0;
            VibrationYYAxisStep = (VibrationYYAxisMax == VibrationYYAxisMin) ? 1 : (VibrationYYAxisMax - VibrationYYAxisMin) / 2.0;

            VibrationZXAxisStep = ((LineSeries)VibrationZSeriesCollection[0]).Values.Count / 1.0;
            VibrationZYAxisStep = (VibrationZYAxisMax == VibrationZYAxisMin) ? 1 : (VibrationZYAxisMax - VibrationZYAxisMin) / 2.0;

            VibrationHighXAxisStep = ((LineSeries)VibrationHighSeriesCollection[0]).Values.Count / 1.0;
            VibrationHighYAxisStep = (VibrationHighYAxisMax == VibrationHighYAxisMin) ? 1 : (VibrationHighYAxisMax - VibrationHighYAxisMin) / 2.0;

            OnPropertyChanged(nameof(CurrentXAxisStep));
            OnPropertyChanged(nameof(CurrentYAxisStep));
            OnPropertyChanged(nameof(VoltageXAxisStep));
            OnPropertyChanged(nameof(VoltageYAxisStep));
            OnPropertyChanged(nameof(VibrationXAxisStep));
            OnPropertyChanged(nameof(VibrationYAxisStep));
            OnPropertyChanged(nameof(VibrationXXAxisStep));
            OnPropertyChanged(nameof(VibrationXYAxisStep));
            OnPropertyChanged(nameof(VibrationYXAxisStep));
            OnPropertyChanged(nameof(VibrationYYAxisStep));
            OnPropertyChanged(nameof(VibrationZXAxisStep));
            OnPropertyChanged(nameof(VibrationZYAxisStep));
            OnPropertyChanged(nameof(VibrationHighXAxisStep));
            OnPropertyChanged(nameof(VibrationHighYAxisStep));
            OnPropertyChanged(nameof(MotorSpeedXAxisStep));
            OnPropertyChanged(nameof(MotorSpeedYAxisStep));
            OnPropertyChanged(nameof(ThrustXAxisStep));
            OnPropertyChanged(nameof(ThrustYAxisStep));
            OnPropertyChanged(nameof(TorqueXAxisStep));
            OnPropertyChanged(nameof(TorqueYAxisStep));
        }
        private void UpdateChartLimits()
        {
            CurrentYAxisMin = CurrentSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
            CurrentYAxisMax = Math.Max(CurrentSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 5);

            VoltageYAxisMin = VoltageSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
            VoltageYAxisMax = Math.Max(VoltageSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 10);

            MotorSpeedYAxisMin = MotorSpeedSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
            MotorSpeedYAxisMax = Math.Max(MotorSpeedSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 1000);

            ThrustYAxisMin = ThrustSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
            ThrustYAxisMax = Math.Max(ThrustSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 500);

            TorqueYAxisMin = TorqueSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
            TorqueYAxisMax = Math.Max(TorqueSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 500);

            VibrationYAxisMin = VibrationSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
            VibrationYAxisMax = Math.Max(VibrationSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 1);

            VibrationXYAxisMin = VibrationXSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
            VibrationXYAxisMax = Math.Max(VibrationXSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 1);

            VibrationYYAxisMin = VibrationYSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
            VibrationYYAxisMax = Math.Max(VibrationYSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 1);

            VibrationZYAxisMin = VibrationZSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
            VibrationZYAxisMax = Math.Max(VibrationZSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 1);

            VibrationHighYAxisMin = VibrationHighSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Min() / 2.0;
            VibrationHighYAxisMax = Math.Max(VibrationHighSeriesCollection.SelectMany(series => series.Values.Cast<double>()).Max() * 1.5, 1);

            OnPropertyChanged(nameof(CurrentYAxisMin));
            OnPropertyChanged(nameof(CurrentYAxisMax));
            OnPropertyChanged(nameof(VoltageYAxisMin));
            OnPropertyChanged(nameof(VoltageYAxisMax));
            OnPropertyChanged(nameof(MotorSpeedYAxisMin));
            OnPropertyChanged(nameof(MotorSpeedYAxisMax));
            OnPropertyChanged(nameof(ThrustYAxisMin));
            OnPropertyChanged(nameof(ThrustYAxisMax));
            OnPropertyChanged(nameof(TorqueYAxisMin));
            OnPropertyChanged(nameof(TorqueYAxisMax));     
            OnPropertyChanged(nameof(VibrationYAxisMin));
            OnPropertyChanged(nameof(VibrationYAxisMax));  
            OnPropertyChanged(nameof(VibrationXYAxisMin));
            OnPropertyChanged(nameof(VibrationXYAxisMax));  
            OnPropertyChanged(nameof(VibrationYYAxisMin));
            OnPropertyChanged(nameof(VibrationYYAxisMax));  
            OnPropertyChanged(nameof(VibrationZYAxisMin));
            OnPropertyChanged(nameof(VibrationZYAxisMax));  
            OnPropertyChanged(nameof(VibrationHighYAxisMin));
            OnPropertyChanged(nameof(VibrationHighYAxisMax));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
