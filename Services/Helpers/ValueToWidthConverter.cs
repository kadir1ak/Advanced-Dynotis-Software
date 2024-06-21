using System;
using System.Globalization;
using System.Windows.Data;

namespace Advanced_Dynotis_Software.Services.Helpers
{
    public class ValueToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = (double)value;
            return val * 3; // Adjust this multiplier as needed for your application.
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
