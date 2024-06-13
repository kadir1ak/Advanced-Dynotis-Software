using Advanced_Dynotis_Software.Views.UserControls;
using Advanced_Dynotis_Software.Views.Pages; 
using LiveCharts.Definitions.Charts;
using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using Advanced_Dynotis_Software.ViewModels.Windows;
using Advanced_Dynotis_Software.Themes;

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
            AnimateButtonScale(HomeButton, 1.15);
            ContentArea.Content = new HomePage();
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
                        AnimateButtonScale(menuButton, 1.0);
                    }

                    // Tıklanan düğmeyi aktifleştir
                    clickedButton.IsActive = true;
                    AnimateButtonScale(clickedButton, 1.15);
                }

                // İşlemlere devam et...
                switch (clickedButton.Icon)
                {
                    case PackIconMaterialKind.Home:
                        ContentArea.Content = new HomePage();
                        break;
                    case PackIconMaterialKind.UsbPort:
                        ContentArea.Content = new SingleTestPage();
                        break;
                    case PackIconMaterialKind.Usb:
                        ContentArea.Content = new CoaxialTestPage();
                        break;
                    case PackIconMaterialKind.Multicast:
                        ContentArea.Content = new MultiTestPage();
                        break;
                    case PackIconMaterialKind.Network:
                        ContentArea.Content = new APIPage();
                        break;
                    case PackIconMaterialKind.Script:
                        ContentArea.Content = new ScriptPage();
                        break;
                    case PackIconMaterialKind.Autorenew:
                        ContentArea.Content = new AutomateTestPage();
                        break;
                    case PackIconMaterialKind.Tools:
                        ContentArea.Content = new SettingsPage();
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
        private void AnimateButtonScale(MenuButton button, double scale)
        {
            var scaleTransform = new ScaleTransform(1.0, 1.0);
            button.RenderTransform = scaleTransform;

            var scaleXAnimation = new DoubleAnimation(scale, TimeSpan.FromSeconds(0.5));
            var scaleYAnimation = new DoubleAnimation(scale, TimeSpan.FromSeconds(0.5));

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
        }

        private void MenuButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
 
        }

        private void MenuButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
 
        }

        private void A_Click(object sender, MouseButtonEventArgs e)
        {
            AppTheme.ChangeTheme(new Uri("Themes/Light.xaml", UriKind.Relative));
        }

        private void B_Click(object sender, MouseButtonEventArgs e)
        {
            AppTheme.ChangeTheme(new Uri("Themes/Dark.xaml", UriKind.Relative));
        }
    }
}
