using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Advanced_Dynotis_Software.ViewModels.UserControls;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    public partial class ESC : UserControl
    {
        public ESC()
        {
            InitializeComponent();
            DataContext = new ESCManager();
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb != null)
            {
                var slider = thumb.TemplatedParent as Slider;
                if (slider != null)
                {
                    double newValue = slider.Value + e.HorizontalChange / slider.ActualWidth * (slider.Maximum - slider.Minimum);
                    newValue = Math.Round(newValue, 0); // Değeri iki ondalık basamağa yuvarla
                    slider.Value = Math.Max(slider.Minimum, Math.Min(slider.Maximum, newValue));
                }
            }
        }
    }
}
