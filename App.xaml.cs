using Advanced_Dynotis_Software.Views.Windows;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;
using System.Threading;

namespace Advanced_Dynotis_Software
{
    public partial class App : Application
    {
        public static InterfaceVariables InterfaceVariables => InterfaceVariables.Instance;

        public App()
        {
            // Set the initial culture based on settings
            var languageCode = Advanced_Dynotis_Software.Properties.Settings.Default.languageCode;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(languageCode);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var appWindow = new MainWindow();
            appWindow.Show();
        }

        public static void ChangeLanguage(string language)
        {
            var cultureInfo = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;

            // Yeni dil ayarını kaydet
            Advanced_Dynotis_Software.Properties.Settings.Default.languageCode = language;
            Advanced_Dynotis_Software.Properties.Settings.Default.Save();

            SelectedIsCheckedUpdate(language);

            // Mevcut içeriği alın
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ReloadContent();
            }
        }

        public static void SelectedIsCheckedUpdate(string language)
        {
            if (language == "tr-TR")
            {
                InterfaceVariables.SelectedIsEnglishChecked = false;
                InterfaceVariables.SelectedIsTurkishChecked = true;
            }
            else
            {
                InterfaceVariables.SelectedIsEnglishChecked = true;
                InterfaceVariables.SelectedIsTurkishChecked = false;
            }
        }
    }
}
