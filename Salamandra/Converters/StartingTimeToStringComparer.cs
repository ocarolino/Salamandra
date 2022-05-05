using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Salamandra.Converters
{
    public class StartingTimeToStringComparer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)values[0];
            bool usePlayingHours = (bool)values[1];

            if (usePlayingHours)
                return String.Format("__:{0:00}:{1:00}", date.TimeOfDay.Minutes, date.TimeOfDay.Seconds);
            else
                return date.ToString("HH:mm:ss");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
