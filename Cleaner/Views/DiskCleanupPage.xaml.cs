using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cleaner.Views;

public partial class DiskCleanupPage : Page
{
    public DiskCleanupPage()
    {
        InitializeComponent();
    }
}

public static class BooleanConverters
{
    public static readonly IValueConverter Invert = new InvertBooleanConverter();
    public static readonly IValueConverter ToVisibility = new BooleanToVisibilityConverter();
}

public class InvertBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => value is bool b ? !b : value;
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => value is bool b ? !b : value;
}
