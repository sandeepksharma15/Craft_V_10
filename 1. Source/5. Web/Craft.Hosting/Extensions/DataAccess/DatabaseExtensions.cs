using Craft.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Craft.Hosting.Extensions;

/// <summary>
/// Extension methods for configuring Craft DbContext with modern DI patterns and pooling support.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Adds a Craft DbContext with standard registration (Scoped lifetime).
    /// Use this when you don't need pooling or for backward compatibility.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="optionsAction">Action to configure DbContext options.</param>
    /// <param name="contextLifetime">The lifetime for the DbContext (default: Scoped).</param>
    /// <param name="optionsLifetime">The lifetime for DbContextOptions (default: Scoped).</param>
    public static IServiceCollection AddCraftDbContext<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            if (loggerFactory != null)
                options.UseLoggerFactory(loggerFactory);

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsAction?.Invoke(serviceProvider, options);
        }, contextLifetime, optionsLifetime);

        return services;
    }

    /// <summary>
    /// Adds a Craft DbContext with POOLING for better performance.
    /// Recommended for production - provides ~10-20% better performance.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="optionsAction">Action to configure DbContext options.</param>
    /// <param name="poolSize">The maximum number of instances retained by the pool (default: 1024).</param>
    public static IServiceCollection AddCraftDbContextPool<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
        int poolSize = 1024)
        where TContext : DbContext
    {
        services.AddDbContextPool<TContext>((serviceProvider, options) =>
        {
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            if (loggerFactory != null)
                options.UseLoggerFactory(loggerFactory);

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsAction?.Invoke(serviceProvider, options);
        }, poolSize);

        return services;
    }

    /// <summary>
    /// Configures PostgreSQL with Aspire integration.
    /// Supports both pooled and non-pooled registration.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to read connection string from.</param>
    /// <param name="connectionName">The connection string name.</param>
    /// <param name="enablePooling">Whether to enable DbContext pooling (default: true).</param>
    /// <param name="poolSize">The pool size if pooling is enabled (default: 1024).</param>
    /// <param name="additionalConfiguration">Additional configuration action.</param>
    public static IServiceCollection AddCraftPostgreSql<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionName,
        bool enablePooling = true,
        int poolSize = 1024,
        Action<DbContextOptionsBuilder>? additionalConfiguration = null)
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionName);

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException(
                $"Connection string '{connectionName}' not found in configuration.");

        services.AddNpgsqlDataSource(connectionString);

        if (enablePooling)
        {
            services.AddCraftDbContextPool<TContext>((serviceProvider, options) =>
            {
                ConfigurePostgreSqlOptions(serviceProvider, options, additionalConfiguration);
            }, poolSize);
        }
        else
        {
            services.AddCraftDbContext<TContext>((serviceProvider, options) =>
            {
                ConfigurePostgreSqlOptions(serviceProvider, options, additionalConfiguration);
            });
        }

        return services;
    }

    /// <summary>
    /// Configures database services including providers, connection string handlers, and custom seeders.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IDatabaseProvider, SqlServerDatabaseProvider>();
        services.AddSingleton<IDatabaseProvider, PostgreSqlDatabaseProvider>();

        services.AddSingleton<IConnectionStringHandler, SqlServerConnectionStringHandler>();
        services.AddSingleton<IConnectionStringHandler, PostgreSqlConnectionStringHandler>();
        services.AddSingleton<ConnectionStringService>();

        services.AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient);

        services.AddScoped<Craft.Data.Helpers.CustomSeederRunner>();
        services.AddScoped(typeof(Craft.Data.IDbContextFactory<>), typeof(TenantDbContextFactory<>));

        return services;
    }

    private static void ConfigurePostgreSqlOptions(
        IServiceProvider serviceProvider,
        DbContextOptionsBuilder options,
        Action<DbContextOptionsBuilder>? additionalConfiguration)
    {
        var dataSource = serviceProvider.GetRequiredService<Npgsql.NpgsqlDataSource>();

        options.UseNpgsql(dataSource, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        });

        additionalConfiguration?.Invoke(options);
    }
}

