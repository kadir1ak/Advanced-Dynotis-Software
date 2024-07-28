using Advanced_Dynotis_Software.Models.Dynotis;
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
    public class BalancerParametersViewModel : INotifyPropertyChanged
    {

        private int _referenceMotorSpeed;
        private double _referenceWeight;
        private InterfaceVariables _interfaceVariables;

        public BalancerParametersViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            SetCommand = new RelayCommand(param => ExecuteTare());
        }

        public ICommand SetCommand { get; }

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

        private void ExecuteTare()
        {
            if (_interfaceVariables != null)
            {
                ReferenceMotorSpeed = _interfaceVariables.ReferenceMotorSpeed;
                ReferenceWeight = _interfaceVariables.ReferenceWeight;
                // Balance Parametre Değerlerini InterfaceVariables.Instance'da saklayın
                InterfaceVariables.Instance.ReferenceMotorSpeed = ReferenceMotorSpeed;
                InterfaceVariables.Instance.ReferenceWeight = ReferenceWeight;
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
