using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;

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

            // Use configured migration assembly or fall back to default
            if (!string.IsNullOrWhiteSpace(options.MigrationAssembly))
                opt.MigrationsAssembly(options.MigrationAssembly);
            else
                opt.MigrationsAssembly(IDatabaseProvider.PostgreSqlMigrationAssembly);
        });
    }

    public bool ValidateConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(
        string connectionString,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        timeout ??= TimeSpan.FromSeconds(5);

        var result = new ConnectionTestResult
        {
            Provider = nameof(PostgreSqlDatabaseProvider)
        };

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            result.IsSuccessful = false;
            result.ErrorMessage = "Connection string is null or empty";
            result.Message = "Connection test failed: Invalid connection string";
            return result;
        }

        var sw = Stopwatch.StartNew();

        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            result.LatencyMs = sw.Elapsed.TotalMilliseconds;
            result.DatabaseName = connection.Database;
            result.ServerVersion = connection.ServerVersion;
            result.IsSuccessful = true;
            result.Message = $"Connection successful to {connection.Host}:{connection.Port}/{connection.Database}";

            await connection.CloseAsync();
        }
        catch (PostgresException ex)
        {
            result.IsSuccessful = false;
            result.ErrorMessage = ex.MessageText;
            result.Message = $"PostgreSQL error: {ex.MessageText}";
            result.LatencyMs = sw.Elapsed.TotalMilliseconds;
        }
        catch (Exception ex)
        {
            result.IsSuccessful = false;
            result.ErrorMessage = ex.Message;
            result.Message = $"Connection test failed: {ex.Message}";
            result.LatencyMs = sw.Elapsed.TotalMilliseconds;
        }

        return result;
    }
}

