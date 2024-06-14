using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Advanced_Dynotis_Software.Themes
{
    public static class AppTheme
    {
        public static event EventHandler ThemeChanged;

        public static void ChangeTheme(Uri themeUri)
        {
            ResourceDictionary newTheme = new ResourceDictionary() { Source = themeUri };
            ResourceDictionary oldTheme = Application.Current.Resources.MergedDictionaries[0];

            foreach (var key in newTheme.Keys)
            {
                if (newTheme[key] is SolidColorBrush newBrush && oldTheme[key] is SolidColorBrush oldBrush)
                {
                    // Create a copy of the current brush to avoid the frozen brush issue
                    SolidColorBrush animatedBrush = new SolidColorBrush(oldBrush.Color);
                    animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation
                    {
                        From = oldBrush.Color,
                        To = newBrush.Color,
                        Duration = new Duration(TimeSpan.FromMilliseconds(250))
                    });

                    // Replace the frozen brush with the new animated brush
                    Application.Current.Resources[key] = animatedBrush;
                }
            }

            // Clear the old theme and add the new theme
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(newTheme);

            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
