using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;

namespace Cleaner.ViewModels;

public class FolderLockViewModel : BaseViewModel
{
    private string _password = "";
    private string _confirmPassword = "";
    private string _statusMessage = "Protege una carpeta con contrase\u00f1a";
    private bool _isLocked;
    private bool _isUnlocked;
    private string _vaultPath = "";
    private string _lockFilePath = "";

    public FolderLockViewModel()
    {
        _vaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "CleanerVault");
        _lockFilePath = Path.Combine(_vaultPath, ".lock");

        Directory.CreateDirectory(_vaultPath);
        SetHiddenAttribute(_vaultPath, true);

        _isLocked = File.Exists(_lockFilePath);
        _isUnlocked = !_isLocked;
        UpdateStatus();

        SetPasswordCommand = new RelayCommand(_ => SetPassword(), _ => CanSetPassword());
        UnlockCommand = new RelayCommand(_ => Unlock(), _ => CanUnlock());
        LockCommand = new RelayCommand(_ => Lock(), _ => _isUnlocked && File.Exists(_lockFilePath));
        OpenFolderCommand = new RelayCommand(_ => OpenFolder());
    }

    public string Password
    {
        get => _password;
        set { SetProperty(ref _password, value); CommandManager.InvalidateRequerySuggested(); }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set { SetProperty(ref _confirmPassword, value); CommandManager.InvalidateRequerySuggested(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsLocked
    {
        get => _isLocked;
        set { SetProperty(ref _isLocked, value); OnPropertyChanged(nameof(IsUnlocked)); }
    }

    public bool IsUnlocked
    {
        get => _isUnlocked;
        set { SetProperty(ref _isUnlocked, value); OnPropertyChanged(nameof(IsLocked)); }
    }

    public string VaultPath => _vaultPath;

    public ICommand SetPasswordCommand { get; }
    public ICommand UnlockCommand { get; }
    public ICommand LockCommand { get; }
    public ICommand OpenFolderCommand { get; }

    private bool CanSetPassword()
    {
        if (string.IsNullOrEmpty(_password) || _password.Length < 6) return false;
        if (_password != _confirmPassword) return false;
        return !_isLocked && _isUnlocked;
    }

    private bool CanUnlock()
    {
        if (string.IsNullOrEmpty(_password)) return false;
        return _isLocked;
    }

    private void SetPassword()
    {
        if (!CanSetPassword()) return;
        var hash = HashPassword(_password);
        File.WriteAllText(_lockFilePath, hash);
        SetHiddenAttribute(_vaultPath, true);
        Password = "";
        ConfirmPassword = "";
        Lock();
        StatusMessage = "Carpeta protegida y bloqueada con \u00e9xito";
    }

    private void Unlock()
    {
        if (!CanUnlock()) return;
        if (!File.Exists(_lockFilePath))
        {
            StatusMessage = "No hay una contrase\u00f1a configurada";
            return;
        }
        var storedHash = File.ReadAllText(_lockFilePath).Trim();
        var inputHash = HashPassword(_password);
        if (storedHash != inputHash)
        {
            StatusMessage = "Contrase\u00f1a incorrecta";
            return;
        }
        SetHiddenAttribute(_vaultPath, false);
        IsLocked = false;
        IsUnlocked = true;
        Password = "";
        StatusMessage = "Carpeta desbloqueada. Puedes acceder a tus archivos";
    }

    private void Lock()
    {
        SetHiddenAttribute(_vaultPath, true);
        IsLocked = true;
        IsUnlocked = false;
        Password = "";
        StatusMessage = "Carpeta bloqueada y oculta";
    }

    private void OpenFolder()
    {
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", _vaultPath);
        }
        catch { }
    }

    private void UpdateStatus()
    {
        if (_isLocked)
            StatusMessage = "Carpeta bloqueada. Ingresa la contrase\u00f1a para desbloquear";
        else
            StatusMessage = "Carpeta desbloqueada. Puedes configurar o cambiar la contrase\u00f1a";
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static void SetHiddenAttribute(string path, bool hidden)
    {
        try
        {
            var dir = new DirectoryInfo(path);
            if (dir.Exists)
                dir.Attributes = hidden
                    ? FileAttributes.Directory | FileAttributes.Hidden | FileAttributes.System
                    : FileAttributes.Directory;
        }
        catch { }
    }
}
