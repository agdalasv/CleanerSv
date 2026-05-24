using System;
using System.IO;
using System.Windows;

namespace Cleaner;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "CleanerLog.txt"),
                $"AppDomain: {args.ExceptionObject}");
        };
        DispatcherUnhandledException += (s, args) =>
        {
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "CleanerLog.txt"),
                $"Dispatcher: {args.Exception}");
            args.Handled = true;
        };

        var mainWindow = new MainWindow();
        mainWindow.Show();
        base.OnStartup(e);
    }
}
