using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CarSharing.Converters
{
    internal class IntegerFormatConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value is int i) return i;

            if (value is string s && int.TryParse(s, out i)) return i;

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (parameter is int i) ? i.ToString() : DependencyProperty.UnsetValue;
        }
    }
}
