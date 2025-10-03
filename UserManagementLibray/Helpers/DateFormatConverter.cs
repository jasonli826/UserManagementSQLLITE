using System;
using System.Globalization;
using System.Windows.Data;

namespace UserManagementLibray.Helpers
{
    public class DateFormatConverter : IValueConverter
    {
        private const string DateFormat = "dd MMM yyyy HH:mm:ss"; // correct format

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
                return date.ToString(DateFormat);
            if (value is DateTime?)
            {
                DateTime? nullableDate = (DateTime?)value;
                return nullableDate?.ToString(DateFormat) ?? "";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (DateTime.TryParseExact(value?.ToString(), DateFormat, culture, DateTimeStyles.None, out var date))
                return date;
            return null;
        }
    }
}
