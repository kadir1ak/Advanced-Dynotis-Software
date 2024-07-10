using Advanced_Dynotis_Software.ViewModels.Pages;
using System.Windows.Controls;

namespace Advanced_Dynotis_Software.Views.Pages
{
    public partial class UnitsSettingsPage : UserControl
    {
        public UnitsSettingsPage()
        {
            InitializeComponent();
            DataContext = new UnitsSettingsViewModel();
        }
    }
}
