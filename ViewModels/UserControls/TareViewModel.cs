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
        private double _tareTorqueValue;
        private double _tareThrustValue;
        private double _tareCurrentValue;
        private double _tareMotorSpeedValue;
        private InterfaceVariables _interfaceVariables;

        public TareViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            TareCommand = new RelayCommand(param => ExecuteTare());
        }

        public ICommand TareCommand { get; }

        public double TareTorqueValue
        {
            get => _tareTorqueValue;
            set
            {
                if (SetProperty(ref _tareTorqueValue, value))
                {
                    _interfaceVariables.TareTorqueValue = value;
                    OnPropertyChanged(nameof(TareTorqueValue));
                }
            }
        }

        public double TareThrustValue
        {
            get => _tareThrustValue;
            set
            {
                if (SetProperty(ref _tareThrustValue, value))
                {
                    _interfaceVariables.TareThrustValue = value;
                    OnPropertyChanged(nameof(TareThrustValue));
                }
            }
        }

        public double TareCurrentValue
        {
            get => _tareCurrentValue;
            set
            {
                if (SetProperty(ref _tareCurrentValue, value))
                {
                    _interfaceVariables.TareCurrentValue = value;
                    OnPropertyChanged(nameof(TareCurrentValue));
                }
            }
        }

        public double TareMotorSpeedValue
        {
            get => _tareMotorSpeedValue;
            set
            {
                if (SetProperty(ref _tareMotorSpeedValue, value))
                {
                    _interfaceVariables.TareMotorSpeedValue = value;
                    OnPropertyChanged(nameof(TareMotorSpeedValue));
                }
            }
        }

        private void ExecuteTare()
        {
            if (_interfaceVariables != null)
            {
                MessageBox.Show("OK");
                TareTorqueValue = _interfaceVariables.Torque.Value;
                TareThrustValue = _interfaceVariables.Thrust.Value;
                TareCurrentValue = _interfaceVariables.Current;
                TareMotorSpeedValue = _interfaceVariables.MotorSpeed.Value;
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
