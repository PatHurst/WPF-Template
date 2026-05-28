using System.Globalization;
using System.Windows.Data;
using System.Windows.Shapes;

namespace WPFTemplate.App.Converters;

[ValueConversion(typeof(Color), typeof(Rectangle))]
internal class ColorSwatchConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Color c
            ? new Rectangle
            {
                Fill = new SolidColorBrush(c),
                RadiusX = 2,
                RadiusY = 2,
                Width = 12,
                Height = 12,
            }
            : null;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
