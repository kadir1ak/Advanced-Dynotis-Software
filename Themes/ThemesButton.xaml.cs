using System.Windows;
using System.Windows.Controls;

namespace Advanced_Dynotis_Software.Themes
{
    public partial class ThemesButton : UserControl
    {
        public ThemesButton()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler Checked;
        public event RoutedEventHandler Unchecked;

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Checked?.Invoke(this, e);
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Unchecked?.Invoke(this, e);
        }
    }
}
