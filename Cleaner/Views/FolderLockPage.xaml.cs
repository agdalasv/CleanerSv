using System.Windows;
using System.Windows.Controls;

namespace Cleaner.Views;

public partial class FolderLockPage : Page
{
    private readonly ViewModels.FolderLockViewModel _vm;

    public FolderLockPage()
    {
        InitializeComponent();
        _vm = new ViewModels.FolderLockViewModel();
        DataContext = _vm;
        Unloaded += (_, _) =>
        {
            PasswordBox.Password = "";
            ConfirmPasswordBox.Password = "";
            UnlockPasswordBox.Password = "";
        };
    }

    private void PasswordBox_Changed(object sender, RoutedEventArgs e)
    {
        _vm.Password = PasswordBox.Password;
    }

    private void ConfirmPasswordBox_Changed(object sender, RoutedEventArgs e)
    {
        _vm.ConfirmPassword = ConfirmPasswordBox.Password;
    }

    private void UnlockPasswordBox_Changed(object sender, RoutedEventArgs e)
    {
        _vm.Password = UnlockPasswordBox.Password;
    }
}
