using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using DocumentFormat.OpenXml.Drawing.Charts;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{

    public class BalancerRoutingStepsVibrationLevelsViewModel : INotifyPropertyChanged
    {
        private InterfaceVariables _interfaceVariables;
        private DynotisData _dynotisData;

        private double _balancerIterationStep;
        private ObservableCollection<double> _balancerIterationStepChart;
        private ObservableCollection<double> _balancerIterationVibrationsChart;
        public BalancerRoutingStepsVibrationLevelsViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;

            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

            _balancerIterationStep = 0;
            _balancerIterationStepChart = new ObservableCollection<double>();
            _balancerIterationVibrationsChart = new ObservableCollection<double>();

            _maxYAxisValue = 1;
            _separatorStep = 0.2;

            VibrationSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Vibration Levels",
                    Values = new ChartValues<ObservablePoint>()
                }
            };

            XFormatter = value => value.ToString("0.0");
            YFormatter = value => value.ToString("0.000");
        }

        private void InterfaceVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InterfaceVariables.BalancerIterationStep) ||
                e.PropertyName == nameof(InterfaceVariables.BalancerIterationStepChart) ||
                e.PropertyName == nameof(InterfaceVariables.BalancerIterationVibrationsChart))
            {
                BalancerIterationStep = _interfaceVariables.BalancerIterationStep;
                BalancerIterationStepChart = _interfaceVariables.BalancerIterationStepChart;
                BalancerIterationVibrationsChart = _interfaceVariables.BalancerIterationVibrationsChart;
                UpdateVibrationChart();
            }
        }

        public ObservableCollection<double> BalancerIterationStepChart
        {
            get => _balancerIterationStepChart;
            set
            {
                if (SetProperty(ref _balancerIterationStepChart, value))
                {
                    OnPropertyChanged(nameof(BalancerIterationStepChart));
                }
            }
        }

        public ObservableCollection<double> BalancerIterationVibrationsChart
        {
            get => _balancerIterationVibrationsChart;
            set
            {
                if (SetProperty(ref _balancerIterationVibrationsChart, value))
                {
                    OnPropertyChanged(nameof(BalancerIterationVibrationsChart));
                }
            }
        }

        public double BalancerIterationStep
        {
            get => _balancerIterationStep;
            set
            {
                if (SetProperty(ref _balancerIterationStep, value))
                {
                    OnPropertyChanged(nameof(BalancerIterationStep));
                    UpdateVibrationChart();
                }
            }
        }

        private double _maxYAxisValue;
        public double MaxYAxisValue
        {
            get => _maxYAxisValue;
            set => SetProperty(ref _maxYAxisValue, value);
        }

        private double _separatorStep;
        public double SeparatorStep
        {
            get => _separatorStep;
            set => SetProperty(ref _separatorStep, value);
        }
        public SeriesCollection VibrationSeriesCollection { get; set; }
        public Func<double, string> XFormatter { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private void UpdateVibrationChart()
        {
            if (VibrationSeriesCollection != null && VibrationSeriesCollection.Count > 0)
            {
                var series = VibrationSeriesCollection[0] as LineSeries;
                if (series != null)
                {
                    series.Values.Clear();

                    double maxVibrationValue = 0;
                    for (int i = 0; i < BalancerIterationStep; i++)
                    {
                        if (i < BalancerIterationStep)
                        {
                            var point = new ObservablePoint(BalancerIterationStepChart[i], BalancerIterationVibrationsChart[i]);
                            series.Values.Add(point);
                            if (point.Y > maxVibrationValue)
                            {
                                maxVibrationValue = point.Y;
                            }
                        }
                    }
                    if (maxVibrationValue != 0)
                    {
                        // Set the maximum value of the Y axis to double the max vibration value
                        MaxYAxisValue = maxVibrationValue * 2;

                        // Set the separator step to MaxYAxisValue / 5
                        SeparatorStep = MaxYAxisValue / 5;
                    }

                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}



