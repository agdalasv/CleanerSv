using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Cleaner.Services;

namespace Cleaner.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly MainViewModel _main;
    private readonly SystemInfoService _sysInfo;
    private readonly DispatcherTimer _timer;
    private float _cpuUsage;
    private float _ramUsage;
    private float _ramAvailable;
    private float _totalRAM;
    private float _temperature;
    private string _diskHealth = "Excelente";
    private float _diskUsage;
    private string _healthScore = "92";
    private string _healthStatus = "Excelente";
    private string _wasteSpace = "0 GB";
    private string _systemUptime = "";
    private double _cpuAngle = 0;
    private double _ramAngle = 0;
    private int _refreshCount;

    public ObservableCollection<float> CpuHistory { get; } = new();
    public ObservableCollection<float> RamHistory { get; } = new();
    public ObservableCollection<float> HealthHistory { get; } = new();
    public ObservableCollection<float> TempHistory { get; } = new();

    private const int MaxHistory = 60;

    public string BtcAddress { get; } = "3L8f3v6BWwL7KBcb8AMZQ2bpE3ACne2EUf";

    public DashboardViewModel(MainViewModel main, SystemInfoService sysInfo)
    {
        _main = main;
        _sysInfo = sysInfo;
        _totalRAM = _sysInfo.GetTotalRAM();
        for (int i = 0; i < MaxHistory; i++)
        {
            CpuHistory.Add(0);
            RamHistory.Add(0);
            HealthHistory.Add(100);
            TempHistory.Add(35);
        }
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += async (s, e) => await RefreshDataAsync();
        _timer.Start();
        _ = RefreshDataAsync();
        RunCleanCommand = new RelayCommand(_ => RunClean());
        CopyBtcCommand = new RelayCommand(_ => CopyBtc());
    }

    public float CpuUsage
    {
        get => _cpuUsage;
        set { SetProperty(ref _cpuUsage, value); OnPropertyChanged(nameof(CpuUsageText)); CpuAngle = value * 3.6; }
    }
    public float RamUsage
    {
        get => _ramUsage;
        set { SetProperty(ref _ramUsage, value); OnPropertyChanged(nameof(RamUsageText)); RamAngle = value * 3.6; }
    }
    public float RamAvailable { get => _ramAvailable; set { SetProperty(ref _ramAvailable, value); OnPropertyChanged(nameof(RamAvailableText)); } }
    public float Temperature { get => _temperature; set { SetProperty(ref _temperature, value); OnPropertyChanged(nameof(TemperatureText)); } }
    public string DiskHealth { get => _diskHealth; set => SetProperty(ref _diskHealth, value); }
    public float DiskUsage { get => _diskUsage; set { SetProperty(ref _diskUsage, value); OnPropertyChanged(nameof(DiskUsageText)); } }
    public string HealthScore { get => _healthScore; set => SetProperty(ref _healthScore, value); }
    public string HealthStatus { get => _healthStatus; set => SetProperty(ref _healthStatus, value); }
    public string WasteSpace { get => _wasteSpace; set => SetProperty(ref _wasteSpace, value); }
    public string SystemUptime { get => _systemUptime; set => SetProperty(ref _systemUptime, value); }
    public double CpuAngle { get => _cpuAngle; set => SetProperty(ref _cpuAngle, value); }
    public double RamAngle { get => _ramAngle; set => SetProperty(ref _ramAngle, value); }

    public string CpuUsageText => $"{CpuUsage:F1}%";
    public string RamUsageText => $"{RamUsage:F1}%";
    public string RamAvailableText => $"{RamAvailable:F0} MB";
    public string TemperatureText => $"{Temperature:F1}°C";
    public string DiskUsageText => $"{DiskUsage:F1}%";

    public ICommand RunCleanCommand { get; }
    public ICommand CopyBtcCommand { get; }

    private async Task RefreshDataAsync()
    {
        _refreshCount++;

        CpuUsage = _sysInfo.GetCpuUsage();
        var avail = _sysInfo.GetAvailableRAM();
        RamAvailable = avail;
        RamUsage = _totalRAM > 0 ? Math.Min(100, (1 - (avail / _totalRAM)) * 100) : 0;
        DiskUsage = _sysInfo.GetDiskUsage();
        SystemUptime = _sysInfo.GetSystemUptime();

        if (_refreshCount % 5 == 0)
        {
            var t = await Task.Run(() => _sysInfo.GetCpuTemperature());
            Temperature = t;
        }
        AddHistory(TempHistory, Temperature);

        if (_refreshCount % 10 == 0)
            WasteSpace = await Task.Run(() =>
            {
                var wasteScan = new DiskCleanupService().ScanAll();
                var wasteSize = wasteScan.Sum(i => i.Size);
                var wasteGB = wasteSize / (1024.0 * 1024.0 * 1024.0);
                return wasteGB >= 0.1 ? $"{wasteGB:F1} GB" : $"{wasteSize / (1024.0 * 1024.0):F0} MB";
            });

        var healthVal = 100 - (CpuUsage * 0.2f) - (RamUsage * 0.2f) - (DiskUsage * 0.1f) - (Temperature > 70 ? 10 : Temperature > 50 ? 5 : 0);
        healthVal = Math.Clamp(healthVal, 0, 100);
        HealthScore = $"{healthVal:F0}";
        HealthStatus = healthVal >= 80 ? "Excelente" : healthVal >= 60 ? "Bueno" : healthVal >= 40 ? "Regular" : "Malo";

        AddHistory(CpuHistory, CpuUsage);
        AddHistory(RamHistory, RamUsage);
        AddHistory(HealthHistory, healthVal);
    }

    private void AddHistory(ObservableCollection<float> list, float value)
    {
        list.RemoveAt(0);
        list.Add(value);
    }

    private void CopyBtc()
    {
        try
        {
            Clipboard.SetText(BtcAddress);
        }
        catch { }
    }

    private void RunClean()
    {
        _main.NavigateCommand.Execute("DiskCleanup");
    }
}
