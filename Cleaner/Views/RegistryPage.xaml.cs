using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Cleaner.Views;

public partial class RegistryPage : Page
{
    public RegistryPage()
    {
        InitializeComponent();
    }
}

public class SeverityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Alta" => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)),
            "Media" => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)),
            "Baja" => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),
            _ => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => throw new NotImplementedException();
}

public static class SeverityConverters
{
    public static readonly IValueConverter Instance = new SeverityConverter();
}
