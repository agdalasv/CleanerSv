using System;
using System.Globalization;
using System.Windows.Data;

namespace Cleaner.Converters;

public class ScaleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        float val = (float)value;
        if (float.IsNaN(val) || float.IsInfinity(val)) val = 0;
        var h = Math.Clamp(val / 100f * 40f, 0, 40);
        return h;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
