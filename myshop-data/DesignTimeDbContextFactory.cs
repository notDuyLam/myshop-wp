using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using myshop_data.Data;

namespace myshop_data
{
    // Design-time factory for EF Core tools (dotnet ef)
    // It provides a DbContext instance when migrations are added/updated from CLI.
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Try to read connection string from environment variable first
            var connectionString = Environment.GetEnvironmentVariable("MYSHOP_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=myshop_db;Username=postgres;Password=password";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention(); 

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
