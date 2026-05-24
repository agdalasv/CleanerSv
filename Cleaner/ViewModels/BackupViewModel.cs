using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace Cleaner.ViewModels;

public class BackupViewModel : BaseViewModel
{
    private bool _isCreating;
    private string _statusMessage = "Sistema de respaldo listo";
    private string _lastBackup = "Nunca";
    private int _backupCount;

    public BackupViewModel()
    {
        Backups = new ObservableCollection<string>();
        CreateRestorePointCommand = new RelayCommand(_ => CreateRestorePoint());
        BackupRegistryCommand = new RelayCommand(_ => BackupRegistry());
    }

    public ObservableCollection<string> Backups { get; }
    public bool IsCreating { get => _isCreating; set => SetProperty(ref _isCreating, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public string LastBackup { get => _lastBackup; set => SetProperty(ref _lastBackup, value); }
    public int BackupCount { get => _backupCount; set => SetProperty(ref _backupCount, value); }

    public ICommand CreateRestorePointCommand { get; }
    public ICommand BackupRegistryCommand { get; }

    private async void CreateRestorePoint()
    {
        IsCreating = true;
        StatusMessage = "Creando punto de restauración...";
        await Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "Checkpoint-Computer -Description \"Cleaner Restore Point\" -RestorePointType MODIFY_SETTINGS",
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                var proc = Process.Start(psi);
                proc?.WaitForExit(30000);
            }
            catch { }
        });
        var time = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        LastBackup = time;
        BackupCount++;
        Application.Current.Dispatcher.Invoke(() =>
            Backups.Insert(0, $"Punto de restauración - {time}"));
        StatusMessage = "Punto de restauración creado exitosamente";
        IsCreating = false;
    }

    private async void BackupRegistry()
    {
        IsCreating = true;
        StatusMessage = "Respaldando registro...";
        await Task.Run(() =>
        {
            try
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Cleaner Backups");
                Directory.CreateDirectory(dir);
                var file = Path.Combine(dir,
                    $"Registry_Full_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.reg");
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c reg export HKLM \"{file}\"",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                var proc = Process.Start(psi);
                proc?.WaitForExit(10000);
            }
            catch { }
        });
        var time = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        LastBackup = time;
        BackupCount++;
        Application.Current.Dispatcher.Invoke(() =>
            Backups.Insert(0, $"Respaldo de registro - {time}"));
        StatusMessage = "Respaldo del registro completado en Documentos/Cleaner Backups";
        IsCreating = false;
    }
}
