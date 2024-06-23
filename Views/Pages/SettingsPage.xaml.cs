using Advanced_Dynotis_Software.Views.UserControls;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Advanced_Dynotis_Software.Views.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is SettingButton clickedButton)
            {
                // Tıklanan düğmenin önceki aktiflik durumunu kontrol et
                if (!clickedButton.IsActive)
                {
                    // Tüm menü düğmelerini pasifleştir
                    foreach (var settingButton in MenuButtonPanel.Children.OfType<SettingButton>())
                    {
                        settingButton.IsActive = false;
                    }

                    // Tıklanan düğmeyi aktifleştir
                    clickedButton.IsActive = true;
                }

                // İşlemlere devam et...
                switch (clickedButton.Icon)
                {
                    case PackIconMaterialKind.ContentSaveCog:
                        ContentArea.Content = new SaveConfigPage();
                        break;
                    case PackIconMaterialKind.Layers:
                        ContentArea.Content = new UnitsSettingsPage();
                        break;
                    case PackIconMaterialKind.AlertCircleCheckOutline:
                        ContentArea.Content = new AlertsPage();
                        break;
                    case PackIconMaterialKind.Update:
                        ContentArea.Content = new SoftwareUpdatePage();
                        break;
                    case PackIconMaterialKind.Translate:
                        ContentArea.Content = new LanguagePage();
                        break;
                    case PackIconMaterialKind.Information:
                        ContentArea.Content = new AboutPage();
                        break;
                    case PackIconMaterialKind.Help:
                        ContentArea.Content = new HelpPage();
                        break;
                    default:
                        MessageBox.Show("Unknown button clicked!");
                        break;
                }
            }
        }
    }
}
