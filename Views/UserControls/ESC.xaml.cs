using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Advanced_Dynotis_Software.ViewModels.Managers;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    public partial class ESC : UserControl
    {
        private ESCManager escManager;

        public ESC()
        {
            InitializeComponent();
            escManager = (ESCManager)DataContext;
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb != null)
            {
                var slider = thumb.TemplatedParent as Slider;
                if (slider != null && !escManager.IsLocked)
                {
                    double newValue = slider.Value + e.HorizontalChange / slider.ActualWidth * (slider.Maximum - slider.Minimum);
                    newValue = Math.Round(newValue, 0); // Değeri iki ondalık basamağa yuvarla
                    slider.Value = Math.Max(slider.Minimum, Math.Min(slider.Maximum, newValue));
                }
            }
        }

        private void Slider_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var slider = sender as Slider;
            if (slider != null)
            {
                escManager.SliderActualWidth = slider.ActualWidth;
            }
        }
    }
}
