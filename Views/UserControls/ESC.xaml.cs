using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Advanced_Dynotis_Software.Services.Helpers;
using Advanced_Dynotis_Software.ViewModels.Pages;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    public partial class ESC : UserControl, INotifyPropertyChanged
    {
        private double _value;
        private bool _isLocked;
        private double _sliderActualWidth;
        private double _sliderWidth;
        private double _thumbPosition;

        public ESC()
        {
            InitializeComponent();
            DataContext = this;

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
                        Value = 0;
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
            IsLocked = !IsLocked;
        }

        private void IncreaseByFive()
        {
            if (!IsLocked)
            {
                Value = Math.Min(Value + 5, 100);
            }
        }

        private void Stop()
        {
            if (!IsLocked)
            {
                Value = 0;
            }
        }

        private void UpdateSliderAndThumb()
        {
            if (SliderActualWidth > 0)
            {
                SliderWidth = (Value / 100) * SliderActualWidth;
                ThumbPosition = (Value / 100) * SliderActualWidth;
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb && thumb.TemplatedParent is Slider slider)
            {
                if (!IsLocked)
                {
                    double newValue = slider.Value + e.HorizontalChange / slider.ActualWidth * (slider.Maximum - slider.Minimum);
                    newValue = Math.Round(newValue, 0);
                    slider.Value = Math.Max(slider.Minimum, Math.Min(slider.Maximum, newValue));
                }
            }
        }

        private void Slider_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                SliderActualWidth = slider.ActualWidth;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
