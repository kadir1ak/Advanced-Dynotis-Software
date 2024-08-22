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
    /// Interaction logic for EquipmentParameters.xaml
    /// </summary>
    public partial class EquipmentParameters : UserControl
    {
        public EquipmentParameters()
        {
            InitializeComponent();
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Sadece sayı, nokta ve virgüle izin ver
            Regex regex = new Regex("[^0-9.,-]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        private void NumericTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string inputText = textBox.Text;

                // Ondalık ayırıcıları göz önünde bulundurarak sayıyı işleyelim
                if (double.TryParse(inputText.Replace(',', '.'), out double result))
                {
                    // Ondalık değeri doğru bir şekilde kaydet
                    textBox.Text = result.ToString("F"); // "F" formatı ondalık olarak saklar
                }
                else
                {
                    // Geçersiz giriş varsa kullanıcıyı bilgilendir
                    textBox.Text = "0"; // Girdiyi temizle
                }
            }
        }


    }
}
