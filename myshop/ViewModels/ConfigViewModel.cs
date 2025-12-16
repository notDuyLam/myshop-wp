using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using myshop.Helpers;
using myshop.Services;
using System.Threading.Tasks;

namespace myshop.ViewModels;

public partial class ConfigViewModel : ObservableObject
{
    private readonly DatabaseConfigService _configService;
    private readonly DatabaseTestService _testService;

    public ConfigViewModel(DatabaseConfigService configService, DatabaseTestService testService)
    {
        _configService = configService;
        _testService = testService;

        _ = LoadConfigAsync();
    }

    // =========================
    // PROPERTIES
    // =========================

    [ObservableProperty]
    private string serverHost;

    [ObservableProperty]
    private int port;

    [ObservableProperty]
    private string databaseName;

    [ObservableProperty]
    private string username;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage;

    [ObservableProperty]
    private string successMessage;

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

        await _configService.SaveConfigAsync(config);

        IsLoading = false;
        SuccessMessage = "Configuration saved successfully.";
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        ClearMessages();

        if (!Validate())
            return;

        IsLoading = true;

        // ⚠️ Test phải dùng PASSWORD THẬT → không encrypt
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
            ErrorMessage = error ?? "Cannot connect to database.";
        }
        else
        {
            SuccessMessage = "Connection successful.";
        }
    }

    // =========================
    // PRIVATE METHODS
    // =========================

    private async Task LoadConfigAsync()
    {
        var config = await _configService.LoadConfigAsync();

        if (config == null)
            return;

        ServerHost = config.Host;
        Port = config.Port;
        DatabaseName = config.Database;
        Username = config.Username;

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
            ErrorMessage = "Please fill all required fields.";
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
