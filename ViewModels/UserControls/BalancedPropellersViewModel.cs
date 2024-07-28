using Advanced_Dynotis_Software.Services.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BalancedPropellersViewModel : INotifyPropertyChanged
    {

        private string _balancedPropellerID;
        private double _referencePropellerArea;

        private List<DateTime> _balancingDate;
        private List<int> _referenceMotorSpeed;
        private List<double> _referenceWeight;
  
        private List<double> _balancerWeight;
        private List<string> _balancerPosition;

        private List<double> _lowestVibrationLevel;
        private List<double> _maximumVibrationLevel;

        private InterfaceVariables _interfaceVariables;

        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand DeleteCommand { get; }


        public BalancedPropellersViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
        }
        public string BalancedPropellerID
        {
            get => _balancedPropellerID;
            set
            {
                if (SetProperty(ref _balancedPropellerID, value))
                {
                    _interfaceVariables.BalancedPropeller.BalancedPropellerID = value;
                    OnPropertyChanged(nameof(BalancedPropellerID));
                }
            }
        }
        public double ReferencePropellerArea
        {
            get => _referencePropellerArea;
            set
            {
                if (SetProperty(ref _referencePropellerArea, value))
                {
                    _interfaceVariables.BalancedPropeller.ReferencePropellerArea = value;
                    OnPropertyChanged(nameof(ReferencePropellerArea));
                }
            }
        }     

        public List<int> ReferenceMotorSpeed
        {
            get => _referenceMotorSpeed;
            set
            {
                if (SetProperty(ref _referenceMotorSpeed, value))
                {
                    _interfaceVariables.BalancedPropeller.ReferenceMotorSpeed = value;
                    OnPropertyChanged(nameof(ReferenceMotorSpeed));
                }
            }
        }
        public List<double> ReferenceWeight
        {
            get => _referenceWeight;
            set
            {
                if (SetProperty(ref _referenceWeight, value))
                {
                    _interfaceVariables.BalancedPropeller.ReferenceWeight = value;
                    OnPropertyChanged(nameof(ReferenceWeight));
                }
            }
        }
        public List<double> BalancerWeight
        {
            get => _balancerWeight;
            set
            {
                if (SetProperty(ref _balancerWeight, value))
                {
                    _interfaceVariables.BalancedPropeller.BalancerWeight = value;
                    OnPropertyChanged(nameof(BalancerWeight));
                }
            }
        }
        public List<string> BalancerPosition
        {
            get => _balancerPosition;
            set
            {
                if (SetProperty(ref _balancerPosition, value))
                {
                    _interfaceVariables.BalancedPropeller.BalancerPosition = value;
                    OnPropertyChanged(nameof(BalancerPosition));
                }
            }
        }
        public List<double> LowestVibrationLevel
        {
            get => _lowestVibrationLevel;
            set
            {
                if (SetProperty(ref _lowestVibrationLevel, value))
                {
                    _interfaceVariables.BalancedPropeller.LowestVibrationLevel = value;
                    OnPropertyChanged(nameof(LowestVibrationLevel));
                }
            }
        }
        public List<double> MaximumVibrationLevel
        {
            get => _maximumVibrationLevel;
            set
            {
                if (SetProperty(ref _maximumVibrationLevel, value))
                {
                    _interfaceVariables.BalancedPropeller.MaximumVibrationLevel = value;
                    OnPropertyChanged(nameof(MaximumVibrationLevel));
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
