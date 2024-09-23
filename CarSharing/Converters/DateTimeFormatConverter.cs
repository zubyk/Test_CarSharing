using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CarSharing.Converters
{
    internal class DateTimeFormatConverter : IValueConverter
    {
        const string _defaultDateTimeFormat = "dd.MM.yyyy HH:mm:ss";

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dt) return null;
            
            try
            {
                if (parameter == null || string.IsNullOrWhiteSpace(parameter.ToString()))
                {
                    return dt.ToString(_defaultDateTimeFormat);
                }
                else
                {
                    return dt.ToString(parameter.ToString());
                }
            }
            catch
            {
                return dt.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
