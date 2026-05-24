using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Cleaner.Services;

namespace Cleaner.ViewModels;

public class DiskCleanupViewModel : BaseViewModel
{
    private readonly DiskCleanupService _service;
    private bool _isScanning;
    private bool _isCleaning;
    private string _totalSize = "0 MB";
    private string _statusMessage = "Listo para escanear";

    public DiskCleanupViewModel()
    {
        _service = new DiskCleanupService();
        Items = new ObservableCollection<CleanupItem>();
        ScanCommand = new RelayCommand(_ => Scan());
        CleanCommand = new RelayCommand(_ => Clean(), _ => Items.Any(i => i.Selected));
    }

    public ObservableCollection<CleanupItem> Items { get; }

    public bool IsScanning
    {
        get => _isScanning;
        set => SetProperty(ref _isScanning, value);
    }
    public bool IsCleaning
    {
        get => _isCleaning;
        set => SetProperty(ref _isCleaning, value);
    }
    public string TotalSize
    {
        get => _totalSize;
        set => SetProperty(ref _totalSize, value);
    }
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand ScanCommand { get; }
    public ICommand CleanCommand { get; }

    private async void Scan()
    {
        IsScanning = true;
        StatusMessage = "Escaneando sistema...";
        Items.Clear();

        await Task.Run(() =>
        {
            var allItems = _service.ScanAll();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in allItems)
                    Items.Add(item);

                var total = _service.CalculateTotalSize(allItems);
                TotalSize = FormatSize(total);
                StatusMessage = $"Escaneo completado: {Items.Count} elementos encontrados";
                IsScanning = false;
            });
        });
    }

    private async void Clean()
    {
        IsCleaning = true;
        StatusMessage = "Limpiando archivos...";

        var selected = Items.Where(i => i.Selected).ToList();
        await Task.Run(() => _service.CleanItems(selected));

        Items.Clear();
        TotalSize = "0 MB";
        IsCleaning = false;
        StatusMessage = "Limpieza completada exitosamente";
    }

    private static string FormatSize(long bytes)
    {
        return bytes switch
        {
            >= 1073741824 => $"{bytes / 1073741824.0:F2} GB",
            >= 1048576 => $"{bytes / 1048576.0:F2} MB",
            >= 1024 => $"{bytes / 1024.0:F2} KB",
            _ => $"{bytes} B"
        };
    }
}
