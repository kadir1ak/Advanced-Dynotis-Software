using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Advanced_Dynotis_Software.ViewModels.UserControls;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    /// <summary>
    /// ESC.xaml etkileşim mantığı
    /// </summary>
    public partial class ESC : UserControl
    {
        public ESC()
        {
            InitializeComponent();
            DataContext = new ESCManager();
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            // Slider değerini güncelleyin
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
