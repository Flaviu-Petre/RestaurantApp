using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestaurantApp.UI.Infrastructure.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter as string == "Invert";
            bool boolValue = value as bool? ?? false;

            if (invert)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter as string == "Invert";
            Visibility visibility = (Visibility)value;
            bool result = visibility == Visibility.Visible;

            if (invert)
                result = !result;

            return result;
        }
    }
}