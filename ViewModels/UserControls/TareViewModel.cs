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
        private DynotisData _dynotisData;

        public TareViewModel(DynotisData dynotisData)
        {
            _dynotisData = dynotisData;
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
                    _dynotisData.TareTorqueValue = value;
                    OnPropertyChanged(nameof(_dynotisData));
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
                    _dynotisData.TareThrustValue = value;
                    OnPropertyChanged(nameof(_dynotisData));
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
                    _dynotisData.TareCurrentValue = value;
                    OnPropertyChanged(nameof(_dynotisData));
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
                    _dynotisData.TareMotorSpeedValue = value;
                    OnPropertyChanged(nameof(_dynotisData));
                }
            }
        }
 
        private void ExecuteTare()
        {
            MessageBox.Show("Tare İşlemleri Yapıldı");

            // Verileri senkronize et
            TareThrustValue = _dynotisData.Thrust.Value;
            TareTorqueValue = _dynotisData.Torque.Value;
            TareCurrentValue = _dynotisData.Current;
            TareMotorSpeedValue = _dynotisData.MotorSpeed.Value;

            // PropertyChanged olayını tetikleyerek UI'yi güncelle
            OnPropertyChanged(nameof(TareThrustValue));
            OnPropertyChanged(nameof(TareTorqueValue));
            OnPropertyChanged(nameof(TareCurrentValue));
            OnPropertyChanged(nameof(TareMotorSpeedValue));
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
