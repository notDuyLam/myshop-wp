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
    private string username;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string errorMessage;

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
        _ = AutoLoginAsync();
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;

        // Validation
        if (string.IsNullOrWhiteSpace(Username) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required";
            IsLoading = false;
            return;
        }

        // Lưu thông tin đăng nhập cho lần đầu tiên (có thể coi là tương đương với register)
        if (!settings.Values.ContainsKey("owner_password_hash_enc"))
        {
            var hash = PasswordHasher.Hash(Password);
            var encryptedHash = await EncryptionHelper.EncryptAsync(hash);

            settings.Values["owner_username"] = Username;
            settings.Values["owner_password_hash_enc"] = encryptedHash;
            settings.Values["is_logged_in"] = true;

            IsLoading = false;
            NavigateToDashboard();
            return;
        }

        var savedUsername = settings.Values["owner_username"]?.ToString();
        var encryptedStoredHash =
            settings.Values["owner_password_hash_enc"]?.ToString();

        var storedHash =
            await EncryptionHelper.DecryptAsync(encryptedStoredHash);

        if (Username != savedUsername ||
            !PasswordHasher.Verify(Password, storedHash))
        {
            ErrorMessage = "Invalid username or password";
            IsLoading = false;
            return;
        }

        settings.Values["is_logged_in"] = true;
        IsLoading = false;
        NavigateToDashboard();
    }

    private async Task AutoLoginAsync()
    {
        if (settings.Values.TryGetValue("is_logged_in", out var logged) &&
            logged is bool isLogged && isLogged)
        {
            NavigateToDashboard();
        }
    }

    private void NavigateToDashboard()
    {
        _navigationService.NavigateTo(typeof(DashboardPage));
    }
}
