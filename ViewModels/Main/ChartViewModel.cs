﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Advanced_Dynotis_Software.Models.Dynotis;
using LiveCharts;
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

        private int seriesBufferSize = 100;

        const double defaultValue = 100;

        public double CurrentXAxisStep { get; private set; }
        public double CurrentYAxisStep { get; private set; }
        public double VoltageXAxisStep { get; private set; }
        public double VoltageYAxisStep { get; private set; }
        public double VibrationXAxisStep { get; private set; }
        public double VibrationYAxisStep { get; private set; }
        public double MotorSpeedXAxisStep { get; private set; }
        public double MotorSpeedYAxisStep { get; private set; }
        public double ThrustXAxisStep { get; private set; }
        public double ThrustYAxisStep { get; private set; }
        public double TorqueXAxisStep { get; private set; }
        public double TorqueYAxisStep { get; private set; }

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

            XAxisFormatter = value => value.ToString("0");
            YAxisFormatter = value => value.ToString("0.00");
        }

        private void InitializeDefaultChartData()
        {
            for (int i = 0; i < seriesBufferSize; i++)
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
            if (TimeLabels.Count >= seriesBufferSize)
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
            return values.Count / 4.0; // X ekseninde 4 noktaya bölmek için
        }

        private double CalculateYAxisStep(SeriesCollection seriesCollection)
        {
            var values = ((LineSeries)seriesCollection[0]).Values;
            var max = values.Cast<double>().Max();
            var min = values.Cast<double>().Min();
            return (max == min) ? 1 : (max - min) / 4.0; // Y ekseninde 4 noktaya bölmek için
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
