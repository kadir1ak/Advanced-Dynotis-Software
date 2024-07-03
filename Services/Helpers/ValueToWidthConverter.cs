﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Advanced_Dynotis_Software.Services.Helpers
{
    public class ValueToWidthAndThumbConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double sliderValue && values[1] is double actualWidth)
            {
                double percentage = sliderValue / 100;
                return percentage * actualWidth;
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
