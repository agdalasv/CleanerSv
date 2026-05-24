using System;
using System.Globalization;
using System.Windows.Data;

namespace Cleaner.Converters;

public class BoolToLockIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "🔒" : "🔓";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
