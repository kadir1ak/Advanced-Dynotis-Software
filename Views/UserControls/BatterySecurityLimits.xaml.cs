using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    public partial class BatterySecurityLimits : UserControl
    {
        public BatterySecurityLimits()
        {
            InitializeComponent();
        }
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(BatterySecurityLimits), new PropertyMetadata(false));
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // ToggleButton Checked olduğunda çalışacak kod
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // ToggleButton Unchecked olduğunda çalışacak kod
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,]+"); // Yalnızca sayı, nokta ve virgül izin ver
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
