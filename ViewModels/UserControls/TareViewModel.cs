using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class TareViewModel : INotifyPropertyChanged
    {
        private double _tareTorqueBaseValue;
        private double _tareThrustBaseValue;
        private double _tareCurrentBaseValue;
        private double _tareMotorSpeedBaseValue;
        private InterfaceVariables _interfaceVariables;

        public TareViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            TareCommand = new RelayCommand(param => ExecuteTare());
        }

        public ICommand TareCommand { get; }

        public double TareTorqueBaseValue
        {
            get => _tareTorqueBaseValue;
            set
            {
                if (SetProperty(ref _tareTorqueBaseValue, value))
                {
                    _interfaceVariables.TareTorqueBaseValue = value;
                    OnPropertyChanged(nameof(TareTorqueBaseValue));
                }
            }
        }

        public double TareThrustBaseValue
        {
            get => _tareThrustBaseValue;
            set
            {
                if (SetProperty(ref _tareThrustBaseValue, value))
                {
                    _interfaceVariables.TareThrustBaseValue = value;
                    OnPropertyChanged(nameof(TareThrustBaseValue));
                }
            }
        }

        public double TareCurrentBaseValue
        {
            get => _tareCurrentBaseValue;
            set
            {
                if (SetProperty(ref _tareCurrentBaseValue, value))
                {
                    _interfaceVariables.TareCurrentBaseValue = value;
                    OnPropertyChanged(nameof(TareCurrentBaseValue));
                }
            }
        }

        public double TareMotorSpeedBaseValue
        {
            get => _tareMotorSpeedBaseValue;
            set
            {
                if (SetProperty(ref _tareMotorSpeedBaseValue, value))
                {
                    _interfaceVariables.TareMotorSpeedBaseValue = value;
                    OnPropertyChanged(nameof(TareMotorSpeedBaseValue));
                }
            }
        }

        private void ExecuteTare()
        {
            if (_interfaceVariables != null)
            {

                TareTorqueBaseValue = _interfaceVariables.Torque.BaseValue;
                TareThrustBaseValue = _interfaceVariables.Thrust.BaseValue;
                TareCurrentBaseValue = _interfaceVariables.Current;
                TareMotorSpeedBaseValue = _interfaceVariables.MotorSpeed.BaseValue;
                // Tare değerlerini InterfaceVariables.Instance'da saklayın
                InterfaceVariables.Instance.TareTorqueBaseValue = TareTorqueBaseValue;
                InterfaceVariables.Instance.TareThrustBaseValue = TareThrustBaseValue;
                InterfaceVariables.Instance.TareCurrentBaseValue = TareCurrentBaseValue;
                InterfaceVariables.Instance.TareMotorSpeedBaseValue = TareMotorSpeedBaseValue;
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
