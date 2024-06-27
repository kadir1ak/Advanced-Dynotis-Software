using System.ComponentModel;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class ESCManager : INotifyPropertyChanged
    {
        private double _speed;

        public double Speed
        {
            get => _speed;
            set
            {
                if (_speed != value)
                {
                    _speed = value;
                    OnPropertyChanged(nameof(Speed));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
