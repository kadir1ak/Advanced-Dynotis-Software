using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advanced_Dynotis_Software.Models.Dynotis;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class EquipmentParametersViewModel : INotifyPropertyChanged
    {
        private double _userPropellerArea;
        private double _userMotorInner;
        private double _userNoLoadCurrents;
        private DynotisData _dynotisData;

        public double UserPropellerArea
        {
            get => _userPropellerArea;
            set
            {
                if (SetProperty(ref _userPropellerArea, value))
                {
                    _dynotisData.PropellerArea = value;
                    OnPropertyChanged(nameof(_dynotisData));
                }
            }
        }

        public double UserMotorInner
        {
            get => _userMotorInner;
            set
            {
                if (SetProperty(ref _userMotorInner, value))
                {
                    _dynotisData.MotorInner = value;
                    OnPropertyChanged(nameof(_dynotisData));
                }
            }
        }

        public double UserNoLoadCurrents
        {
            get => _userNoLoadCurrents;
            set
            {
                if (SetProperty(ref _userNoLoadCurrents, value))
                {
                    _dynotisData.NoLoadCurrents = value;
                    OnPropertyChanged(nameof(_dynotisData));
                }
            }
        }

        public EquipmentParametersViewModel(DynotisData dynotisData)
        {
            _dynotisData = dynotisData;
            UserPropellerArea = dynotisData.PropellerArea;
            UserMotorInner = dynotisData.MotorInner;
            UserNoLoadCurrents = dynotisData.NoLoadCurrents;
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
