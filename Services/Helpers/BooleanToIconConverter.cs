using MahApps.Metro.IconPacks;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Advanced_Dynotis_Software.Services.Helpers
{
    public class BooleanToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? PackIconMaterialKind.StopCircleOutline : PackIconMaterialKind.PlayCircleOutline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
