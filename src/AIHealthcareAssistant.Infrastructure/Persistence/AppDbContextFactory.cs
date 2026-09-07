using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AIHealthcareAssistant.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core tools (migrations add / database update).
/// Reads the connection string from the HEALTHCARE_DB_CONNECTION environment variable
/// and otherwise falls back to the local SQL Server (LocalDB) instance.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=AIHealthcareAssistantDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("HEALTHCARE_DB_CONNECTION") ?? DefaultConnectionString;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}
