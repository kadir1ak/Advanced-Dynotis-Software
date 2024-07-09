using Advanced_Dynotis_Software.Models.Dynotis;
using Advanced_Dynotis_Software.Services.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class UnitsSettingsViewModel : INotifyPropertyChanged
    {
        InterfaceVariables _interfaceVariables;

        private string _torqueUnit;
        private string _thrustUnit;
        private string _motorSpeedUnit;
        private string _temperatureUnit;
        private string _windSpeedUnit;
        private string _pressureUnit;

        public string TorqueUnit
        {
            get => _torqueUnit;
            set
            {
                if (SetProperty(ref _torqueUnit, value))
                {
                    _interfaceVariables.SelectedTorqueUnit = value;
                    OnPropertyChanged(nameof(_interfaceVariables));
                }
            }
        }
        public string ThrustUnit
        {
            get => _thrustUnit;
            set
            {
                if (SetProperty(ref _thrustUnit, value))
                {
                    _interfaceVariables.SelectedThrustUnit = value;
                    OnPropertyChanged(nameof(_interfaceVariables));
                }
            }
        }
        public string MotorSpeedUnit
        {
            get => _motorSpeedUnit;
            set
            {
                if (SetProperty(ref _motorSpeedUnit, value))
                {
                    _interfaceVariables.SelectedMotorSpeedUnit = value;
                    OnPropertyChanged(nameof(_interfaceVariables));
                }
            }
        }
        public string TemperatureUnit
        {
            get => _temperatureUnit;
            set
            {
                if (SetProperty(ref _temperatureUnit, value))
                {
                    _interfaceVariables.SelectedMotorTempUnit = value;
                    _interfaceVariables.SelectedAmbientTempUnit = value;
                    OnPropertyChanged(nameof(_interfaceVariables));
                }
            }
        }
        public string WindSpeedUnit
        {
            get => _windSpeedUnit;
            set
            {
                if (SetProperty(ref _windSpeedUnit, value))
                {
                    _interfaceVariables.SelectedWindSpeedUnit = value;
                    OnPropertyChanged(nameof(_interfaceVariables));
                }
            }
        }
        public string PressureUnit
        {
            get => _pressureUnit;
            set
            {
                if (SetProperty(ref _pressureUnit, value))
                {
                    _interfaceVariables.SelectedPressureUnit = value;
                    OnPropertyChanged(nameof(_interfaceVariables));
                }
            }
        }
        public UnitsSettingsViewModel(InterfaceVariables interfaceVariables)
        {
            _interfaceVariables = interfaceVariables;
            TorqueUnit = interfaceVariables.SelectedTorqueUnit;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}