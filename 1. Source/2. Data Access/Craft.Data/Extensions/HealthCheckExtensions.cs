using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Craft.Data;

/// <summary>
/// Extension methods for adding database health checks.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds a health check for the specified DbContext.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type to check.</typeparam>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">The name of the health check (default: "database").</param>
    /// <param name="failureStatus">The health status to report on failure (default: Unhealthy).</param>
    /// <param name="tags">Optional tags for categorizing the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddDatabaseHealthCheck<TContext>(this IHealthChecksBuilder builder,
        string name = "database", HealthStatus failureStatus = HealthStatus.Unhealthy, IEnumerable<string>? tags = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        tags ??= ["db", "ready"];

        return builder.AddDbContextCheck<TContext>(name, failureStatus, tags);
    }

    /// <summary>
    /// Adds a SQL Server database connection health check.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="connectionString">The SQL Server connection string to check.</param>
    /// <param name="name">The name of the health check (default: "sqlserver").</param>
    /// <param name="failureStatus">The health status to report on failure (default: Unhealthy).</param>
    /// <param name="tags">Optional tags for categorizing the health check.</param>
    /// <param name="timeout">Optional timeout for the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddSqlServerHealthCheck(this IHealthChecksBuilder builder,
        string connectionString, string name = "sqlserver", HealthStatus failureStatus = HealthStatus.Unhealthy,
        IEnumerable<string>? tags = null, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        tags ??= ["db", "sqlserver", "ready"];

        return builder.AddSqlServer(connectionString, healthQuery: "SELECT 1;", failureStatus: failureStatus, name: name, 
            tags: tags, timeout: timeout);
    }

    /// <summary>
    /// Adds a PostgreSQL database connection health check.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="connectionString">The PostgreSQL connection string to check.</param>
    /// <param name="name">The name of the health check (default: "postgresql").</param>
    /// <param name="failureStatus">The health status to report on failure (default: Unhealthy).</param>
    /// <param name="tags">Optional tags for categorizing the health check.</param>
    /// <param name="timeout">Optional timeout for the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddPostgreSqlHealthCheck(this IHealthChecksBuilder builder,
        string connectionString, string name = "postgresql", HealthStatus failureStatus = HealthStatus.Unhealthy,
        IEnumerable<string>? tags = null, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        tags ??= ["db", "postgresql", "ready"];

        return builder.AddNpgSql(connectionString, healthQuery: "SELECT 1;", failureStatus: failureStatus, name: name,
            tags: tags, timeout: timeout);
    }

    /// <summary>
    /// Adds a database connection health check based on the provider type.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="connectionString">The connection string to check.</param>
    /// <param name="dbProvider">The database provider key.</param>
    /// <param name="name">The name of the health check (default: "database-connection").</param>
    /// <param name="failureStatus">The health status to report on failure (default: Unhealthy).</param>
    /// <param name="tags">Optional tags for categorizing the health check.</param>
    /// <param name="timeout">Optional timeout for the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddDatabaseConnectionHealthCheck(this IHealthChecksBuilder builder,
        string connectionString, string dbProvider, string name = "database-connection",
        HealthStatus failureStatus = HealthStatus.Unhealthy, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(dbProvider);

        tags ??= ["db", "ready"];

        return dbProvider.ToLowerInvariant() switch
        {
            DbProviderKeys.SqlServer => builder.AddSqlServerHealthCheck(connectionString, name, failureStatus, tags,
                timeout),

            DbProviderKeys.Npgsql => builder.AddPostgreSqlHealthCheck(connectionString, name, failureStatus, tags,
                timeout),

            // TODO: Add MySQL health check once AspNetCore.HealthChecks.MySql package is added
            // DbProviderKeys.MySql => builder.AddMySqlHealthCheck(connectionString, name, failureStatus, tags, timeout),

            _ => throw new NotSupportedException($"Health check for provider '{dbProvider}' is not supported.")
        };
    }

    /// <summary>
    /// Adds database health checks using configured DatabaseOptions.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="options">The database options containing connection details.</param>
    /// <param name="name">The name of the health check (default: "database").</param>
    /// <param name="failureStatus">The health status to report on failure (default: Unhealthy).</param>
    /// <param name="tags">Optional tags for categorizing the health check.</param>
    /// <param name="timeout">Optional timeout for the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddDatabaseHealthCheck(this IHealthChecksBuilder builder, DatabaseOptions options,
        string name = "database", HealthStatus failureStatus = HealthStatus.Unhealthy, IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        return builder.AddDatabaseConnectionHealthCheck(options.ConnectionString, options.DbProvider, name,
            failureStatus, tags, timeout);
    }
}

