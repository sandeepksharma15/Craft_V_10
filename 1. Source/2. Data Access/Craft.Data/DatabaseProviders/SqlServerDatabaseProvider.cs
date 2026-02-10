using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

public class SqlServerDatabaseProvider : IDatabaseProvider
{
    public bool CanHandle(string dbProvider) =>
        dbProvider.Equals(DbProviderKeys.SqlServer, StringComparison.OrdinalIgnoreCase);

    public void Configure(DbContextOptionsBuilder builder, string connectionString, DatabaseOptions options)
    {
        builder.UseSqlServer(connectionString, opt =>
        {
            opt.EnableRetryOnFailure(options.MaxRetryCount, TimeSpan.FromSeconds(options.MaxRetryDelay), null);
            opt.CommandTimeout(options.CommandTimeout);
            opt.MigrationsAssembly(IDatabaseProvider.MsSqlMigrationAssembly);
        });
    }

    public bool ValidateConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return true; // Connection successful
            }
        }
        catch (SqlException)
        {
            return false; // Connection failed
        }
    }
}

