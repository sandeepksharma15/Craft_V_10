using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

public class PostgreSqlDatabaseProvider : IDatabaseProvider
{
    public bool CanHandle(string dbProvider) =>
        dbProvider.Equals(DbProviderKeys.Npgsql, StringComparison.OrdinalIgnoreCase);

    public void Configure(DbContextOptionsBuilder builder, string connectionString, DatabaseOptions options)
    {
        builder.UseNpgsql(connectionString, opt =>
        {
            opt.EnableRetryOnFailure(options.MaxRetryCount, TimeSpan.FromSeconds(options.MaxRetryDelay), null);
            opt.CommandTimeout(options.CommandTimeout);
            opt.MigrationsAssembly(IDatabaseProvider.PostgreSqlMigrationAssembly);
        });
    }

    public bool ValidateConnection(string connectionString)
    {
        // Check if the connection string is null or empty
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        try
        {
            // Attempt to open a connection to the DB here
            using (var context = new DbContext(new DbContextOptionsBuilder().UseNpgsql(connectionString).Options))
            {
                context.Database.OpenConnection();
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}

