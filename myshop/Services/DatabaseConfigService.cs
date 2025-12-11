using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace myshop.Services;

/// <summary>
/// Database configuration model để serialize/deserialize JSON
/// </summary>
public class DatabaseConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Database { get; set; } = "myshop_db";
    public string Username { get; set; } = "postgres";
    public string Password { get; set; } = "";

    /// <summary>
    /// Build connection string từ các properties
    /// </summary>
    public string ToConnectionString()
    {
        return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";
    }

    /// <summary>
    /// Parse connection string thành DatabaseConfig
    /// </summary>
    public static DatabaseConfig FromConnectionString(string connectionString)
    {
        var config = new DatabaseConfig();
        if (string.IsNullOrEmpty(connectionString))
            return config;

        try
        {
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                var keyValue = part.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();

                    switch (key.ToLower())
                    {
                        case "host":
                            config.Host = value;
                            break;
                        case "port":
                            if (int.TryParse(value, out var port))
                                config.Port = port;
                            break;
                        case "database":
                            config.Database = value;
                            break;
                        case "username":
                            config.Username = value;
                            break;
                        case "password":
                            config.Password = value;
                            break;
                    }
                }
            }
        }
        catch
        {
            // Return default config if parsing fails
        }

        return config;
    }
}

public class DatabaseConfigService
{
    private const string ConfigFileName = "database.json";
    private StorageFile? _configFile;

    // Connection string mặc định cho development
    // Có thể override bằng cách tạo file database.json hoặc qua ConfigPage
    private static readonly DatabaseConfig DefaultConfig = new DatabaseConfig
    {
        Host = "localhost",
        Port = 5432,
        Database = "myshop_db",
        Username = "postgres",
        Password = "password" // ⚠️ THAY ĐỔI PASSWORD Ở ĐÂY cho database của bạn
    };

    /// <summary>
    /// Lấy đường dẫn file config (có thể mở bằng text editor)
    /// </summary>
    public async Task<string> GetConfigFilePathAsync()
    {
        var localFolder = ApplicationData.Current.LocalFolder;
        var file = await localFolder.TryGetItemAsync(ConfigFileName) as StorageFile;
        if (file != null)
        {
            return file.Path;
        }
        return Path.Combine(localFolder.Path, ConfigFileName);
    }

    /// <summary>
    /// Lấy file config (KHÔNG tự động tạo - chỉ tạo khi user lưu config)
    /// </summary>
    private async Task<StorageFile?> GetConfigFileAsync()
    {
        if (_configFile != null)
            return _configFile;

        var localFolder = ApplicationData.Current.LocalFolder;
        try
        {
            _configFile = await localFolder.GetFileAsync(ConfigFileName);
        }
        catch
        {
            // File chưa tồn tại - không tạo tự động
            // File sẽ được tạo khi user nhấn "Lưu cấu hình" trong ConfigPage
            return null;
        }

        return _configFile;
    }

    /// <summary>
    /// Lấy connection string từ file JSON, nếu không có thì dùng default
    /// </summary>
    public async Task<string> GetConnectionStringAsync()
    {
        try
        {
            var config = await LoadConfigAsync();
            // Nếu file không tồn tại hoặc password rỗng, dùng default config
            if (config == null || string.IsNullOrEmpty(config.Password))
            {
                return DefaultConfig.ToConnectionString();
            }
            return config.ToConnectionString();
        }
        catch
        {
            // Nếu có lỗi, dùng default config
            return DefaultConfig.ToConnectionString();
        }
    }

    /// <summary>
    /// Lưu connection string vào file JSON
    /// </summary>
    public async Task SaveConnectionStringAsync(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            // Xóa file nếu connection string rỗng
            try
            {
                var file = await GetConfigFileAsync();
                if (file != null)
                {
                    await file.DeleteAsync();
                    _configFile = null;
                }
            }
            catch
            {
                // Ignore
            }
            return;
        }

        var config = DatabaseConfig.FromConnectionString(connectionString);
        await SaveConfigAsync(config);
    }

    /// <summary>
    /// Load config từ file JSON, nếu không có file thì return null
    /// </summary>
    public async Task<DatabaseConfig?> LoadConfigAsync()
    {
        try
        {
            var file = await GetConfigFileAsync();
            if (file == null)
                return null; // File chưa tồn tại
            
            var json = await FileIO.ReadTextAsync(file);
            
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<DatabaseConfig>(json);
        }
        catch
        {
            return null; // File không tồn tại hoặc có lỗi
        }
    }

    /// <summary>
    /// Lưu config vào file JSON (tạo file nếu chưa có)
    /// </summary>
    public async Task SaveConfigAsync(DatabaseConfig config)
    {
        var localFolder = ApplicationData.Current.LocalFolder;
        
        // Tạo file nếu chưa có
        if (_configFile == null)
        {
            _configFile = await localFolder.CreateFileAsync(ConfigFileName, CreationCollisionOption.ReplaceExisting);
        }
        
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
        { 
            WriteIndented = true // Format đẹp để dễ đọc/edit
        });
        await FileIO.WriteTextAsync(_configFile, json);
    }

    /// <summary>
    /// Kiểm tra xem đã có connection string chưa
    /// </summary>
    public async Task<bool> HasConnectionStringAsync()
    {
        try
        {
            var config = await LoadConfigAsync();
            return config != null && !string.IsNullOrEmpty(config.Password);
        }
        catch
        {
            return false;
        }
    }
}
