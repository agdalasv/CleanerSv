using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows.Input;
using Cleaner.Services;

namespace Cleaner.ViewModels;

public class RamOptimizerViewModel : BaseViewModel
{
    private readonly DispatcherTimer _timer;
    private float _ramUsage;
    private float _available;
    private float _total;
    private bool _isOptimizing;
    private string _statusMessage = "Monitor de RAM activo";

    public RamOptimizerViewModel()
    {
        _total = new SystemInfoService().GetTotalRAM();
        TopProcesses = new ObservableCollection<ProcessInfo>();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += (s, e) => Refresh();
        _timer.Start();
        Refresh();
        OptimizeCommand = new RelayCommand(_ => Optimize());
        StartGamingModeCommand = new RelayCommand(_ => GamingMode());
    }

    public ObservableCollection<ProcessInfo> TopProcesses { get; }

    public float RamUsage { get => _ramUsage; set { SetProperty(ref _ramUsage, value); OnPropertyChanged(nameof(RamUsageText)); } }
    public float Available { get => _available; set => SetProperty(ref _available, value); }
    public float Total { get => _total; set => SetProperty(ref _total, value); }
    public bool IsOptimizing { get => _isOptimizing; set => SetProperty(ref _isOptimizing, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    public string RamUsageText => $"{RamUsage:F1}%";
    public string AvailableText => $"{Available / 1024:F1} GB / {Total / 1024:F1} GB";

    public ICommand OptimizeCommand { get; }
    public ICommand StartGamingModeCommand { get; }

    private void Refresh()
    {
        var sys = new SystemInfoService();
        var avail = sys.GetAvailableRAM();
        Available = avail;
        RamUsage = _total > 0 ? Math.Min(100, (1 - (avail / _total)) * 100) : 0;
        var procs = new SystemInfoService().GetTopProcesses(10);
        TopProcesses.Clear();
        foreach (var p in procs)
            TopProcesses.Add(p);
    }

    private async void Optimize()
    {
        IsOptimizing = true;
        StatusMessage = "Liberando memoria RAM...";

        await Task.Run(() =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var procs = Process.GetProcesses();
            foreach (var p in procs)
            {
                try
                {
                    if (p.Id == 0 || p.Id == 4) continue;
                    NativeMethods.EmptyWorkingSet(p.Handle);
                }
                catch { }
                finally { p.Dispose(); }
            }

            var self = Process.GetCurrentProcess();
            NativeMethods.SetProcessWorkingSetSize(self.Handle, -1, -1);
        });

        await Task.Delay(1500);
        Refresh();
        IsOptimizing = false;
        StatusMessage = $"RAM liberada. Uso actual: {RamUsage:F1}%";
    }

    private async void GamingMode()
    {
        IsOptimizing = true;
        StatusMessage = "Modo gaming: optimizando RAM y prioridad...";

        await Task.Run(() =>
        {
            try { Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High; } catch { }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var procs = Process.GetProcesses();
            foreach (var p in procs)
            {
                try
                {
                    if (p.Id == 0 || p.Id == 4) continue;
                    if (p.ProcessName.Contains("Cleaner")) continue;
                    NativeMethods.EmptyWorkingSet(p.Handle);
                }
                catch { }
                finally { p.Dispose(); }
            }
        });

        await Task.Delay(1500);
        Refresh();
        IsOptimizing = false;
        StatusMessage = "Modo gaming activado: máximo rendimiento";
    }
}

internal static class NativeMethods
{
    [DllImport("kernel32.dll")]
    internal static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

    [DllImport("psapi.dll")]
    internal static extern int EmptyWorkingSet(IntPtr hProcess);
}
