using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Cleaner.Services;

namespace Cleaner.ViewModels;

public class RegistryViewModel : BaseViewModel
{
    private readonly RegistryService _service;
    private bool _isScanning;
    private bool _isRepairing;
    private string _statusMessage = "Listo para escanear el registro";
    private int _repairedCount;

    public RegistryViewModel()
    {
        _service = new RegistryService();
        Issues = new ObservableCollection<RegistryIssue>();
        ScanCommand = new RelayCommand(_ => Scan());
        RepairCommand = new RelayCommand(_ => Repair(), _ => Issues.Any(i => i.Selected));
        BackupCommand = new RelayCommand(_ => Backup());
    }

    public ObservableCollection<RegistryIssue> Issues { get; }
    public bool IsScanning { get => _isScanning; set => SetProperty(ref _isScanning, value); }
    public bool IsRepairing { get => _isRepairing; set => SetProperty(ref _isRepairing, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public int RepairedCount { get => _repairedCount; set => SetProperty(ref _repairedCount, value); }
    public ICommand ScanCommand { get; }
    public ICommand RepairCommand { get; }
    public ICommand BackupCommand { get; }

    private async void Scan()
    {
        IsScanning = true;
        StatusMessage = "Escaneando registro...";
        Issues.Clear();

        await Task.Run(() =>
        {
            var found = _service.ScanIssues();
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var issue in found)
                    Issues.Add(issue);
                StatusMessage = $"Escaneo completado: {found.Count} problemas encontrados";
                IsScanning = false;
            });
        });
    }

    private async void Repair()
    {
        var selected = Issues.Where(i => i.Selected).ToList();
        if (selected.Count == 0) return;

        IsRepairing = true;
        StatusMessage = "Creando respaldo antes de reparar...";
        _service.BackupRegistry(@"SOFTWARE\Microsoft\Windows\CurrentVersion");

        var fixedList = new List<RegistryIssue>();
        await Task.Run(() =>
        {
            foreach (var issue in selected)
            {
                if (_service.FixIssue(issue))
                    fixedList.Add(issue);
            }
        });

        RepairedCount += fixedList.Count;
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (var item in fixedList)
                Issues.Remove(item);
            StatusMessage = $"Reparación completada: {fixedList.Count} problemas corregidos de {selected.Count} seleccionados";
            IsRepairing = false;
        });
    }

    private void Backup()
    {
        StatusMessage = "Creando respaldo completo del registro...";
        if (_service.BackupRegistry(""))
            StatusMessage = "Respaldo del registro creado en Documentos/Cleaner Backups";
        else
            StatusMessage = "Error al crear respaldo";
    }
}
