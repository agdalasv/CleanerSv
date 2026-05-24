using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Cleaner.Services;
using Cleaner.ViewModels;
using Forms = System.Windows.Forms;

namespace Cleaner;

public partial class MainWindow : Window
{
    private Forms.NotifyIcon? _trayIcon;

    public MainWindow()
    {
        try
        {
            InitializeComponent();
            var themeService = new ThemeService();
            DataContext = new MainViewModel(themeService);
            InitTrayIcon();
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "CleanerCrash.txt"),
                $"MainWindow constructor error:\n{ex}");
        }
    }

    private void InitTrayIcon()
    {
        _trayIcon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(GetType().Assembly.Location),
            Text = "Cleaner Sv",
            Visible = true
        };
        _trayIcon.Click += (_, _) =>
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        };
        StateChanged += (_, _) =>
        {
            if (WindowState == WindowState.Minimized)
                Hide();
        };
        Closed += (_, _) => _trayIcon?.Dispose();
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void SidebarHeader_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
