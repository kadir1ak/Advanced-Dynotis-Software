using System.Windows;
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
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb && thumb.TemplatedParent is Slider slider && DataContext is ESCParametersViewModel viewModel && viewModel.ESCStatus)
            {
                double newValue = slider.Value + (e.HorizontalChange / slider.ActualWidth) * (slider.Maximum - slider.Minimum);
                viewModel.ESCValue = (int)Math.Round(Math.Max(slider.Minimum, Math.Min(slider.Maximum, newValue)), 0);
            }
        }
    }
}
