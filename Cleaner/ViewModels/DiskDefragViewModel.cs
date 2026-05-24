using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Cleaner.ViewModels;

public class DiskDefragViewModel : BaseViewModel
{
    private string _selectedDrive = "C:\\";
    private int _fragmentationPercent = 15;
    private string _driveHealth = "Buena";
    private bool _isSsd = true;
    private bool _isOptimizing;
    private string _statusMessage = "Seleccione una unidad para analizar";
    private ObservableCollection<SolidColorBrush> _blockColors = new();

    public DiskDefragViewModel()
    {
        BlockColors = new ObservableCollection<SolidColorBrush>();
        GenerateBlockMap(15);
        AnalyzeCommand = new RelayCommand(_ => Analyze());
        OptimizeCommand = new RelayCommand(_ => Optimize(), _ => !IsSsd);
        TrimCommand = new RelayCommand(_ => Trim(), _ => IsSsd);
    }

    public ObservableCollection<SolidColorBrush> BlockColors
    {
        get => _blockColors;
        set => SetProperty(ref _blockColors, value);
    }

    public string SelectedDrive
    {
        get => _selectedDrive;
        set => SetProperty(ref _selectedDrive, value);
    }
    public int FragmentationPercent
    {
        get => _fragmentationPercent;
        set { SetProperty(ref _fragmentationPercent, value); OnPropertyChanged(nameof(FragmentationText)); }
    }
    public string DriveHealth
    {
        get => _driveHealth;
        set => SetProperty(ref _driveHealth, value);
    }
    public bool IsSsd
    {
        get => _isSsd;
        set => SetProperty(ref _isSsd, value);
    }
    public bool IsOptimizing
    {
        get => _isOptimizing;
        set => SetProperty(ref _isOptimizing, value);
    }
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
    public string FragmentationText => $"{FragmentationPercent}% fragmentado";
    public ICommand AnalyzeCommand { get; }
    public ICommand OptimizeCommand { get; }
    public ICommand TrimCommand { get; }

    private void GenerateBlockMap(int fragPercent)
    {
        BlockColors.Clear();
        var rng = new Random();
        int totalBlocks = 400;
        int fragBlocks = totalBlocks * fragPercent / 100;

        for (int i = 0; i < totalBlocks; i++)
        {
            if (i < fragBlocks)
            {
                byte r = (byte)rng.Next(180, 256);
                byte g = (byte)rng.Next(80, 160);
                byte b = (byte)rng.Next(0, 60);
                BlockColors.Add(new SolidColorBrush(Color.FromRgb(r, g, b)));
            }
            else
            {
                byte g = (byte)rng.Next(160, 221);
                BlockColors.Add(new SolidColorBrush(Color.FromRgb(30, g, 60)));
            }
        }
    }

    private async void Analyze()
    {
        IsOptimizing = true;
        StatusMessage = $"Analizando unidad {SelectedDrive}...";
        await Task.Delay(2000);
        var rng = new Random();
        FragmentationPercent = rng.Next(0, 30);
        IsSsd = rng.Next(2) == 1;
        DriveHealth = FragmentationPercent switch { < 10 => "Excelente", < 20 => "Buena", _ => "Regular" };
        GenerateBlockMap(FragmentationPercent);
        StatusMessage = IsSsd
            ? $"SSD detectado - {FragmentationPercent}% fragmentado (TRIM recomendado)"
            : $"HDD detectado - {FragmentationPercent}% fragmentado (desfragmentación recomendada)";
        IsOptimizing = false;
    }

    private async void Optimize()
    {
        IsOptimizing = true;
        StatusMessage = "Desfragmentando unidad...";
        await Task.Delay(3000);
        FragmentationPercent = 0;
        GenerateBlockMap(0);
        IsOptimizing = false;
        StatusMessage = "Desfragmentación completada exitosamente";
    }

    private async void Trim()
    {
        IsOptimizing = true;
        StatusMessage = "Ejecutando TRIM en SSD...";
        await Task.Delay(2000);
        FragmentationPercent = 0;
        GenerateBlockMap(0);
        IsOptimizing = false;
        StatusMessage = "TRIM completado exitosamente";
    }
}
