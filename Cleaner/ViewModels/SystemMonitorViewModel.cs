using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using Cleaner.Services;

namespace Cleaner.ViewModels;

public class SystemMonitorViewModel : BaseViewModel
{
    private readonly SystemInfoService _sysInfo;
    private readonly DispatcherTimer _timer;
    private float _cpu;
    private float _ram;
    private float _gpu;
    private float _disk;
    private float _temperature;
    private string _topProcesses = "";
    private int _activeProcesses;
    private string _batteryStatus = "Cargado 100%";
    private int _tick;

    public SystemMonitorViewModel(SystemInfoService sysInfo)
    {
        _sysInfo = sysInfo;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += async (s, e) => await RefreshAsync();
        _timer.Start();
        _ = RefreshAsync();
    }

    public float Cpu { get => _cpu; set { SetProperty(ref _cpu, value); OnPropertyChanged(nameof(CpuText)); } }
    public float Ram { get => _ram; set { SetProperty(ref _ram, value); OnPropertyChanged(nameof(RamText)); } }
    public float Gpu { get => _gpu; set { SetProperty(ref _gpu, value); OnPropertyChanged(nameof(GpuText)); } }
    public float Disk { get => _disk; set { SetProperty(ref _disk, value); OnPropertyChanged(nameof(DiskText)); } }
    public float Temperature { get => _temperature; set { SetProperty(ref _temperature, value); OnPropertyChanged(nameof(TemperatureText)); } }
    public string TopProcesses { get => _topProcesses; set => SetProperty(ref _topProcesses, value); }
    public int ActiveProcesses { get => _activeProcesses; set => SetProperty(ref _activeProcesses, value); }
    public string BatteryStatus { get => _batteryStatus; set => SetProperty(ref _batteryStatus, value); }

    public string CpuText => $"{Cpu:F1}%";
    public string RamText => $"{Ram:F1}%";
    public string GpuText => $"{Gpu:F1}%";
    public string DiskText => $"{Disk:F1}%";
    public string TemperatureText => $"{Temperature:F1}°C";

    public ICommand RefreshCommand => new RelayCommand(_ => { _ = RefreshAsync(); });

    private async Task RefreshAsync()
    {
        _tick++;

        // Fast: perf counters on UI thread
        Cpu = _sysInfo.GetCpuUsage();
        var avail = _sysInfo.GetAvailableRAM();
        var total = _sysInfo.GetTotalRAM();
        Ram = total > 0 ? Math.Min(100, (1 - (avail / total)) * 100) : 0;
        Disk = _sysInfo.GetDiskUsage();

        // WMI temperature every 5 seconds
        if (_tick % 5 == 0)
            Temperature = await Task.Run(() => _sysInfo.GetCpuTemperature());

        // Process list every 3 seconds
        if (_tick % 3 == 0)
        {
            await Task.Run(() =>
            {
                try
                {
                    var procs = System.Diagnostics.Process.GetProcesses();
                    ActiveProcesses = procs.Length;
                    var top = procs
                        .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                        .OrderByDescending(p =>
                        {
                            try { return p.PrivateMemorySize64; }
                            catch { return 0L; }
                        })
                        .Take(5)
                        .Select(p =>
                        {
                            try { return $"{p.ProcessName} ({p.PrivateMemorySize64 / 1024 / 1024}MB)"; }
                            catch { return p.ProcessName; }
                        });
                    TopProcesses = string.Join("\n", top);
                }
                catch { }
            });
        }
    }
}
