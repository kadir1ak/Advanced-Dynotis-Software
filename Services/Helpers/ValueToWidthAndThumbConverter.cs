using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Advanced_Dynotis_Software.Services.Helpers
{
    public class ValueToWidthAndThumbConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double sliderValue && values[1] is double actualWidth)
            {
                double min = 800;
                double max = 2200;
                double percentage = (sliderValue - min) / (max - min);
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
