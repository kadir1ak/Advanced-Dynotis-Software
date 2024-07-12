using Advanced_Dynotis_Software.Services.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class TareViewModel : INotifyPropertyChanged
    {
        public static InterfaceVariables InterfaceVariables => InterfaceVariables.Instance;
        public ICommand TareCommand { get; }

        public TareViewModel()
        {
            TareCommand = new RelayCommand(_ => Tare());
        }

        private void Tare()
        {
            InterfaceVariables.TareTorqueValue = InterfaceVariables.Torque.Value;
            InterfaceVariables.TareThrustValue = InterfaceVariables.Thrust.Value;
            InterfaceVariables.TareCurrentValue = InterfaceVariables.Current;
            InterfaceVariables.TareMotorSpeedValue = InterfaceVariables.MotorSpeed.Value;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }


}