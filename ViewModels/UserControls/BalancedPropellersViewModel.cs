using Advanced_Dynotis_Software.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private double _balancedPropellerArea;

        private ObservableCollection<DateTime> _balancingTestDate;
        private ObservableCollection<double> _vibrationLevel;

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
        public double BalancedPropellerArea
        {
            get => _balancedPropellerArea;
            set
            {
                if (SetProperty(ref _balancedPropellerArea, value))
                {
                    _interfaceVariables.BalancedPropeller.BalancedPropellerArea = value;
                    OnPropertyChanged(nameof(BalancedPropellerArea));
                }
            }
        }
        public ObservableCollection<DateTime> BalancingTestDate
        {
            get => _balancingTestDate;
            set
            {
                if (SetProperty(ref _balancingTestDate, value))
                {
                    _interfaceVariables.BalancedPropeller.BalancingTestDate = value;
                    OnPropertyChanged(nameof(BalancingTestDate));
                }
            }
        }
        public ObservableCollection<double> VibrationLevel
        {
            get => _vibrationLevel;
            set
            {
                if (SetProperty(ref _vibrationLevel, value))
                {
                    _interfaceVariables.BalancedPropeller.VibrationLevel = value;
                    OnPropertyChanged(nameof(VibrationLevel));
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
