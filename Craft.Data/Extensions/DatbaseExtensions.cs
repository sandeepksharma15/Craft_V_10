using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Data;

/// <summary>
/// Extension methods for configuring database services.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Configures database services including providers, connection string handlers, and custom seeders.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure database options from configuration with validation
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register database providers
        services.AddSingleton<IDatabaseProvider, SqlServerDatabaseProvider>();
        services.AddSingleton<IDatabaseProvider, PostgreSqlDatabaseProvider>();

        // Register connection string handlers
        services.AddSingleton<IConnectionStringHandler, MsSqlConnectionStringHandler>();
        services.AddSingleton<IConnectionStringHandler, PostgreSqlConnectionStringHandler>();
        services.AddSingleton<ConnectionStringService>();

        // Register all ICustomSeeder implementations across all assemblies as transient services
        services.AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient);

        // Register the seeder runner
        services.AddScoped<Helpers.CustomSeederRunner>();

        // Register the tenant-aware DbContext factory
        services.AddScoped(typeof(IDbContextFactory<>), typeof(TenantDbContextFactory<>));

        return services;
    }
}
