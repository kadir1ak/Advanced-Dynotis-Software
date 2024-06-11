using Advanced_Dynotis_Software.Views.UserControls;
using Advanced_Dynotis_Software.Views.Pages; 
using LiveCharts.Definitions.Charts;
using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Advanced_Dynotis_Software.Views.Windows
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }

        }
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuButton clickedButton)
            {
                // Tıklanan düğmenin önceki aktiflik durumunu kontrol et
                if (!clickedButton.IsActive)
                {
                    // Tüm menü düğmelerini pasifleştir
                    foreach (var menuButton in LeftMenuPanel.Children.OfType<MenuButton>())
                    {
                        menuButton.IsActive = false;
                    }

                    // Tıklanan düğmeyi aktifleştir
                    clickedButton.IsActive = true;
                }

                // İşlemlere devam et...
                switch (clickedButton.Icon)
                {
                    case PackIconMaterialKind.HomeVariant:
                        ContentArea.Content = new HomePage();
                        break;
                    case PackIconMaterialKind.ChartLine:
                        ContentArea.Content = new ChartPage();
                        break;
                    case PackIconMaterialKind.Autorenew:
                        ContentArea.Content = new AutomateTestPage();
                        break;
                    case PackIconMaterialKind.ContentSaveCog:
                        MessageBox.Show("ContentSaveCog button clicked!");
                        break;
                    case PackIconMaterialKind.BookSyncOutline:
                        MessageBox.Show("BookSyncOutline button clicked!");
                        break;
                    case PackIconMaterialKind.AlertCircleCheckOutline:
                        MessageBox.Show("AlertCircleCheckOutline button clicked!");
                        break;
                    case PackIconMaterialKind.CogOutline:
                        MessageBox.Show("CogOutline button clicked!");
                        break;
                    case PackIconMaterialKind.Update:
                        MessageBox.Show("Update button clicked!");
                        break;
                    case PackIconMaterialKind.Power:
                        Application.Current.Shutdown();
                        break;
                    default:
                        MessageBox.Show("Unknown button clicked!");
                        break;
                }
            }
        }

        private void MenuButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
 
        }

        private void MenuButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
 
        }


    }
}
