using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using myshop_data.Data;
using Npgsql;

namespace myshop.Services;

public class DatabaseTestService
{
    /// <summary>
    /// Test kết nối database với connection string cho trước
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> TestConnectionAsync(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return (false, "Connection string is empty");
        }

        try
        {
            // Test connection với Npgsql trực tiếp
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            await connection.CloseAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Test kết nối database với DbContext
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> TestDbContextAsync(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return (false, "Connection string is empty");
        }

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var context = new AppDbContext(options);
            var canConnect = await context.Database.CanConnectAsync();
            
            if (canConnect)
            {
                return (true, null);
            }
            else
            {
                return (false, "Cannot connect to database");
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}

