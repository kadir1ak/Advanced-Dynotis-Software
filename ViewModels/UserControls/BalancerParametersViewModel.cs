using Advanced_Dynotis_Software.Models.Dynotis;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancerParametersViewModel : INotifyPropertyChanged
    {
        private int _referenceMotorSpeed;
        private double _referenceWeight;
        private int _balancerIterationStep;
        private ObservableCollection<int> _balancerIterationStepChart;
        private ObservableCollection<double> _balancerIterationVibrationsChart;
        private InterfaceVariables _interfaceVariables;

        private ObservableCollection<BalancerIteration> _balancingIterations;

        public BalancerParametersViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            // Subscribe to the PropertyChanged event of InterfaceVariables
            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

            _referenceWeight = 0.050; // g
            _balancerIterationStepChart = new ObservableCollection<int>();
            _balancerIterationVibrationsChart = new ObservableCollection<double>();
            _balancingIterations = new ObservableCollection<BalancerIteration>();
        }

        private void InterfaceVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_interfaceVariables.BalancerIterationStep) ||
                e.PropertyName == nameof(_interfaceVariables.BalancerIterationStepChart) ||
                e.PropertyName == nameof(_interfaceVariables.BalancerIterationVibrationsChart))
            {
                BalancerIterationStep = _interfaceVariables.BalancerIterationStep;
                BalancerIterationStepChart = _interfaceVariables.BalancerIterationStepChart;
                BalancerIterationVibrationsChart = _interfaceVariables.BalancerIterationVibrationsChart;
                UpdateBalancingIterations();
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

        public ObservableCollection<int> BalancerIterationStepChart
        {
            get => _balancerIterationStepChart;
            set
            {
                if (SetProperty(ref _balancerIterationStepChart, value))
                {
                    _interfaceVariables.BalancerIterationStepChart = value;
                    OnPropertyChanged(nameof(BalancerIterationStepChart));
                    UpdateBalancingIterations();
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
                    UpdateBalancingIterations();
                }
            }
        }
        public ObservableCollection<BalancerIteration> BalancingIterations
        {
            get => _balancingIterations;
            set
            {
                if (SetProperty(ref _balancingIterations, value))
                {
                    OnPropertyChanged(nameof(BalancingIterations));
                }
            }
        }

        private void UpdateBalancingIterations()
        {
            // Clear the BalancingIterations collection to prevent data stacking
            BalancingIterations.Clear();

            // Iterate through the BalancerIterationStepChart and BalancerIterationVibrationsChart collections
            for (int i = 0; i < BalancerIterationStepChart.Count && i < BalancerIterationVibrationsChart.Count; i++)
            {
                // Add each step and corresponding vibration to the BalancingIterations collection
                BalancingIterations.Add(new BalancerIteration
                {
                    IterationStep = BalancerIterationStepChart[i],
                    Vibrations = Math.Round(BalancerIterationVibrationsChart[i], 3)
                });
            }

            // Notify that the BalancingIterations collection has been updated
            OnPropertyChanged(nameof(BalancingIterations));
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
