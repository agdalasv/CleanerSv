using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace Cleaner.ViewModels;

public class SystemOptimizerViewModel : BaseViewModel
{
    private bool _isOptimizing;
    private string _statusMessage = "Listo para optimizar";
    private int _startupPrograms = 0;
    private int _unnecessaryServices = 0;
    private bool _turboMode;
    private bool _gamingMode;
    private bool _networkOpt;

    public ObservableCollection<StartupItem> StartupList { get; } = new();
    public ObservableCollection<ServiceItem> ServiceList { get; } = new();

    public bool IsOptimizing { get => _isOptimizing; set => SetProperty(ref _isOptimizing, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public int StartupPrograms { get => _startupPrograms; set => SetProperty(ref _startupPrograms, value); }
    public int UnnecessaryServices { get => _unnecessaryServices; set => SetProperty(ref _unnecessaryServices, value); }
    public bool TurboMode { get => _turboMode; set => SetProperty(ref _turboMode, value); }
    public bool GamingMode { get => _gamingMode; set => SetProperty(ref _gamingMode, value); }
    public bool NetworkOpt { get => _networkOpt; set => SetProperty(ref _networkOpt, value); }

    public ICommand AnalyzeCommand { get; }
    public ICommand OptimizeCommand { get; }

    public SystemOptimizerViewModel()
    {
        AnalyzeCommand = new RelayCommand(_ => Analyze());
        OptimizeCommand = new RelayCommand(_ => Optimize());
    }

    private void ScanStartupPrograms()
    {
        StartupList.Clear();
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            if (key != null)
                foreach (var name in key.GetValueNames())
                {
                    var val = key.GetValue(name)?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(name))
                        StartupList.Add(new StartupItem { Name = name, Command = val, Source = "HKCU" });
                }
        }
        catch { }
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            if (key != null)
                foreach (var name in key.GetValueNames())
                {
                    var val = key.GetValue(name)?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(name) && !StartupList.Any(s => s.Name == name))
                        StartupList.Add(new StartupItem { Name = name, Command = val, Source = "HKLM" });
                }
        }
        catch { }
        StartupPrograms = StartupList.Count;
    }

    private void ScanUnnecessaryServices()
    {
        ServiceList.Clear();
        var checkList = new (string name, string desc)[]
        {
            ("XblAuthManager", "Autenticación Xbox Live"),
            ("XboxNetApiSvc", "Red Xbox Live"),
            ("XboxGipSvc", "Accesorios Xbox"),
            ("WSearch", "Búsqueda de Windows"),
            ("DiagTrack", "Seguimiento de diagnóstico"),
            ("TabletInputService", "Entrada táctil / lápiz"),
            ("PrintSpooler", "Cola de impresión"),
            ("WMPNetworkSvc", "WMP Compartir en red"),
            ("RemoteRegistry", "Registro remoto"),
            ("TermService", "Escritorio remoto"),
            ("SysMain", "SysMain (Superfetch)"),
            ("lfsvc", "Servicio de ubicación"),
            ("dmwappushservice", "Push de dispositivos WAP"),
            ("PcaSvc", "Asistente de compatibilidad"),
            ("WlanSvc", "Wi-Fi (si usas cable)"),
            ("SharedAccess", "Compartir Internet ICS"),
            ("WerSvc", "Informes de errores Windows"),
            ("Fax", "Fax"),
            ("XboxNetApiSvc", "Red Xbox Live"),
        };

        foreach (var (svc, desc) in checkList)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{svc}");
                if (key == null) continue;
                var start = key.GetValue("Start")?.ToString();
                var displayName = key.GetValue("DisplayName")?.ToString() ?? svc;
                if (start == "2" || start == "3")
                    ServiceList.Add(new ServiceItem
                    {
                        Name = svc,
                        DisplayName = displayName,
                        Description = desc,
                        StartType = start == "2" ? "Automático" : "Manual"
                    });
            }
            catch { }
        }
        UnnecessaryServices = ServiceList.Count;
    }

    private async void Analyze()
    {
        IsOptimizing = true;
        StatusMessage = "Analizando sistema...";
        await Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ScanStartupPrograms();
                ScanUnnecessaryServices();
            });
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = $"Análisis completo: {StartupPrograms} programas de inicio, {UnnecessaryServices} servicios innecesarios";
                IsOptimizing = false;
            });
        });
    }

    private async void Optimize()
    {
        IsOptimizing = true;
        StatusMessage = "Optimizando sistema...";
        var removedStartup = 0;
        var stoppedServices = 0;

        await Task.Run(() =>
        {
            try
            {
                if (TurboMode || GamingMode)
                {
                    using var key = Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    if (key != null)
                    {
                        var names = key.GetValueNames();
                        foreach (var n in names)
                        {
                            try { key.DeleteValue(n); removedStartup++; } catch { }
                        }
                    }
                    using var key2 = Registry.LocalMachine.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    if (key2 != null)
                    {
                        var names = key2.GetValueNames();
                        foreach (var n in names)
                        {
                            try { key2.DeleteValue(n); removedStartup++; } catch { }
                        }
                    }
                }
            }
            catch { }

            try
            {
                var toDisable = new[] {
                    "XblAuthManager", "XboxNetApiSvc", "XboxGipSvc",
                    "WSearch", "DiagTrack", "TabletInputService",
                    "WMPNetworkSvc", "RemoteRegistry", "lfsvc",
                    "dmwappushservice", "PcaSvc", "Fax", "WerSvc"
                };
                foreach (var svc in toDisable)
                {
                    try
                    {
                        using var key = Registry.LocalMachine.OpenSubKey(
                            $@"SYSTEM\CurrentControlSet\Services\{svc}", true);
                        if (key?.GetValue("Start")?.ToString() == "2" ||
                            key?.GetValue("Start")?.ToString() == "3")
                        {
                            key.SetValue("Start", 4, RegistryValueKind.DWord);
                            stoppedServices++;
                        }
                    }
                    catch { }
                }
            }
            catch { }

            Application.Current.Dispatcher.Invoke(() =>
            {
                ScanStartupPrograms();
                ScanUnnecessaryServices();
                StatusMessage = $"Optimización completada: {removedStartup} programas eliminados, {stoppedServices} servicios desactivados";
                IsOptimizing = false;
            });
        });
    }
}

public class StartupItem
{
    public string Name { get; set; } = "";
    public string Command { get; set; } = "";
    public string Source { get; set; } = "";
}

public class ServiceItem
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
    public string StartType { get; set; } = "";
}
