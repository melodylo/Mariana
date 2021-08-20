using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DatabaseToGraph
{
    public class MinMaxConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value)
            {
                return Brushes.Transparent;
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(255, 204, 196));
            }
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == Brushes.Transparent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class YAxisConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString()[0].ToString().ToUpper();
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString().ToLower() == "P")
            {
                return "primary";
            }
            else if (value.ToString().ToLower() == "S")
            {
                return "secondary";
            }
            else
            {
                return "other";
            }
        }
    }
}
