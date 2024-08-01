using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.ViewModels.UserControls;
using LiveCharts;
using LiveCharts.Wpf;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{

    public class BalancerRoutingStepsVibrationLevelsViewModel : INotifyPropertyChanged
    {
        private InterfaceVariables _interfaceVariables;
        private DynotisData _dynotisData;

        public BalancerRoutingStepsVibrationLevelsViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;

            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

            BalancerIterationVibrations = new List<double>();
            BalancerIterationStep = 0;

            VibrationSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Vibration Levels",
                    Values = new ChartValues<double>(BalancerIterationVibrations)
                }
            };

            XFormatter = value => value.ToString("0.0");
            YFormatter = value => value.ToString("0.000");
        }

        private void InterfaceVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InterfaceVariables.BalancerIterationVibrations))
            {
                BalancerIterationVibrations = _interfaceVariables.BalancerIterationVibrations;
                UpdateVibrationChart();
            }
            else if (e.PropertyName == nameof(InterfaceVariables.BalancerIterationStep))
            {
                BalancerIterationStep = _interfaceVariables.BalancerIterationStep;
                UpdateVibrationChart();
            }
        }

        private List<double> _balancerIterationVibrations;
        public List<double> BalancerIterationVibrations
        {
            get => _balancerIterationVibrations;
            set
            {
                if (SetProperty(ref _balancerIterationVibrations, value))
                {
                    UpdateVibrationChart();
                }
            }
        }

        private int _balancerIterationStep;
        public int BalancerIterationStep
        {
            get => _balancerIterationStep;
            set => SetProperty(ref _balancerIterationStep, value);
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
                    series.Values = new ChartValues<double>(BalancerIterationVibrations);
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



