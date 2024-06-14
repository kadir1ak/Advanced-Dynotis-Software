using System;
using System.Windows;

namespace Advanced_Dynotis_Software.Themes
{
    public static class AppTheme
    {
        public static event EventHandler ThemeChanged;

        public static void ChangeTheme(Uri themeUri)
        {
            ResourceDictionary theme = new ResourceDictionary() { Source = themeUri };
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Add(theme);
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
