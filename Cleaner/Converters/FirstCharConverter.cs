using System;
using System.Globalization;
using System.Windows.Data;

namespace Cleaner.Converters;

public class FirstCharConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value?.ToString() ?? "?";
        return s.Length > 0 ? s[0].ToString() : "?";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
