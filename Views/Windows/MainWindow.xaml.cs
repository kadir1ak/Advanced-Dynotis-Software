using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Advanced_Dynotis_Software.Themes;
using MahApps.Metro.IconPacks;
using Advanced_Dynotis_Software.Views.Pages;
using Advanced_Dynotis_Software.Views.UserControls;
using Advanced_Dynotis_Software.ViewModels.Pages;

namespace Advanced_Dynotis_Software.Views.Windows
{
    public partial class MainWindow : Window
    {
        private HomeViewModel _homeViewModel;
        private SingleTestViewModel _singleTestViewModel;
        private CoaxialTestViewModel _coaxialTestViewModel;
        private MultiTestViewModel _multiTestViewModel;
        private APIViewModel _apiViewModel;
        private ScriptViewModel _scriptViewModel;
        private AutomateTestViewModel _automateTestViewModel;
        private SettingsViewModel _settingsViewModel;
        private BalancerViewModel _balancerViewModel;

        public MainWindow()
        {
            InitializeComponent();
            AppTheme.ThemeChanged += OnThemeChanged;

            InitializeViewModels();

            HomeInitialize();

            this.Closing += MainWindow_Closing;
        }

        private void InitializeViewModels()
        {
            _homeViewModel = new HomeViewModel();
            _singleTestViewModel = new SingleTestViewModel();
            _coaxialTestViewModel = new CoaxialTestViewModel();
            _multiTestViewModel = new MultiTestViewModel();
            _apiViewModel = new APIViewModel();
            _scriptViewModel = new ScriptViewModel();
            _automateTestViewModel = new AutomateTestViewModel();
            _settingsViewModel = new SettingsViewModel();
            _balancerViewModel = new BalancerViewModel();
        }

        private void HomeInitialize()
        {
            foreach (var menuButton in LeftMenuPanel.Children.OfType<MenuButton>())
            {
                menuButton.IsActive = false;
                AnimateButtonScale(menuButton, 1.0);
            }

            HomeButton.IsActive = true;
            AnimateButtonScale(HomeButton, 1.15);
            ContentArea.Content = new HomePage { DataContext = _homeViewModel };
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
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
                        ContentArea.Content = new HomePage { DataContext = _homeViewModel };
                        break;
                    case PackIconMaterialKind.UsbPort:
                        ContentArea.Content = new SingleTestPage { DataContext = _singleTestViewModel };
                        break;
                    case PackIconMaterialKind.Usb:
                        ContentArea.Content = new CoaxialTestPage { DataContext = _coaxialTestViewModel };
                        break;
                    case PackIconMaterialKind.Multicast:
                        ContentArea.Content = new MultiTestPage { DataContext = _multiTestViewModel };
                        break;
                    case PackIconMaterialKind.ScaleBalance:
                        ContentArea.Content = new BalancerPage { DataContext = _balancerViewModel };
                        break;
                    case PackIconMaterialKind.Network:
                        ContentArea.Content = new APIPage { DataContext = _apiViewModel };
                        break;
                    case PackIconMaterialKind.Script:
                        ContentArea.Content = new ScriptPage { DataContext = _scriptViewModel };
                        break;
                    case PackIconMaterialKind.Autorenew:
                        ContentArea.Content = new AutomateTestPage { DataContext = _automateTestViewModel };
                        break;
                    case PackIconMaterialKind.Tools:
                        ContentArea.Content = new SettingsPage { DataContext = _settingsViewModel };
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
            try
            {
                foreach (var menuButton in LeftMenuPanel.Children.OfType<MenuButton>())
                {
                    menuButton.UpdateTheme();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying theme: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReloadContent()
        {
            // Ana pencerenin içeriğini yeniden yükleyin
            var newContent = new MainWindow();
            Application.Current.MainWindow.Content = newContent.Content;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
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
