using Advanced_Dynotis_Software.ViewModels.Pages;
using System.Windows.Controls;

namespace Advanced_Dynotis_Software.Views.Pages
{
    public partial class LanguagePage : UserControl
    {
        public LanguagePage()
        {
            InitializeComponent();
            DataContext = new LanguageViewModel();
        }
    }
}
