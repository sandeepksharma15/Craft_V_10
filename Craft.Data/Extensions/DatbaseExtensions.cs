using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Data;

public static class DatabaseExtensions
{
    public static IServiceCollection ConfigureDatabases(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind DatabaseOptions
        var dbOptions = new DatabaseOptions();
        configuration.GetSection(DatabaseOptions.SectionName).Bind(dbOptions);

        // Bind MultiTenantOptions
        var multiTenantOptions = new MultiTenantOptions();
        configuration.GetSection(MultiTenantOptions.SectionName).Bind(multiTenantOptions);

        // Configure database options from configuration
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        // Register database providers
        services.AddSingleton<IDatabaseProvider, SqlServerDatabaseProvider>();
        services.AddSingleton<IDatabaseProvider, PostgreSqlDatabaseProvider>();

        // Register connection string handlers
        services.AddSingleton<IConnectionStringHandler, MsSqlConnectionStringHandler>();
        services.AddSingleton<IConnectionStringHandler, PostgreSqlConnectionStringHandler>();
        services.AddSingleton<ConnectionStringService, ConnectionStringService>();

        // Write the code to register all the ICustomSeeder implementation across all assemblies as transient services
        services.AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient);

        // Register the tenant-aware DbContext factory
        services.AddScoped(typeof(IDbContextFactory<>), typeof(TenantDbContextFactory<>));


        // If app is NOT multi-tenant, register a "default tenant" so that ICurrentTenant can always be injected
        // And the DbContextFactory can always resolve a tenant context to create context
        //services.AddScoped<ICurrentTenant>(sp =>
        //{
        //    var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        //    return (ICurrentTenant)(ITenant)new Tenant
        //    {
        //        Identifier = "Default",
        //        Name = "DefaultTenant",
        //        DbType = TenantDbType.Shared,
        //        ConnectionString = dbOptions.ConnectionString,
        //        DbProvider = dbOptions.DbProvider
        //    };
        //});

        // 4. Register AppDbContext
        //if (multiTenantOptions.IsEnabled == false)
        //{
        //    // Single-tenant: Use DbContext pooling for memory efficiency
        //    services.AddDbContextPool<AppDbContext>((sp, optionsBuilder) =>
        //    {
        //        var tenant = sp.GetRequiredService<ICurrentTenant>();
        //        var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();

        //        // Build options using our factory helper
        //        var tempContext = factory.CreateDbContext() as AppDbContext;
        //        var dbProvider = tenant.DbProvider ?? dbOptions.DbProvider;
        //        var connectionString = tenant.ConnectionString ?? dbOptions.ConnectionString;

        //        tempContext!.Database.GetDbConnection(); // ensure connection is initialized

        //        optionsBuilder.UseDatabase(
        //            dbProvider,
        //            connectionString,
        //            dbOptions.MaxRetryCount,
        //            dbOptions.MaxRetryDelay,
        //            dbOptions.CommandTimeout);

        //        optionsBuilder.EnableSensitiveDataLogging(dbOptions.EnableSensitiveDataLogging);
        //        optionsBuilder.EnableDetailedErrors(dbOptions.EnableDetailedErrors);
        //    });
        //}
        //else
        //{
        //    // Multi-tenant or hybrid: Scoped DbContext per tenant
        //    services.AddScoped(sp =>
        //    {
        //        var tenant = sp.GetRequiredService<ICurrentTenant>();
        //        var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
        //        return factory.CreateDbContext() as AppDbContext;
        //    });
        //}

        return services;
    }
}
