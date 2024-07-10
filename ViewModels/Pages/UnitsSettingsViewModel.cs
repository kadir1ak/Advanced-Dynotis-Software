using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Advanced_Dynotis_Software.ViewModels.Pages
{
    public class UnitsSettingsViewModel : INotifyPropertyChanged
    {
        public InterfaceVariables InterfaceVariables => InterfaceVariables.Instance;

        public int SelectedTorqueUnitIndex
        {
            get => InterfaceVariables.SelectedTorqueUnitIndex;
            set
            {
                if (InterfaceVariables.SelectedTorqueUnitIndex != value)
                {
                    InterfaceVariables.SelectedTorqueUnitIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedThrustUnitIndex
        {
            get => InterfaceVariables.SelectedThrustUnitIndex;
            set
            {
                if (InterfaceVariables.SelectedThrustUnitIndex != value)
                {
                    InterfaceVariables.SelectedThrustUnitIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedMotorSpeedUnitIndex
        {
            get => InterfaceVariables.SelectedMotorSpeedUnitIndex;
            set
            {
                if (InterfaceVariables.SelectedMotorSpeedUnitIndex != value)
                {
                    InterfaceVariables.SelectedMotorSpeedUnitIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedTemperatureUnitIndex
        {
            get => InterfaceVariables.SelectedTemperatureUnitIndex;
            set
            {
                if (InterfaceVariables.SelectedTemperatureUnitIndex != value)
                {
                    InterfaceVariables.SelectedTemperatureUnitIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedWindSpeedUnitIndex
        {
            get => InterfaceVariables.SelectedWindSpeedUnitIndex;
            set
            {
                if (InterfaceVariables.SelectedWindSpeedUnitIndex != value)
                {
                    InterfaceVariables.SelectedWindSpeedUnitIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedPressureUnitIndex
        {
            get => InterfaceVariables.SelectedPressureUnitIndex;
            set
            {
                if (InterfaceVariables.SelectedPressureUnitIndex != value)
                {
                    InterfaceVariables.SelectedPressureUnitIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
