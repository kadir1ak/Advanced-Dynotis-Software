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
        public double _tareTorqueValue;
        public double _tareThrustValue;
        public double _tareCurrentValue;
        public double _tareMotorSpeedValue;
        private DynotisData _dynotisData;
        public ICommand TareCommand { get; }

        public TareViewModel(DynotisData dynotisData)
        {
            _dynotisData = dynotisData;
            TareCommand = new RelayCommand(_ => Tare());
        }

        private void Tare()
        {
            MessageBox.Show("Tare İşlemleri Yapıldı");
            tareThrustValue = _dynotisData.Thrust.Value;
            tareTorqueValue = _dynotisData.Torque.Value;
            tareCurrentValue = _dynotisData.Current;
            tareMotorSpeedValue = _dynotisData.MotorSpeed.Value;

        }

        public double tareThrustValue
        {
            get => _tareThrustValue;
            set
            {
                if (SetProperty(ref _tareThrustValue, value))
                {
                    _dynotisData.TareThrustValue = value;
                }
                OnPropertyChanged(nameof(tareThrustValue));
            }
        }
        public double tareTorqueValue
        {
            get => _tareTorqueValue;
            set
            {
                if (SetProperty(ref _tareTorqueValue, value))
                {
                    _dynotisData.TareTorqueValue = value;
                }
                OnPropertyChanged(nameof(tareTorqueValue));
            }
        }
        public double tareCurrentValue
        {
            get => _tareCurrentValue;
            set
            {
                if (SetProperty(ref _tareCurrentValue, value))
                {
                    _dynotisData.TareCurrentValue = value;
                }
                OnPropertyChanged(nameof(tareCurrentValue));
            }
        }
        public double tareMotorSpeedValue
        {
            get => _tareMotorSpeedValue;
            set
            {
                if (SetProperty(ref _tareMotorSpeedValue, value))
                {
                    _dynotisData.TareMotorSpeedValue = value;
                }
                OnPropertyChanged(nameof(tareMotorSpeedValue));
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