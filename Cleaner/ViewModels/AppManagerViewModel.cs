using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.Win32;

namespace Cleaner.ViewModels;

public class AppManagerViewModel : BaseViewModel
{
    private bool _isScanning;
    private string _statusMessage = "Listo";

    public AppManagerViewModel()
    {
        InstalledApps = new ObservableCollection<AppItem>();
        ScanCommand = new RelayCommand(_ => Scan());
        UninstallCommand = new RelayCommand(_ => Uninstall(), _ => SelectedApp != null);
    }

    public ObservableCollection<AppItem> InstalledApps { get; }
    public bool IsScanning { get => _isScanning; set => SetProperty(ref _isScanning, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    private AppItem? _selectedApp;
    public AppItem? SelectedApp
    {
        get => _selectedApp;
        set => SetProperty(ref _selectedApp, value);
    }

    public ICommand ScanCommand { get; }
    public ICommand UninstallCommand { get; }

    private async void Scan()
    {
        IsScanning = true;
        StatusMessage = "Escaneando aplicaciones instaladas...";
        InstalledApps.Clear();

        await Task.Run(() =>
        {
            var apps = new List<AppItem>();
            var paths = new[]
            {
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
            };

            foreach (var key in paths)
            {
                if (key == null) continue;
                foreach (var subName in key.GetSubKeyNames())
                {
                    try
                    {
                        using var subKey = key.OpenSubKey(subName);
                        var name = subKey?.GetValue("DisplayName")?.ToString();
                        if (string.IsNullOrEmpty(name)) continue;
                        var sizeStr = subKey?.GetValue("EstimatedSize")?.ToString();
                        long size = 0;
                        if (long.TryParse(sizeStr, out var s)) size = s * 1024L;
                        var displayIcon = subKey?.GetValue("DisplayIcon")?.ToString() ?? "";
                        apps.Add(new AppItem
                        {
                            Name = name,
                            Size = size,
                            Version = subKey?.GetValue("DisplayVersion")?.ToString() ?? "N/A",
                            Publisher = subKey?.GetValue("Publisher")?.ToString() ?? "Desconocido",
                            UninstallString = subKey?.GetValue("UninstallString")?.ToString() ?? "",
                            RegistryPath = subName,
                            DisplayIcon = displayIcon
                        });
                    }
                    catch { }
                }
                key.Dispose();
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var app in apps.OrderBy(a => a.Name))
                    InstalledApps.Add(app);
                StatusMessage = $"{apps.Count} aplicaciones encontradas";
                IsScanning = false;
            });
        });
    }

    private void Uninstall()
    {
        if (SelectedApp == null) return;
        StatusMessage = $"Iniciando desinstalación de {SelectedApp.Name}...";

        try
        {
            var cmd = SelectedApp.UninstallString;
            if (!string.IsNullOrEmpty(cmd))
            {
                cmd = cmd.Replace("/I", "/X ").Replace("MsiExec.exe", "msiexec.exe");
                if (!cmd.Contains("msiexec") && !cmd.Contains(".exe") && !cmd.Contains("rundll"))
                    cmd = $"msiexec /x \"{SelectedApp.RegistryPath}\"";

                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"\" /wait \"{cmd}\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                StatusMessage = $"Desinstalando {SelectedApp.Name}...";
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"\" /wait msiexec /x \"{SelectedApp.RegistryPath}\"",
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                StatusMessage = $"Desinstalando {SelectedApp.Name}...";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}

public class AppItem
{
    public string Name { get; set; } = "";
    public long Size { get; set; }
    public string SizeFormatted => Size >= 1073741824 ? $"{Size / 1073741824.0:F2} GB" :
                                    Size >= 1048576 ? $"{Size / 1048576.0:F2} MB" :
                                    Size >= 1024 ? $"{Size / 1024.0:F2} KB" : $"{Size} B";
    public string Version { get; set; } = "";
    public string Publisher { get; set; } = "";
    public string UninstallString { get; set; } = "";
    public string RegistryPath { get; set; } = "";
    public string DisplayIcon { get; set; } = "";
    public char Initial => Name.Length > 0 ? Name[0] : '?';
}
