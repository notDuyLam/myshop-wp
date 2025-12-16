using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using myshop.Services;
using Npgsql;
using Windows.System;
using Windows.Storage;

namespace myshop.Views;

public sealed partial class ConfigPage : Page
{
    private readonly DatabaseConfigService _configService;
    private readonly DatabaseTestService _testService;

    public ConfigPage()
    {
        InitializeComponent();
        
        // Get services from DI
        _configService = App.ServiceProvider?.GetService<DatabaseConfigService>() 
            ?? throw new InvalidOperationException("DatabaseConfigService not found");
        _testService = App.ServiceProvider?.GetService<DatabaseTestService>() 
            ?? throw new InvalidOperationException("DatabaseTestService not found");
        
        LoadSavedConfig();
    }

    /// <summary>
    /// Load cấu hình đã lưu vào các TextBox (hoặc dùng default nếu chưa có file)
    /// </summary>
    private async void LoadSavedConfig()
    {
        var config = await _configService.LoadConfigAsync();
        
        // Nếu có file config, load từ file
        // Nếu không có file, hiển thị default values
        if (config != null)
        {
            ServerHostTextBox.Text = config.Host ?? "localhost";
            PortTextBox.Text = config.Port.ToString();
            DatabaseNameTextBox.Text = config.Database ?? "myshop_db";
            UsernameTextBox.Text = config.Username ?? "postgres";
            // Password không load lại vì lý do bảo mật
        }
        else
        {
            // Hiển thị default values
            ServerHostTextBox.Text = "localhost";
            PortTextBox.Text = "5432";
            DatabaseNameTextBox.Text = "myshop_db";
            UsernameTextBox.Text = "postgres";
        }
    }

    /// <summary>
    /// Build connection string từ các TextBox
    /// </summary>
    private string BuildConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = string.IsNullOrWhiteSpace(ServerHostTextBox.Text) ? "localhost" : ServerHostTextBox.Text,
            Port = int.TryParse(PortTextBox.Text, out var port) ? port : 5432,
            Database = string.IsNullOrWhiteSpace(DatabaseNameTextBox.Text) ? "myshop_db" : DatabaseNameTextBox.Text,
            Username = string.IsNullOrWhiteSpace(UsernameTextBox.Text) ? "postgres" : UsernameTextBox.Text,
            Password = PasswordBox.Password
        };
        
        return builder.ConnectionString;
    }

    private async void TestConnectionButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        StatusTextBlock.Text = "Đang kiểm tra kết nối...";
        StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Blue);
        
        var connectionString = BuildConnectionString();
        var (success, errorMessage) = await _testService.TestConnectionAsync(connectionString);
        
        if (success)
        {
            StatusTextBlock.Text = "Kết nối thành công!";
            StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
        }
        else
        {
            StatusTextBlock.Text = $"Lỗi kết nối: {errorMessage}";
            StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
        }
    }

    private async void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            var config = new DatabaseConfig
            {
                Host = string.IsNullOrWhiteSpace(ServerHostTextBox.Text) ? "localhost" : ServerHostTextBox.Text,
                Port = int.TryParse(PortTextBox.Text, out var port) ? port : 5432,
                Database = string.IsNullOrWhiteSpace(DatabaseNameTextBox.Text) ? "myshop_db" : DatabaseNameTextBox.Text,
                Username = string.IsNullOrWhiteSpace(UsernameTextBox.Text) ? "postgres" : UsernameTextBox.Text,
                Password = PasswordBox.Password
            };
            
            await _configService.SaveConfigAsync(config);
            
            var filePath = await _configService.GetConfigFilePathAsync();
            StatusTextBlock.Text = $"Đã lưu cấu hình thành công!\nFile: {filePath}";
            StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Lỗi khi lưu: {ex.Message}";
            StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
        }
    }

    private async void OpenConfigFileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            var filePath = await _configService.GetConfigFilePathAsync();
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath);
            await Launcher.LaunchFileAsync(file);
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Không thể mở file: {ex.Message}";
            StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
        }
    }
}

