using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Advanced_Dynotis_Software.Views.UserControls
{
    /// <summary>
    /// BalancerParameters.xaml etkileşim mantığı
    /// </summary>
    public partial class BalancerParameters : UserControl
    {
        public BalancerParameters()
        {
            InitializeComponent();
        }
        private void NumericTextBoxDouble_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,]+"); // Allow only numbers, dots, and commas
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NumericTextBoxInt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); // Allow only numbers
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
