using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace Data.Context;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var apiAppSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "API", "appsettings.json");
            if (!File.Exists(apiAppSettingsPath))
            {
                apiAppSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "API", "appsettings.json");
            }

            if (File.Exists(apiAppSettingsPath))
            {
                var json = File.ReadAllText(apiAppSettingsPath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var connSection)
                    && connSection.TryGetProperty("DefaultConnection", out var defaultConn))
                {
                    connectionString = defaultConn.GetString();
                }
            }
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection was not found for design-time DbContext creation.");
        }

        optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("Data"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
