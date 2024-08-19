using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.Services.Logger;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancerParametersViewModel : INotifyPropertyChanged
    {
        private int _referenceMotorSpeed;
        private double _referencePropellerDiameter;

        private double _balancerIterationStep;
        private ObservableCollection<double> _balancerIterationStepChart;
        private ObservableCollection<double> _balancerIterationVibrationsChart;
        private ObservableCollection<string> _balancerIterationDescription;
        private InterfaceVariables _interfaceVariables;
        private DynotisData _dynotisData;

        private ObservableCollection<BalancerIteration> _balancingIterations;
        public BalancerParametersViewModel(DynotisData dynotisData, InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            _dynotisData = dynotisData;
            // Subscribe to the PropertyChanged event of InterfaceVariables
            _interfaceVariables.PropertyChanged += InterfaceVariables_PropertyChanged;

            _balancerIterationStepChart = new ObservableCollection<double>();
            _balancerIterationVibrationsChart = new ObservableCollection<double>();
            _balancerIterationDescription = new ObservableCollection<string>();
            _balancingIterations = new ObservableCollection<BalancerIteration>();
        }

        private void InterfaceVariables_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_interfaceVariables.BalancerIterationStep) ||
                e.PropertyName == nameof(_interfaceVariables.BalancerIterationStepChart) ||
                e.PropertyName == nameof(_interfaceVariables.BalancerIterationDescription) ||
                e.PropertyName == nameof(_interfaceVariables.ReferencePropellerDiameter) ||
                e.PropertyName == nameof(_interfaceVariables.ReferenceMotorSpeed) ||
                e.PropertyName == nameof(_interfaceVariables.BalancerIterationVibrationsChart))
            {
                BalancerIterationStep = _interfaceVariables.BalancerIterationStep;
                BalancerIterationStepChart = _interfaceVariables.BalancerIterationStepChart;
                BalancerIterationVibrationsChart = _interfaceVariables.BalancerIterationVibrationsChart;
                BalancerIterationDescription = _interfaceVariables.BalancerIterationDescription;
                ReferencePropellerDiameter = _interfaceVariables.ReferencePropellerDiameter;
                ReferenceMotorSpeed = _interfaceVariables.ReferenceMotorSpeed;
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
        public double ReferencePropellerDiameter
        {
            get => _referencePropellerDiameter;
            set
            {
                if (SetProperty(ref _referencePropellerDiameter, value))
                {
                    _interfaceVariables.ReferencePropellerDiameter = value;
                    OnPropertyChanged(nameof(ReferencePropellerDiameter));
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
                    UpdateBalancingIterations();
                }
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
        public ObservableCollection<string> BalancerIterationDescription
        {
            get => _balancerIterationDescription;
            set
            {
                if (SetProperty(ref _balancerIterationDescription, value))
                {
                    OnPropertyChanged(nameof(BalancerIterationDescription));
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
            for (int i = 0; i < BalancerIterationStep; i++)
            {
              
                // Add each step and corresponding vibration to the BalancingIterations collection
                BalancingIterations.Add(new BalancerIteration
                {
                    IterationStep = BalancerIterationStepChart[i],
                    Vibrations = Math.Round(BalancerIterationVibrationsChart[i], 3),
                    Unit = "IPS",
                    Description = BalancerIterationDescription[i]
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
        public double IterationStep { get; set; }
        public double Vibrations { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
    }
}
