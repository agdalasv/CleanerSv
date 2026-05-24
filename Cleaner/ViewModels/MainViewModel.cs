using System.Windows.Input;
using Cleaner.Services;
using Cleaner.Views;

namespace Cleaner.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly ThemeService _themeService;
    private object? _currentPage;
    private int _selectedIndex;
    private string _themeIcon = "🌙";

    public MainViewModel(ThemeService themeService)
    {
        _themeService = themeService;
        _themeService.ApplyTheme(ThemeMode.Dark);
        NavigateCommand = new RelayCommand(param => Navigate(param?.ToString() ?? ""));
        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        Navigate("Dashboard");
    }

    public object? CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            SetProperty(ref _selectedIndex, value);
            OnPropertyChanged(nameof(IsDarkTheme));
        }
    }

    public string ThemeIcon
    {
        get => _themeIcon;
        set => SetProperty(ref _themeIcon, value);
    }

    public bool IsDarkTheme => _themeService.CurrentTheme == ThemeMode.Dark;

    public ICommand NavigateCommand { get; }
    public ICommand ToggleThemeCommand { get; }

    private void Navigate(string page)
    {
        var sysInfo = new SystemInfoService();
        switch (page)
        {
            case "Dashboard":
                CurrentPage = new DashboardPage { DataContext = new DashboardViewModel(this, sysInfo) };
                break;
            case "DiskCleanup":
                CurrentPage = new DiskCleanupPage { DataContext = new DiskCleanupViewModel() };
                break;
            case "Registry":
                CurrentPage = new RegistryPage { DataContext = new RegistryViewModel() };
                break;
            case "RamOptimizer":
                CurrentPage = new RamOptimizerPage { DataContext = new RamOptimizerViewModel() };
                break;
            case "DiskDefrag":
                CurrentPage = new DiskDefragPage { DataContext = new DiskDefragViewModel() };
                break;
            case "DiskCheck":
                CurrentPage = new DiskCheckPage { DataContext = new DiskCheckViewModel() };
                break;
            case "SystemOptimizer":
                CurrentPage = new SystemOptimizerPage { DataContext = new SystemOptimizerViewModel() };
                break;
            case "SystemMonitor":
                CurrentPage = new SystemMonitorPage { DataContext = new SystemMonitorViewModel(sysInfo) };
                break;
            case "AppManager":
                CurrentPage = new AppManagerPage { DataContext = new AppManagerViewModel() };
                break;
            case "Backup":
                CurrentPage = new BackupPage { DataContext = new BackupViewModel() };
                break;
            case "FolderLock":
                CurrentPage = new FolderLockPage();
                break;
        }
    }

    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
        ThemeIcon = _themeService.CurrentTheme == ThemeMode.Dark ? "🌙" : "☀️";
        OnPropertyChanged(nameof(IsDarkTheme));
    }
}
