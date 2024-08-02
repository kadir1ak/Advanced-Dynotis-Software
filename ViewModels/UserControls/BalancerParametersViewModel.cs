using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancerParametersViewModel : INotifyPropertyChanged
    {
        private int _referenceMotorSpeed;
        private double _referenceWeight;
        private ObservableCollection<int> _balancerIterationStepChart;
        private ObservableCollection<double> _balancerIterationVibrationsChart;
        private InterfaceVariables _interfaceVariables;

        private List<BalancerIteration> _balancingIterations;

        public BalancerParametersViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            // Subscribe to the PropertyChanged event of InterfaceVariables
            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

            _referenceWeight = 0.05;
            _balancerIterationStepChart = new ObservableCollection<int>();
            _balancerIterationVibrationsChart = new ObservableCollection<double>();
            _balancingIterations = new List<BalancerIteration>();
        }

        private void InterfaceVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_interfaceVariables.BalancerIterationStepChart) ||
                e.PropertyName == nameof(_interfaceVariables.BalancerIterationVibrationsChart))
            {
                BalancerIterationStepChart = _interfaceVariables.BalancerIterationStepChart;
                BalancerIterationVibrationsChart = _interfaceVariables.BalancerIterationVibrationsChart;
                UpdateChartBalancingIterations();
            }
        }

        public int ReferenceMotorSpeed
        {
            get => _referenceMotorSpeed;
            set
            {
                if (SetProperty(ref _referenceMotorSpeed, value))
                {
                    _interfaceVariables.ReferenceMotorSpeed = value;
                    OnPropertyChanged(nameof(ReferenceMotorSpeed));
                }
            }
        }

        public double ReferenceWeight
        {
            get => _referenceWeight;
            set
            {
                if (SetProperty(ref _referenceWeight, value))
                {
                    _interfaceVariables.ReferenceWeight = value;
                    OnPropertyChanged(nameof(ReferenceWeight));
                }
            }
        }

        public ObservableCollection<int> BalancerIterationStepChart
        {
            get => _balancerIterationStepChart;
            set
            {
                if (SetProperty(ref _balancerIterationStepChart, value))
                {
                    _interfaceVariables.BalancerIterationStepChart = value;
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
                    _interfaceVariables.BalancerIterationVibrationsChart = value;
                    OnPropertyChanged(nameof(BalancerIterationVibrationsChart));
                }
            }
        }

        public List<BalancerIteration> BalancingIterations
        {
            get => _balancingIterations;
            set => SetProperty(ref _balancingIterations, value);
        }

        private void UpdateChartBalancingIterations()
        {
            BalancingIterations = BalancerIterationStepChart
                .Zip(BalancerIterationVibrationsChart, (step, vibration) => new BalancerIteration
                {
                    IterationStep = step,
                    Vibrations = vibration
                })
                .ToList();
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

    public class BalancerIteration
    {
        public int IterationStep { get; set; }
        public double Vibrations { get; set; }
    }
}
