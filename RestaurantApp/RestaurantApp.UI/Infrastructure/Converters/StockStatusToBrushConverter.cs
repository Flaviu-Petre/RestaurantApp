using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RestaurantApp.UI.Infrastructure.Converters
{
    public class StockStatusToBrushConverter : IValueConverter
    {
        public ResourceDictionary StatusBrushes { get; set; } = new ResourceDictionary();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status && StatusBrushes.Contains(status))
            {
                return StatusBrushes[status];
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}