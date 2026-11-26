
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MultiMediaRoomReservationSystem.Data;

public class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddUserSecrets<ApplicationDbContextFactory>(
                optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration
            .GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.Contains("MYSQL_PUBLIC_URL",
            StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("MYSQL_PUBLIC_URL",
            StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("MYSQLDATABASE",
            StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("MYSQLUSER",
            StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("MYSQLPASSWORD",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "DefaultConnection is missing or contains placeholder values. " +
                "Check your Railway connection in User Secrets.");
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 0))
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
