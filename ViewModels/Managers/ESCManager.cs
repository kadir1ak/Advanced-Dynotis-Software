using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Input;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.Views.UserControls;

namespace Advanced_Dynotis_Software.ViewModels.Managers
{
    public class ESCManager : INotifyPropertyChanged
    {
        private double _value;
        private bool _isLocked;
        private double _sliderActualWidth;

        public ESCManager()
        {
            IsLocked = true;
            EscLockCommand = new RelayCommand(_ => LockESC());
            IncreaseByFiveCommand = new RelayCommand(_ => IncreaseByFive());
            StopCommand = new RelayCommand(_ => Stop());
        }

        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                    UpdateSliderAndThumb();
                }
            }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                if (_isLocked != value)
                {
                    _isLocked = value;
                    OnPropertyChanged(nameof(IsLocked));
                    if (_isLocked)
                    {
                        Value = 0; // ESC değerini sıfırla
                    }
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

        public static IValueConverter BooleanToVisibilityConverter { get; } = new BooleanToVisibilityConverterImpl();

        private class BooleanToVisibilityConverterImpl : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool boolValue)
                {
                    return boolValue ? Visibility.Visible : Visibility.Collapsed;
                }
                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is Visibility visibility)
                {
                    return visibility == Visibility.Visible;
                }
                return false;
            }
        }
    }
}
