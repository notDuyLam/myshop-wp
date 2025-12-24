using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using myshop.Helpers;
using myshop.Services;
using System;
using System.Threading.Tasks;

namespace myshop.ViewModels;

public partial class ConfigViewModel : ObservableObject
{
    private readonly DatabaseConfigService _configService;
    private readonly DatabaseTestService _testService;
    private readonly NavigationService _navigationService;
    private readonly DbContextService _dbContextService;

    public ConfigViewModel(
        DatabaseConfigService configService,
        DatabaseTestService testService,
        NavigationService navigationService,
   DbContextService dbContextService)
    {
        _configService = configService;
        _testService = testService;
        _navigationService = navigationService;
        _dbContextService = dbContextService;

        _ = LoadConfigAsync();
    }

    // =========================
    // PROPERTIES
    // =========================

    [ObservableProperty]
    private string serverHost = string.Empty;

    [ObservableProperty]
    private int port;

    [ObservableProperty]
    private string databaseName = string.Empty;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string successMessage = string.Empty;

    // =========================
    // COMMANDS
    // =========================

    [RelayCommand]
    private async Task SaveConfigAsync()
    {
        ClearMessages();

        if (!Validate())
            return;

        IsLoading = true;

        var encryptedPassword = await EncryptionHelper.EncryptAsync(Password);

        var config = new DatabaseConfig
        {
            Host = ServerHost,
            Port = Port,
            Database = DatabaseName,
            Username = Username,
            Password = encryptedPassword
        };

        var testConfig = new DatabaseConfig
        {
            Host = ServerHost,
            Port = Port,
            Database = DatabaseName,
            Username = Username,
            Password = Password
        };


        //Migration khi lưu cấu hình mới, dev tự chạy update migration nếu cần
        // Test kết nối trước khi migrate
        var (success, error) = await _testService.TestConnectionAsync(testConfig.ToConnectionString());
        if (!success)
        {
            IsLoading = false;
            ErrorMessage = $"Kết nối database thất bại: {testConfig.ToConnectionString()}";
            return;
        }

        try
        {
            await _configService.SaveConfigAsync(config);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"SaveConfigAsync lỗi: {ex.Message}";
            IsLoading = false;
            return;
        }

        // XÓA CACHE connection string để lần sau sẽ đọc cấu hình mới
        _dbContextService.RefreshConnectionString();

        // Chạy migration nếu kết nối thành công
        try
        {
            await _dbContextService.MigrateAndSeedAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Migration lỗi: {ex.Message}";
            IsLoading = false;
            return;
        }

        IsLoading = false;
        var filePath = await _configService.GetConfigFilePathAsync();
        SuccessMessage = $"Đã lưu cấu hình thành công!\nFile: {filePath}\n\nCấu hình database đã được cập nhật.";
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        ClearMessages();

        if (!Validate())
            return;

        IsLoading = true;

        //  Test phải dùng PASSWORD THẬT → không encrypt
        var testConfig = new DatabaseConfig
        {
            Host = ServerHost,
            Port = Port,
            Database = DatabaseName,
            Username = Username,
            Password = Password
        };

        var (success, error) =
     await _testService.TestConnectionAsync(testConfig.ToConnectionString());

        IsLoading = false;

        if (!success)
        {
            ErrorMessage = error ?? "Không thể kết nối đến database.";
        }
        else
        {
            SuccessMessage = "Kết nối thành công!";
        }
    }

    [RelayCommand]
    private async Task OpenConfigFileAsync()
    {
        try
        {
            var filePath = await _configService.GetConfigFilePathAsync();
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath);
            await Windows.System.Launcher.LaunchFileAsync(file);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể mở file: {ex.Message}";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }

    // =========================
    // PRIVATE METHODS
    // =========================

    private async Task LoadConfigAsync()
    {
        var config = await _configService.LoadConfigAsync();

        if (config == null)
        {
            // Set default values
            ServerHost = "localhost";
            Port = 5432;
            DatabaseName = "myshop_db";
            Username = "postgres";
            return;
        }

        ServerHost = config.Host ?? "localhost";
        Port = config.Port > 0 ? config.Port : 5432;
        DatabaseName = config.Database ?? "myshop_db";
        Username = config.Username ?? "postgres";

        if (!string.IsNullOrEmpty(config.Password))
        {
            Password = await EncryptionHelper.DecryptAsync(config.Password);
        }
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(ServerHost) ||
       string.IsNullOrWhiteSpace(DatabaseName) ||
    string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Vui lòng điền đầy đủ các trường bắt buộc.";
            return false;
        }

        if (Port < 1 || Port > 65535)
        {
            ErrorMessage = "Port phải nằm trong khoảng 1-65535.";
            return false;
        }

        return true;
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }
}
