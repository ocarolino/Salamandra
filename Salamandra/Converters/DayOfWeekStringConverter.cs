using Salamandra.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Salamandra.Converters
{
    public class DayOfWeekStringConverter : IValueConverter
    {
        // ToDo: Lançar um parãmetro pra short
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DayOfWeek dayOfWeek = (DayOfWeek)value;
            culture = Thread.CurrentThread.CurrentUICulture;

            return culture.DateTimeFormat.GetAbbreviatedDayName(dayOfWeek).FirstCharToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
