using Microsoft.EntityFrameworkCore;
using myshop_data.Data;
using System.Threading.Tasks;

namespace myshop.Services;

/// <summary>
/// Service quản lý việc tạo DbContext với connection string động
/// </summary>
public class DbContextService
{
    private readonly DatabaseConfigService _configService;
    private string? _cachedConnectionString;

    public DbContextService(DatabaseConfigService configService)
    {
        _configService = configService;
    }

    /// <summary>
    /// Tạo DbContext với connection string từ DatabaseConfigService
    /// </summary>
    public async Task<AppDbContext> CreateDbContextAsync()
    {
        // Lấy connection string (cache lại để tránh đọc file nhiều lần)
        if (_cachedConnectionString == null)
        {
            _cachedConnectionString = await _configService.GetConnectionStringAsync();
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(_cachedConnectionString).UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Làm mới cache connection string (gọi khi user đổi cấu hình)
    /// </summary>
    public void RefreshConnectionString()
    {
        _cachedConnectionString = null;
    }
    public async Task MigrateAndSeedAsync()
    {
        await using var context = await CreateDbContextAsync();

        // Áp dụng migration (nếu có migration mới thì sẽ update DB, không có thì không làm gì)
        await context.Database.MigrateAsync();

    }
}
