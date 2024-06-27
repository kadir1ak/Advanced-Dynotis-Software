using System;
using System.ComponentModel;
using System.Windows.Input;
using Advanced_Dynotis_Software.Models.ESC;
using Advanced_Dynotis_Software.Services.Helpers;

namespace Advanced_Dynotis_Software.ViewModels.UserControls
{
    public class ESCManager : INotifyPropertyChanged
    {
        private ESC _esc;
        private double _sliderActualWidth;

        public ESCManager()
        {
            _esc = new ESC();
            EscLockCommand = new RelayCommand(_ => LockESC());
            IncreaseByFiveCommand = new RelayCommand(_ => IncreaseByFive());
            StopCommand = new RelayCommand(_ => Stop());
        }

        public double Value
        {
            get => _esc.Value;
            set
            {
                if (_esc.Value != value)
                {
                    _esc.Value = value;
                    OnPropertyChanged(nameof(Value));
                    UpdateSliderAndThumb();
                }
            }
        }

        public bool IsLocked
        {
            get => _esc.IsLocked;
            set
            {
                if (_esc.IsLocked != value)
                {
                    _esc.IsLocked = value;
                    OnPropertyChanged(nameof(IsLocked));
                }
            }
        }

        public double SliderActualWidth
        {
            get => _sliderActualWidth;
            set
            {
                if (_sliderActualWidth != value)
                {
                    _sliderActualWidth = value;
                    OnPropertyChanged(nameof(SliderActualWidth));
                    UpdateSliderAndThumb();
                }
            }
        }

        private double _sliderWidth;
        public double SliderWidth
        {
            get => _sliderWidth;
            private set
            {
                if (_sliderWidth != value)
                {
                    _sliderWidth = value;
                    OnPropertyChanged(nameof(SliderWidth));
                }
            }
        }

        private double _thumbPosition;
        public double ThumbPosition
        {
            get => _thumbPosition;
            private set
            {
                if (_thumbPosition != value)
                {
                    _thumbPosition = value;
                    OnPropertyChanged(nameof(ThumbPosition));
                }
            }
        }

        public ICommand EscLockCommand { get; }
        public ICommand IncreaseByFiveCommand { get; }
        public ICommand StopCommand { get; }

        private void LockESC()
        {
            IsLocked = !IsLocked; // ESC kilidini değiştir
        }

        private void IncreaseByFive()
        {
            if (!IsLocked)
            {
                Value = Math.Min(Value + 5, 100); // Değeri 5 artır
            }
        }

        private void Stop()
        {
            if (!IsLocked)
            {
                Value = 0; // Değeri sıfırla
            }
        }

        private void UpdateSliderAndThumb()
        {
            // Slider genişliği ve thumb pozisyonu hesaplamaları
            if (SliderActualWidth > 0)
            {
                SliderWidth = (Value / 100) * SliderActualWidth; // Ölçekleme faktörünü ayarlayın
                ThumbPosition = (Value / 100) * SliderActualWidth;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
