using System.Text.RegularExpressions;
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

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,]+"); // Yalnızca sayı, nokta ve virgül izin ver
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
