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
        private int _escValue;
        private bool _escStatus;
        private DynotisData _dynotisData;

        public ESCParametersViewModel(DynotisData dynotisData)
        {
            _dynotisData = dynotisData;
            ESCValue = 800; 
            ESCStatus = false; 

            EscLockCommand = new RelayCommand(_ => LockESC());
            IncreaseStepByStepCommand = new RelayCommand(_ => IncreaseStepByStep(), _ => ESCStatus);
            StopCommand = new RelayCommand(_ => Stop(), _ => ESCStatus);
        }

        public int ESCValue
        {
            get => _escValue;
            set
            {
                if (SetProperty(ref _escValue, value))
                {
                    _dynotisData.ESCValue = value;
                }
            }
        }

        public bool ESCStatus
        {
            get => _escStatus;
            set
            {
                if (SetProperty(ref _escStatus, value))
                {
                    _dynotisData.ESCStatus = value;
                    if (_escStatus)
                    {
                        ESCValue = 800;
                    }
                    OnPropertyChanged(nameof(ESCStatus));
                }
            }
        }

        public ICommand EscLockCommand { get; }
        public ICommand IncreaseStepByStepCommand { get; }
        public ICommand StopCommand { get; }

        private void LockESC()
        {
            ESCStatus = !ESCStatus;
            if (!ESCStatus)
            {
                ESCValue = 800;
            }
        }

        private void IncreaseStepByStep()
        {
            if (ESCStatus)
            {
                ESCValue = Math.Min(ESCValue + 100, 2200);
            }
        }

        private void Stop()
        {
            if (ESCStatus)
            {
                ESCValue = 800;
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
