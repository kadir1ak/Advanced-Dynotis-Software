using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Advanced_Dynotis_Software.Themes;
using MahApps.Metro.IconPacks;
using Advanced_Dynotis_Software.Views.Pages;
using Advanced_Dynotis_Software.Views.UserControls;
using Advanced_Dynotis_Software.ViewModels.Pages;
using System.Windows.Media;

namespace Advanced_Dynotis_Software.Views.Windows
{
    public partial class MainWindow : Window
    {
        private SingleTestViewModel _singleTestViewModel;

        public MainWindow()
        {
            InitializeComponent();
            AppTheme.ThemeChanged += OnThemeChanged;
            AnimateButtonScale(HomeButton, 1.15);
            ContentArea.Content = new HomePage();

            _singleTestViewModel = new SingleTestViewModel();
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
                if (!clickedButton.IsActive)
                {
                    foreach (var menuButton in LeftMenuPanel.Children.OfType<MenuButton>())
                    {
                        menuButton.IsActive = false;
                        AnimateButtonScale(menuButton, 1.0);
                    }

                    clickedButton.IsActive = true;
                    AnimateButtonScale(clickedButton, 1.15);
                }

                switch (clickedButton.Icon)
                {
                    case PackIconMaterialKind.Home:
                        ContentArea.Content = new HomePage();
                        break;
                    case PackIconMaterialKind.UsbPort:
                        if (ContentArea.Content is SingleTestPage singleTestPage)
                        {
                            singleTestPage.DataContext = _singleTestViewModel;
                        }
                        else
                        {
                            var singleTestPageNew = new SingleTestPage { DataContext = _singleTestViewModel };
                            ContentArea.Content = singleTestPageNew;
                        }
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

        private void OnThemeChanged(object sender, EventArgs e)
        {
            foreach (var menuButton in LeftMenuPanel.Children.OfType<MenuButton>())
            {
                menuButton.UpdateTheme();
            }
        }

        private void ThemesButton_Checked(object sender, RoutedEventArgs e)
        {
            AppTheme.ChangeTheme(new Uri("Themes/Dark.xaml", UriKind.Relative));
        }

        private void ThemesButton_Unchecked(object sender, RoutedEventArgs e)
        {
            AppTheme.ChangeTheme(new Uri("Themes/Light.xaml", UriKind.Relative));
        }
    }
}
