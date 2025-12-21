using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using myshop.Helpers;
using myshop.Services;
using myshop.Views;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace myshop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    private readonly ApplicationDataContainer settings =
        ApplicationData.Current.LocalSettings;

    private readonly NavigationService _navigationService;

    public LoginViewModel()
    {
        // Get NavigationService from DI
        _navigationService = App.ServiceProvider?.GetService<NavigationService>()
            ?? throw new InvalidOperationException("NavigationService not found in DI container");
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;

        // Trim whitespace từ Username và Password
        var trimmedUsername = Username?.Trim() ?? string.Empty;
        var trimmedPassword = Password?.Trim() ?? string.Empty;

        // Validation
        if (string.IsNullOrWhiteSpace(trimmedUsername) ||
            string.IsNullOrWhiteSpace(trimmedPassword))
        {
            ErrorMessage = "Vui lòng nhập tên đăng nhập và mật khẩu";
            IsLoading = false;
            return;
        }

        // Kiểm tra xem đã có thông tin đăng nhập được lưu chưa
        var savedUsername = settings.Values["owner_username"]?.ToString()?.Trim();
        var encryptedStoredHash = settings.Values["owner_password_hash_enc"]?.ToString();

        // Nếu chưa có thông tin đăng nhập được lưu, setup lần đầu
        // Chỉ cho phép setup với username/password mặc định
        if (string.IsNullOrEmpty(encryptedStoredHash))
        {
            // Setup lần đầu: chỉ chấp nhận username/password mặc định
            // Có thể thay đổi sau khi đăng nhập thành công
            const string defaultUsername = "admin";
            const string defaultPassword = "admin123"; // Password mặc định cho lần đầu setup

            if (!trimmedUsername.Equals(defaultUsername, StringComparison.OrdinalIgnoreCase) || 
                trimmedPassword != defaultPassword)
            {
                ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng. Lần đầu tiên sử dụng, vui lòng dùng:\nUsername: admin\nPassword: admin123";
                IsLoading = false;
                return;
            }

            // Lưu thông tin đăng nhập đã mã hóa
            var hash = PasswordHasher.Hash(trimmedPassword);
            var encryptedHash = await EncryptionHelper.EncryptAsync(hash);

            settings.Values["owner_username"] = trimmedUsername;
            settings.Values["owner_password_hash_enc"] = encryptedHash;
            settings.Values["is_logged_in"] = true;

            IsLoading = false;
            NavigateToMainWindow();
            return;
        }

        // Verify với thông tin đã lưu
        var storedHash = await EncryptionHelper.DecryptAsync(encryptedStoredHash);

        if (!trimmedUsername.Equals(savedUsername, StringComparison.OrdinalIgnoreCase) ||
            !PasswordHasher.Verify(trimmedPassword, storedHash))
        {
            ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng";
            IsLoading = false;
            return;
        }

        // Đăng nhập thành công
        settings.Values["is_logged_in"] = true;
        IsLoading = false;
        NavigateToMainWindow();
    }

    [RelayCommand]
    private void NavigateToConfig()
    {
        _navigationService.NavigateTo(typeof(ConfigPage));
    }

    /// <summary>
    /// Gọi từ LoginPage khi page đã loaded để tránh lỗi COMException
    /// </summary>
    public async void TryAutoLogin()
    {
        if (settings.Values.TryGetValue("is_logged_in", out var logged) &&
            logged is bool isLogged && isLogged)
        {
            // Delay một chút để đảm bảo window đã sẵn sàng
            await Task.Delay(300);
            NavigateToMainWindow();
        }
    }

    private void NavigateToMainWindow()
    {
        // Get current LoginWindow
        var loginWindow = WindowHelper.GetWindow<LoginWindow>();
        
        if (loginWindow != null)
        {
            // Create and activate MainWindow
            var mainWindow = new MainWindow();
            mainWindow.Activate();
            
            // Close LoginWindow
            loginWindow.Close();
        }
    }
}
