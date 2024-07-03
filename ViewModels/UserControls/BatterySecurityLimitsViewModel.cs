using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advanced_Dynotis_Software.Models.Dynotis;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class BatterySecurityLimitsViewModel : INotifyPropertyChanged
    {
        private double _maxCurrent;
        private double _batteryLevel;
        private string _securityStatus;
        private DynotisData _dynotisData;

        public double MaxCurrent
        {
            get => _maxCurrent;
            set
            {
                if (SetProperty(ref _maxCurrent, value))
                {
                    _dynotisData.MaxCurrent = value;
                    OnPropertyChanged(nameof(_dynotisData));
                }
            }
        }

        public double BatteryLevel
        {
            get => _batteryLevel;
            set
            {
                if (SetProperty(ref _batteryLevel, value))
                {
                    _dynotisData.BatteryLevel = value;
                    OnPropertyChanged(nameof(_dynotisData));
                }
            }
        }

        public string SecurityStatus
        {
            get => _securityStatus;
            set
            {
                if (SetProperty(ref _securityStatus, value))
                {
                    _dynotisData.SecurityStatus = value;
                    OnPropertyChanged(nameof(_dynotisData));
                }
            }
        }

        public BatterySecurityLimitsViewModel(DynotisData dynotisData)
        {
            _dynotisData = dynotisData;
            MaxCurrent = dynotisData.MaxCurrent;
            BatteryLevel = dynotisData.BatteryLevel;
            SecurityStatus = dynotisData.SecurityStatus;
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
