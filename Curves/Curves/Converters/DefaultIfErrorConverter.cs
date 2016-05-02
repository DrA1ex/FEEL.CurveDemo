using System;
using System.Globalization;
using System.Windows.Data;

namespace Curves.Converters
{
    internal class DefaultIfErrorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(targetType == typeof(double))
            {
                double result;
                double.TryParse(value.ToString(), out result);

                return result;
            }
            if(targetType == typeof(uint))
            {
                int result;
                int.TryParse(value.ToString(), out result);

                return result;
            }

            throw new NotSupportedException(string.Format("Type {0} not supported!", targetType));
        }
    }
}