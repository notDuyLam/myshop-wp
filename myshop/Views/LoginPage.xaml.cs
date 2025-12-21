using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using myshop.ViewModels;
using Windows.ApplicationModel;

namespace myshop.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginPage()
    {
        InitializeComponent();
        
        // Set DataContext from DI
        ViewModel = App.ServiceProvider?.GetService<LoginViewModel>()
            ?? throw new InvalidOperationException("LoginViewModel not found in DI container");
        
        DataContext = ViewModel;
        
        // Set version info
        var version = Package.Current.Id.Version;
        VersionTextBlock.Text = $"Version {version.Major}.{version.Minor}.{version.Build}";
        
        // Sync PasswordBox với ViewModel
        PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        
        // Try auto-login sau khi page đã loaded
        Loaded += LoginPage_Loaded;
    }

    private void LoginPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Gọi auto-login sau khi page đã loaded để tránh lỗi COMException
        ViewModel.TryAutoLogin();
    }

    private void PasswordBox_PasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            ViewModel.Password = passwordBox.Password;
        }
    }
}

