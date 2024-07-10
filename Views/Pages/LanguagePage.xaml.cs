using System.Windows.Controls;

namespace Advanced_Dynotis_Software.Views.Pages
{
    public partial class LanguagePage : UserControl
    {
        public LanguagePage()
        {
            InitializeComponent();
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender == TurkishCheckBox)
            {
                EnglishCheckBox.IsChecked = false;
            }
            else if (sender == EnglishCheckBox)
            {
                TurkishCheckBox.IsChecked = false;
            }
        }

        private void CheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            // Boş bırakıldı, ileride gerekirse ek işlevler için kullanılabilir.
        }
    }
}
