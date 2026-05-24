using System.Windows;
using System.Windows.Media;

namespace Cleaner.Services;

public enum ThemeMode
{
    Dark,
    Light
}

public class ThemeService
{
    public ThemeMode CurrentTheme { get; private set; } = ThemeMode.Dark;

    public event Action<ThemeMode>? ThemeChanged;

    public void ApplyTheme(ThemeMode mode)
    {
        CurrentTheme = mode;
        var resources = Application.Current.Resources;

        if (mode == ThemeMode.Dark)
        {
            resources["WindowBackground"] = new SolidColorBrush(Color.FromRgb(0x1F, 0x1F, 0x1F));
            resources["CardBackground"] = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x2D));
            resources["SidebarBackground"] = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x25));
            resources["TextPrimary"] = new SolidColorBrush(Colors.White);
            resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0xAB, 0xAB, 0xAB));
            resources["AccentColor"] = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
            resources["CardBorder"] = new SolidColorBrush(Color.FromRgb(0x3D, 0x3D, 0x3D));
            resources["HealthGreen"] = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
            resources["HealthYellow"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0x3B));
            resources["HealthRed"] = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
            resources["HoverColor"] = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3A));
            resources["SelectedColor"] = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A));
        }
        else
        {
            resources["WindowBackground"] = new SolidColorBrush(Color.FromRgb(0xF0, 0xF4, 0xF8));
            resources["CardBackground"] = new SolidColorBrush(Colors.White);
            resources["SidebarBackground"] = new SolidColorBrush(Color.FromRgb(0xE3, 0xEB, 0xF3));
            resources["TextPrimary"] = new SolidColorBrush(Color.FromRgb(0x1A, 0x2B, 0x3C));
            resources["TextSecondary"] = new SolidColorBrush(Color.FromRgb(0x6B, 0x7C, 0x8D));
            resources["AccentColor"] = new SolidColorBrush(Color.FromRgb(0x2B, 0x7F, 0xD4));
            resources["CardBorder"] = new SolidColorBrush(Color.FromRgb(0xD0, 0xDD, 0xEA));
            resources["HealthGreen"] = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
            resources["HealthYellow"] = new SolidColorBrush(Color.FromRgb(0xE6, 0x9F, 0x0E));
            resources["HealthRed"] = new SolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F));
            resources["HoverColor"] = new SolidColorBrush(Color.FromRgb(0xD6, 0xE4, 0xF0));
            resources["SelectedColor"] = new SolidColorBrush(Color.FromRgb(0xC8, 0xDA, 0xEA));
        }

        ThemeChanged?.Invoke(mode);
    }

    public void ToggleTheme()
    {
        ApplyTheme(CurrentTheme == ThemeMode.Dark ? ThemeMode.Light : ThemeMode.Dark);
    }
}
