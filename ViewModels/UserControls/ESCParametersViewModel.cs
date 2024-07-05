using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.Models.Dynotis;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class ESCParametersViewModel : INotifyPropertyChanged
    {
        private double _escValue;
        private bool _escStatus;
        private DynotisData _dynotisData;

        public ESCParametersViewModel(DynotisData dynotisData)
        {
            _dynotisData = dynotisData;
            ESCValue = dynotisData.ESCValue;
            ESCStatus = dynotisData.ESCStatus == true;

            EscLockCommand = new RelayCommand(_ => LockESC());
            IncreaseByFiveCommand = new RelayCommand(_ => IncreaseByFive());
            StopCommand = new RelayCommand(_ => Stop());
        }

        public double ESCValue
        {
            get => _escValue;
            set
            {
                if (SetProperty(ref _escValue, value))
                {
                    _dynotisData.ESCValue = value;
                }
                OnPropertyChanged(nameof(ESCValue));
            }
        }

        public bool ESCStatus
        {
            get => _escStatus;
            set
            {
                if (SetProperty(ref _escStatus, value))
                {
                    _dynotisData.ESCStatus = value ? true : false;
                    if (_escStatus)
                    {
                        ESCValue = 0;
                    }
                    OnPropertyChanged(nameof(ESCStatus));
                }
            }
        }

        public ICommand EscLockCommand { get; }
        public ICommand IncreaseByFiveCommand { get; }
        public ICommand StopCommand { get; }

        private void LockESC()
        {
            ESCStatus = !ESCStatus;
        }

        private void IncreaseByFive()
        {
            if (!ESCStatus)
            {
                ESCValue = Math.Min(ESCValue + 5, 100);
            }
        }

        private void Stop()
        {
            if (!ESCStatus)
            {
                ESCValue = 0;
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
