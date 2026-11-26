
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
            connectionString.Contains(
                "YOUR_LOCAL_MYSQL",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "DefaultConnection is missing. Configure your Railway " +
                "public MySQL connection in User Secrets.");
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
