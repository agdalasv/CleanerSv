using System.IO;

namespace Cleaner.Services;

public class DiskCleanupService
{
    public List<CleanupItem> ScanAll()
    {
        var items = new List<CleanupItem>();
        ScanPath(items, "Archivos temporales - Sistema", Path.GetTempPath(), "Temporales de Sistema", "Bajo");
        ScanPath(items, "Archivos temporales - Windows",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
            "Temporales de Windows", "Bajo");
        ScanPath(items, "Archivos temporales - Apps",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"),
            "Temporales de Apps", "Bajo");
        ScanPath(items, "Caché de Internet/Edge",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "INetCache"),
            "Caché", "Bajo");
        ScanPath(items, "Prefetch de Windows",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"),
            "Caché", "Medio");
        ScanPath(items, "Caché de Windows",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Caches"),
            "Caché", "Bajo");

        var recycleBin = @"C:\$Recycle.Bin";
        if (Directory.Exists(recycleBin))
        {
            try
            {
                long size = 0;
                var dirs = Directory.GetDirectories(recycleBin);
                foreach (var dir in dirs)
                {
                    try
                    {
                        var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            try { size += new FileInfo(file).Length; } catch { }
                        }
                    }
                    catch { }
                }
                if (size > 0)
                    items.Add(new CleanupItem
                    {
                        Name = "Papelera de reciclaje",
                        Size = size,
                        FileCount = 0,
                        Risk = "Bajo",
                        Category = "Sistema",
                        CleanAction = CleanRecycleBin
                    });
            }
            catch { }
        }

        return items;
    }

    private void ScanPath(List<CleanupItem> items, string name, string path, string category, string risk)
    {
        if (!Directory.Exists(path)) return;
        try
        {
            long size = 0;
            int count = 0;
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try { size += new FileInfo(file).Length; count++; }
                catch { }
            }
            if (count > 0)
                items.Add(new CleanupItem
                {
                    Name = name,
                    Size = size,
                    FileCount = count,
                    Risk = risk,
                    Category = category,
                    CleanPath = path
                });
        }
        catch { }
    }

    public long CalculateTotalSize(List<CleanupItem> items)
    {
        return items.Sum(i => i.Size);
    }

    public bool CleanItems(List<CleanupItem> items)
    {
        bool allOk = true;
        foreach (var item in items)
        {
            try
            {
                if (item.CleanAction != null)
                {
                    item.CleanAction();
                }
                else if (!string.IsNullOrEmpty(item.CleanPath))
                {
                    CleanDirectory(item.CleanPath);
                }
            }
            catch
            {
                allOk = false;
            }
        }
        return allOk;
    }

    private void CleanDirectory(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.GetFiles(path))
        {
            try { File.Delete(file); } catch { }
        }
        foreach (var dir in Directory.GetDirectories(path))
        {
            try { Directory.Delete(dir, true); } catch { }
        }
    }

    private void CleanRecycleBin()
    {
        try
        {
            System.Diagnostics.Process.Start("cmd.exe", "/c rd /s /q C:\\$Recycle.bin")?.WaitForExit(5000);
        }
        catch { }
    }
}

public class CleanupItem
{
    public string Name { get; set; } = "";
    public long Size { get; set; }
    public string SizeFormatted => Size switch
    {
        >= 1073741824 => $"{Size / 1073741824.0:F2} GB",
        >= 1048576 => $"{Size / 1048576.0:F2} MB",
        >= 1024 => $"{Size / 1024.0:F2} KB",
        _ => $"{Size} B"
    };
    public int FileCount { get; set; }
    public string Risk { get; set; } = "Bajo";
    public string Category { get; set; } = "";
    public bool Selected { get; set; } = true;
    public string CleanPath { get; set; } = "";
    public Action? CleanAction { get; set; }
}
