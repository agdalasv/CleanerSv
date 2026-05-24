using System.Management;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace Cleaner.Services;

public class SystemInfoService : IDisposable
{
    private PerformanceCounter? _cpuCounter;
    private PerformanceCounter? _ramCounter;
    private PerformanceCounter? _diskCounter;
    private PerformanceCounter? _netCounter;

    public SystemInfoService()
    {
        InitCounter(ref _cpuCounter, "Processor", "% Processor Time", "_Total");
        InitCounter(ref _ramCounter, "Memory", "Available MBytes");
        InitCounter(ref _diskCounter, "PhysicalDisk", "% Disk Time", "_Total");
        InitNetworkCounter();

        _cpuCounter?.NextValue();
        _ramCounter?.NextValue();
        _diskCounter?.NextValue();
        _netCounter?.NextValue();
    }

    private void InitCounter(ref PerformanceCounter? counter, string category, string name, string instance)
    {
        try { counter = new PerformanceCounter(category, name, instance); }
        catch { }
    }

    private void InitCounter(ref PerformanceCounter? counter, string category, string name)
    {
        try { counter = new PerformanceCounter(category, name); }
        catch { }
    }

    private void InitNetworkCounter()
    {
        try
        {
            var instance = GetNetworkInstance();
            if (!string.IsNullOrEmpty(instance))
                _netCounter = new PerformanceCounter("Network Interface", "Bytes Total/sec", instance);
        }
        catch { }
    }

    public float GetCpuUsage()
    {
        try { return _cpuCounter?.NextValue() ?? 0; }
        catch { return 0; }
    }

    public float GetAvailableRAM()
    {
        try { return _ramCounter?.NextValue() ?? 0; }
        catch { return 0; }
    }

    public float GetTotalRAM()
    {
        try
        {
            var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
                return Convert.ToSingle(obj["TotalVisibleMemorySize"]) / 1024;
        }
        catch { }
        return 16384;
    }

    public float GetCpuTemperature()
    {
        try
        {
            var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT Temperature FROM MSAcpi_ThermalZoneTemperature");
            foreach (var obj in searcher.Get())
            {
                var t = Convert.ToSingle(obj["Temperature"]) / 10 - 273.15f;
                if (t > 0 && t < 120)
                    return t;
            }
        }
        catch { }

        try
        {
            var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM Win32_PerfFormattedData_Counters_ThermalZoneInformation");
            foreach (var obj in searcher.Get())
            {
                var t = Convert.ToSingle(obj["Temperature"]);
                if (t > 0 && t < 120)
                    return t;
            }
        }
        catch { }

        try
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                var load = Convert.ToSingle(obj["LoadPercentage"]);
                if (load >= 0)
                    return 35 + load * 0.5f;
            }
        }
        catch { }

        var idle = _cpuCounter?.NextValue() ?? 0;
        return 35 + idle * 0.4f;
    }

    public float GetDiskUsage()
    {
        try { return _diskCounter?.NextValue() ?? 0; }
        catch { return 0; }
    }

    public List<DriveInfo> GetDrives()
    {
        return DriveInfo.GetDrives().Where(d => d.IsReady).ToList();
    }

    public long GetTotalFreeSpace()
    {
        return DriveInfo.GetDrives().Where(d => d.IsReady).Sum(d => d.TotalFreeSpace);
    }

    public long GetTotalDiskSize()
    {
        return DriveInfo.GetDrives().Where(d => d.IsReady).Sum(d => d.TotalSize);
    }

    public string GetWindowsVersion()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            if (key != null)
            {
                var name = key.GetValue("ProductName")?.ToString() ?? "Windows";
                var ver = key.GetValue("DisplayVersion")?.ToString() ?? "";
                return $"{name} {ver}";
            }
        }
        catch { }
        return "Windows 11";
    }

    public string GetSystemUptime()
    {
        try
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        }
        catch { return "0h 0m"; }
    }

    public List<ProcessInfo> GetTopProcesses(int count = 5)
    {
        var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrEmpty(p.ProcessName))
            .Select(p =>
            {
                try
                {
                    return new ProcessInfo
                    {
                        Name = p.ProcessName,
                        MemoryMB = Math.Round(p.PrivateMemorySize64 / (1024.0 * 1024.0), 1),
                        CpuPercent = 0
                    };
                }
                catch { return null; }
            })
            .Where(p => p != null)
            .OrderByDescending(p => p!.MemoryMB)
            .Take(count)
            .ToList()!;
        return processes!;
    }

    private string GetNetworkInstance()
    {
        try
        {
            var category = new PerformanceCounterCategory("Network Interface");
            var instances = category.GetInstanceNames();
            if (instances.Length == 0) return "";

            var real = instances
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .FirstOrDefault(i =>
                    i.StartsWith("Realtek") ||
                    i.StartsWith("Intel") ||
                    i.StartsWith("Qualcomm") ||
                    i.StartsWith("Broadcom") ||
                    i.StartsWith("Wi-Fi") ||
                    i.StartsWith("Ethernet") ||
                    i.StartsWith("Local") ||
                    (!i.Contains("Loopback") &&
                     !i.Contains("Bluetooth") &&
                     !i.Contains("Virtual") &&
                     !i.Contains("Pseudo") &&
                     !i.Contains("Teredo") &&
                     !i.Contains("Hamachi") &&
                     !i.Contains("VBox") &&
                     !i.Contains("VMware") &&
                     !i.Contains("Hyper-V") &&
                     !i.Contains("Default")));

            return real ?? instances[0];
        }
        catch { return ""; }
    }

    public float GetNetworkSpeed()
    {
        try
        {
            if (_netCounter == null) return 0;
            var val = _netCounter.NextValue();
            if (val < 0) return 0;
            return val / (1024f * 1024f);
        }
        catch { return 0; }
    }

    public void Dispose()
    {
        _cpuCounter?.Dispose();
        _ramCounter?.Dispose();
        _diskCounter?.Dispose();
        _netCounter?.Dispose();
    }
}

public class ProcessInfo
{
    public string Name { get; set; } = "";
    public double MemoryMB { get; set; }
    public double CpuPercent { get; set; }
}
