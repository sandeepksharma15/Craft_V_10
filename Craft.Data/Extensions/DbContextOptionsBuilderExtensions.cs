using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Data;

/// <summary>
/// Extension methods for <see cref="DbContextOptionsBuilder"/> to simplify database configuration.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configures the DbContext to use a specific database provider with the given connection string and options.
    /// </summary>
    /// <param name="builder">The DbContextOptionsBuilder to configure.</param>
    /// <param name="dbProvider">The database provider key (e.g., "mssql", "postgresql").</param>
    /// <param name="connectionString">The connection string to the database.</param>
    /// <param name="options">Database configuration options.</param>
    /// <param name="serviceProvider">Optional service provider to resolve database providers.</param>
    /// <returns>The configured DbContextOptionsBuilder for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder, dbProvider, or connectionString is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when the specified database provider is not supported.</exception>
    public static DbContextOptionsBuilder UseDatabase(
        this DbContextOptionsBuilder builder,
        string dbProvider,
        string connectionString,
        DatabaseOptions options,
        IServiceProvider? serviceProvider = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(dbProvider);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(options);

        // Resolve provider from service provider or use built-in providers
        IDatabaseProvider? provider = null;

        if (serviceProvider != null)
        {
            var providers = serviceProvider.GetServices<IDatabaseProvider>();
            provider = providers.FirstOrDefault(p => p.CanHandle(dbProvider));
        }

        // Fallback to built-in providers if not found in DI
        provider ??= dbProvider.ToLowerInvariant() switch
        {
            DbProviderKeys.SqlServer => new SqlServerDatabaseProvider(),
            DbProviderKeys.Npgsql => new PostgreSqlDatabaseProvider(),
            _ => null
        };

        if (provider == null)
            throw new NotSupportedException($"Database provider '{dbProvider}' is not supported.");

        provider.Configure(builder, connectionString, options);

        // Apply additional options
        if (options.EnableDetailedErrors)
            builder.EnableDetailedErrors();

        if (options.EnableSensitiveDataLogging)
            builder.EnableSensitiveDataLogging();

        return builder;
    }

    /// <summary>
    /// Configures the DbContext to use a specific database provider with simplified parameters.
    /// </summary>
    /// <param name="builder">The DbContextOptionsBuilder to configure.</param>
    /// <param name="dbProvider">The database provider key (e.g., "mssql", "postgresql").</param>
    /// <param name="connectionString">The connection string to the database.</param>
    /// <param name="maxRetryCount">Maximum number of retry attempts on transient failures.</param>
    /// <param name="maxRetryDelay">Maximum delay (in seconds) between retries.</param>
    /// <param name="commandTimeout">Command timeout in seconds.</param>
    /// <param name="serviceProvider">Optional service provider to resolve database providers.</param>
    /// <returns>The configured DbContextOptionsBuilder for chaining.</returns>
    public static DbContextOptionsBuilder UseDatabase(
        this DbContextOptionsBuilder builder,
        string dbProvider,
        string connectionString,
        int maxRetryCount = 3,
        int maxRetryDelay = 15,
        int commandTimeout = 30,
        IServiceProvider? serviceProvider = null)
    {
        var options = new DatabaseOptions
        {
            ConnectionString = connectionString,
            DbProvider = dbProvider,
            MaxRetryCount = maxRetryCount,
            MaxRetryDelay = maxRetryDelay,
            CommandTimeout = commandTimeout
        };

        return builder.UseDatabase(dbProvider, connectionString, options, serviceProvider);
    }

    /// <summary>
    /// Configures the DbContext to use a specific database provider with typed options builder.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="builder">The DbContextOptionsBuilder to configure.</param>
    /// <param name="dbProvider">The database provider key (e.g., "mssql", "postgresql").</param>
    /// <param name="connectionString">The connection string to the database.</param>
    /// <param name="options">Database configuration options.</param>
    /// <param name="serviceProvider">Optional service provider to resolve database providers.</param>
    /// <returns>The configured DbContextOptionsBuilder for chaining.</returns>
    public static DbContextOptionsBuilder<TContext> UseDatabase<TContext>(
        this DbContextOptionsBuilder<TContext> builder,
        string dbProvider,
        string connectionString,
        DatabaseOptions options,
        IServiceProvider? serviceProvider = null)
        where TContext : DbContext
    {
        UseDatabase((DbContextOptionsBuilder)builder, dbProvider, connectionString, options, serviceProvider);
        return builder;
    }

    /// <summary>
    /// Configures the DbContext to use a specific database provider with typed options builder and simplified parameters.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="builder">The DbContextOptionsBuilder to configure.</param>
    /// <param name="dbProvider">The database provider key (e.g., "mssql", "postgresql").</param>
    /// <param name="connectionString">The connection string to the database.</param>
    /// <param name="maxRetryCount">Maximum number of retry attempts on transient failures.</param>
    /// <param name="maxRetryDelay">Maximum delay (in seconds) between retries.</param>
    /// <param name="commandTimeout">Command timeout in seconds.</param>
    /// <param name="serviceProvider">Optional service provider to resolve database providers.</param>
    /// <returns>The configured DbContextOptionsBuilder for chaining.</returns>
    public static DbContextOptionsBuilder<TContext> UseDatabase<TContext>(
        this DbContextOptionsBuilder<TContext> builder,
        string dbProvider,
        string connectionString,
        int maxRetryCount = 3,
        int maxRetryDelay = 15,
        int commandTimeout = 30,
        IServiceProvider? serviceProvider = null)
        where TContext : DbContext
    {
        UseDatabase((DbContextOptionsBuilder)builder, dbProvider, connectionString, maxRetryCount, maxRetryDelay, commandTimeout, serviceProvider);
        return builder;
    }
}
