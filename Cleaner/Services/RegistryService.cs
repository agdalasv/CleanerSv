using System.IO;
using Microsoft.Win32;

namespace Cleaner.Services;

public class RegistryService
{
    public List<RegistryIssue> ScanIssues()
    {
        var issues = new List<RegistryIssue>();
        ScanUninstallEntries(issues, RegistryView.Default);
        ScanUninstallEntries(issues, RegistryView.Registry32);
        ScanRunEntries(issues);
        ScanExtensions(issues);
        return issues;
    }

    private void ScanUninstallEntries(List<RegistryIssue> issues, RegistryView view)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
            using var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key == null) return;
            foreach (var subName in key.GetSubKeyNames())
            {
                try
                {
                    using var subKey = key.OpenSubKey(subName);
                    if (subKey == null) continue;
                    var displayName = subKey.GetValue("DisplayName")?.ToString();
                    if (string.IsNullOrEmpty(displayName)) continue;
                    var installLocation = subKey.GetValue("InstallLocation")?.ToString();
                    if (!string.IsNullOrEmpty(installLocation) && !Directory.Exists(installLocation))
                    {
                        issues.Add(new RegistryIssue
                        {
                            Name = $"Entrada huérfana: {displayName}",
                            KeyPath = @"HKLM\SOFTWARE\" + (view == RegistryView.Registry32 ? "WOW6432Node\\" : "") + @"Microsoft\Windows\CurrentVersion\Uninstall\" + subName,
                            Severity = "Media",
                            Type = "Entrada inválida",
                            SubKey = subName,
                            Hive = RegistryHive.LocalMachine,
                            View = view
                        });
                    }
                }
                catch { }
            }
        }
        catch { }
    }

    private void ScanRunEntries(List<RegistryIssue> issues)
    {
        string[] roots = { @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" };
        foreach (var root in roots)
        {
            try
            {
                var hive = root.StartsWith("HKLM") ? RegistryHive.LocalMachine : RegistryHive.CurrentUser;
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                using var key = baseKey.OpenSubKey(root.Substring(root.IndexOf('\\') + 1));
                if (key == null) continue;
                foreach (var valueName in key.GetValueNames())
                {
                    try
                    {
                        var path = key.GetValue(valueName)?.ToString();
                        if (string.IsNullOrEmpty(path)) continue;
                        var exePath = path.Trim('"');
                        if (exePath.Contains(".exe") && !File.Exists(Environment.ExpandEnvironmentVariables(exePath)))
                        {
                            issues.Add(new RegistryIssue
                            {
                                Name = $"Referencia muerta: {valueName}",
                                KeyPath = root + "\\" + valueName,
                                Severity = "Baja",
                                Type = "Ruta inexistente",
                                ValueName = valueName,
                                Hive = hive,
                                RegRoot = root.Substring(root.IndexOf('\\') + 1)
                            });
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
    }

    private void ScanExtensions(List<RegistryIssue> issues)
    {
        try
        {
            using var key = Registry.ClassesRoot;
            foreach (var subName in key.GetSubKeyNames())
            {
                if (subName.StartsWith(".")) continue;
                try
                {
                    using var cmdKey = key.OpenSubKey($"{subName}\\shell\\open\\command");
                    if (cmdKey == null) continue;
                    var cmd = cmdKey.GetValue("")?.ToString();
                    if (string.IsNullOrEmpty(cmd)) continue;
                    var parts = cmd.Replace("\"", "").Split(',')[0].Trim().Split(' ');
                    var exe = parts.FirstOrDefault(p => p.EndsWith(".exe") || p.EndsWith(".dll"));
                    if (exe != null && !File.Exists(Environment.ExpandEnvironmentVariables(exe)) && exe.Contains("\\"))
                    {
                        issues.Add(new RegistryIssue
                        {
                            Name = $"Asociación rota: .{subName} → {Path.GetFileName(exe)}",
                            KeyPath = $@"HKEY_CLASSES_ROOT\{subName}\shell\open\command",
                            Severity = "Alta",
                            Type = "DLL faltante",
                            FileAssocExt = subName
                        });
                    }
                }
                catch { }
            }
        }
        catch { }
    }

    public bool FixIssue(RegistryIssue issue)
    {
        try
        {
            string args;
            switch (issue.Type)
            {
                case "Entrada inválida":
                    args = $"delete \"{issue.KeyPath}\" /f";
                    break;
                case "Ruta inexistente":
                    args = $"delete \"{issue.RegRoot}\" /v \"{issue.ValueName}\" /f";
                    break;
                case "DLL faltante":
                    args = $"delete \"HKEY_CLASSES_ROOT\\{issue.FileAssocExt}\" /f";
                    break;
                default:
                    return false;
            }
            var proc = System.Diagnostics.Process.Start("reg.exe", args);
            proc?.WaitForExit(3000);
            return proc?.ExitCode == 0;
        }
        catch { return false; }
    }

    public bool BackupRegistry(string path)
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Cleaner Backups");
            Directory.CreateDirectory(dir);
            var backupFile = Path.Combine(dir, $"Registry_Backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.reg");
            var proc = System.Diagnostics.Process.Start("cmd.exe", $"/c reg export \"{path}\" \"{backupFile}\"");
            proc?.WaitForExit(10000);
            return File.Exists(backupFile);
        }
        catch { return false; }
    }
}

public class RegistryIssue
{
    public string Name { get; set; } = "";
    public string KeyPath { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Type { get; set; } = "";
    public string SubKey { get; set; } = "";
    public string ValueName { get; set; } = "";
    public string FileAssocExt { get; set; } = "";
    public string RegRoot { get; set; } = "";
    public RegistryHive Hive { get; set; }
    public RegistryView View { get; set; } = RegistryView.Default;
    public bool Selected { get; set; } = true;
}
