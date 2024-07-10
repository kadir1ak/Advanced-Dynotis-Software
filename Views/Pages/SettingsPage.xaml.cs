using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;
using Advanced_Dynotis_Software.ViewModels.Pages;
using Advanced_Dynotis_Software.Views.UserControls;

namespace Advanced_Dynotis_Software.Views.Pages
{
    public partial class SettingsPage : UserControl
    {
        private UnitsSettingsViewModel _unitsSettingsViewModel;
        private AlertsViewModel _alertsViewModel;
        private ESCCalibrationViewModel _escCalibrationViewModel;
        private SoftwareUpdateViewModel _softwareUpdateViewModel;
        private LanguageViewModel _languageViewModel;
        private AboutViewModel _aboutViewModel;
        private HelpViewModel _helpViewModel;
        private SaveConfigViewModel _saveConfigViewModel;

        public SettingsPage()
        {
            InitializeComponent();
            _unitsSettingsViewModel = new UnitsSettingsViewModel();
            _alertsViewModel = new AlertsViewModel();
            _softwareUpdateViewModel = new SoftwareUpdateViewModel();
            _languageViewModel = new LanguageViewModel();
            _aboutViewModel = new AboutViewModel();
            _helpViewModel = new HelpViewModel();
            _saveConfigViewModel = new SaveConfigViewModel();
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is SettingButton clickedButton)
            {
                if (!clickedButton.IsActive)
                {
                    foreach (var settingButton in MenuButtonPanel.Children.OfType<SettingButton>())
                    {
                        settingButton.IsActive = false;
                    }

                    clickedButton.IsActive = true;
                }

                switch (clickedButton.Icon)
                {
                    case PackIconMaterialKind.ContentSaveCog:
                        ContentArea.Content = new SaveConfigPage { DataContext = _saveConfigViewModel };
                        break;
                    case PackIconMaterialKind.Layers:
                        ContentArea.Content = new UnitsSettingsPage { DataContext = _unitsSettingsViewModel };
                        break;
                    case PackIconMaterialKind.AlertCircleCheckOutline:
                        ContentArea.Content = new AlertsPage { DataContext = _alertsViewModel };
                        break;
                    case PackIconMaterialKind.Quadcopter:
                        ContentArea.Content = new ESCCalibrationPage { DataContext = _escCalibrationViewModel };
                        break;
                    case PackIconMaterialKind.Update:
                        ContentArea.Content = new SoftwareUpdatePage { DataContext = _softwareUpdateViewModel };
                        break;
                    case PackIconMaterialKind.Translate:
                        ContentArea.Content = new LanguagePage { DataContext = _languageViewModel };
                        break;
                    case PackIconMaterialKind.Information:
                        ContentArea.Content = new AboutPage { DataContext = _aboutViewModel };
                        break;
                    case PackIconMaterialKind.Help:
                        ContentArea.Content = new HelpPage { DataContext = _helpViewModel };
                        break;
                    default:
                        MessageBox.Show("Unknown button clicked!");
                        break;
                }
            }
        }
    }
}
