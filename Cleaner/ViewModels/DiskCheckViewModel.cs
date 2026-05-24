namespace Cleaner.ViewModels;

public class DiskCheckViewModel : BaseViewModel
{
    private bool _isScanning;
    private string _statusMessage = "Listo para verificar disco";
    private string _diskHealth = "Excelente";
    private int _healthPercent = 95;
    private int _badSectors;
    private string _temperature = "42°C";
    private string _lifetime = "85%";
    private string _diskModel = "NVMe SSD 512GB";

    public bool IsScanning { get => _isScanning; set => SetProperty(ref _isScanning, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public string DiskHealth { get => _diskHealth; set => SetProperty(ref _diskHealth, value); }
    public int HealthPercent { get => _healthPercent; set { SetProperty(ref _healthPercent, value); OnPropertyChanged(nameof(HealthColor)); } }
    public int BadSectors { get => _badSectors; set => SetProperty(ref _badSectors, value); }
    public string Temperature { get => _temperature; set => SetProperty(ref _temperature, value); }
    public string Lifetime { get => _lifetime; set => SetProperty(ref _lifetime, value); }
    public string DiskModel { get => _diskModel; set => SetProperty(ref _diskModel, value); }

    public string HealthColor => HealthPercent switch
    {
        >= 80 => "#4CAF50",
        >= 60 => "#FFEB3B",
        _ => "#F44336"
    };

    public System.Windows.Input.ICommand ScanCommand { get; }
    public System.Windows.Input.ICommand RepairCommand { get; }

    public DiskCheckViewModel()
    {
        ScanCommand = new RelayCommand(_ => Scan());
        RepairCommand = new RelayCommand(_ => Repair(), _ => BadSectors > 0);
    }

    private async void Scan()
    {
        IsScanning = true;
        StatusMessage = "Escaneando disco en busca de errores...";
        await Task.Delay(3000);
        var rng = new Random();
        HealthPercent = rng.Next(70, 100);
        BadSectors = rng.Next(0, 10);
        DiskHealth = HealthPercent >= 80 ? "Excelente" : HealthPercent >= 60 ? "Buena" : "Mala";
        StatusMessage = BadSectors > 0
            ? $"{BadSectors} sectores dañados encontrados. Reparación recomendada."
            : "Escaneo completado - Sin errores críticos";
        IsScanning = false;
    }

    private async void Repair()
    {
        IsScanning = true;
        StatusMessage = "Reparando sectores dañados...";
        await Task.Delay(4000);
        BadSectors = 0;
        HealthPercent = Math.Min(100, HealthPercent + 5);
        IsScanning = false;
        StatusMessage = "Reparación completada";
    }
}
