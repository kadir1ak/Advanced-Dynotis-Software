using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{

    public class BalancerRoutingStepsVibrationLevelsViewModel : INotifyPropertyChanged
    {
        private InterfaceVariables _interfaceVariables;
        private DynotisData _dynotisData;

        private int _balancerIterationStep;
        private ObservableCollection<int> _balancerIterationStepChart;
        private ObservableCollection<double> _balancerIterationVibrationsChart;
        public BalancerRoutingStepsVibrationLevelsViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;

            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

            _balancerIterationStep = 0;
            _balancerIterationStepChart = new ObservableCollection<int>();
            _balancerIterationVibrationsChart = new ObservableCollection<double>();
            

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

        public ObservableCollection<int> BalancerIterationStepChart
        {
            get => _balancerIterationStepChart;
            set
            {
                if (SetProperty(ref _balancerIterationStepChart, value))
                {
                    OnPropertyChanged(nameof(BalancerIterationStepChart));
                    UpdateVibrationChart();
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
                    UpdateVibrationChart();
                }
            }
        }

        public int BalancerIterationStep
        {
            get => _balancerIterationStep;
            set
            {
                if (SetProperty(ref _balancerIterationStep, value))
                {
                    _interfaceVariables.BalancerIterationStep = value;
                    OnPropertyChanged(nameof(BalancerIterationStep));
                }
            }
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

                    for (int i = 0; i < BalancerIterationStepChart.Count; i++)
                    {
                        if (i < BalancerIterationVibrationsChart.Count)
                        {
                            series.Values.Add(new ObservablePoint(BalancerIterationStepChart[i], BalancerIterationVibrationsChart[i]));
                        }
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



