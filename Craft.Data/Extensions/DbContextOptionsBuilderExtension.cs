using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

public static class DbContextOptionsBuilderExtension
{
    private const string MsSqlMigrationAssembly = "Migrators.MSSQL";
    // private const string MySqlMigrationAssembly = "Migrators.MySQL";
    private const string PostgreSqlMigrationAssembly = "Migrators.PostgreSQL";

    public static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string dbProvider,
        string connectionString, int maxRetryCount, int maxRetryDelay, int commandTimeout)
    {
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (dbProvider.ToLowerInvariant())
        {
            case DbProviderKeys.Npgsql:
                return builder.UseNpgsql(connectionString, optionAction =>
                {
                    optionAction.EnableRetryOnFailure(maxRetryCount: maxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelay),
                        null);
                    optionAction.CommandTimeout(commandTimeout);
                    optionAction.MigrationsAssembly(PostgreSqlMigrationAssembly);
                });

            case DbProviderKeys.SqlServer:
                return builder.UseSqlServer(connectionString, optionAction =>
                {
                    optionAction.EnableRetryOnFailure(
                        maxRetryCount: maxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelay),
                        null);
                    optionAction.CommandTimeout(commandTimeout);
                    optionAction.MigrationsAssembly(MsSqlMigrationAssembly);
                });

            //case DbProviderKeys.MySql:
            //    return builder.UseMySql(connectionString,
            //        ServerVersion.AutoDetect(connectionString), optionAction =>
            //    {
            //        optionAction.EnableRetryOnFailure(
            //            maxRetryCount: maxRetryCount,
            //            maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelay),
            //            null);
            //        optionAction.CommandTimeout(commandTimeout);
            //        optionAction.MigrationsAssembly(MySqlMigrationAssembly);
            //    });

            default:
                throw new InvalidOperationException($"DB Provider {dbProvider} is not supported.");
        }
#pragma warning restore IDE0066 // Convert switch statement to expression
    }
}
