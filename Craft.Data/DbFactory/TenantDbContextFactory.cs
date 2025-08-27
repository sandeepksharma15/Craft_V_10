using Craft.Core;
using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Data;

/// <summary>
/// Factory that creates a <see cref="DbContext"/> instance honoring the current tenant's database strategy.
/// Supports Shared, PerTenant and Hybrid strategies and falls back to shared defaults when multi-tenancy is disabled.
/// </summary>
/// <typeparam name="T">Concrete DbContext type.</typeparam>
public class TenantDbContextFactory<T> : IDbContextFactory<T> where T : DbContext, IDbContext
{
    private readonly ICurrentTenant _currentTenant;                // Current resolved tenant (may be a lightweight proxy)
    private readonly IEnumerable<IDatabaseProvider> _providers;    // Registered database providers
    private readonly IServiceProvider _sp;                         // Root service provider for context construction
    private readonly DatabaseOptions _dbOptions;                   // Default / shared database options
    private readonly MultiTenantOptions _multiTenantOptions;       // Multi-tenant feature options

    public TenantDbContextFactory(ICurrentTenant currentTenant, IEnumerable<IDatabaseProvider> providers,
        IServiceProvider sp, IOptions<DatabaseOptions> dbOptions, IOptions<MultiTenantOptions> multiTenantOptions)
    {
        _currentTenant = currentTenant;
        _providers = providers;
        _sp = sp;
        _dbOptions = dbOptions.Value;
        _multiTenantOptions = multiTenantOptions.Value;
    }

    /// <summary>
    /// Creates a DbContext configured for the current tenant (or shared defaults if multi-tenancy disabled).
    /// </summary>
    public T CreateDbContext()
    {
        var target = DetermineTarget();

        // Build DbContextOptions for the selected provider/connection
        var options = BuildOptions(target.ConnectionString, target.ProviderKey);

        // IMPORTANT: pass the configured options instance so provider configuration is honored
        return ActivatorUtilities.CreateInstance<T>(_sp, options);
    }

    #region Target Resolution

    private (string ConnectionString, string ProviderKey) DetermineTarget()
    {
        // Multi-tenancy disabled OR no tenant context -> always use shared defaults.
        if (_multiTenantOptions.IsEnabled == false || _currentTenant == null)
            return (_dbOptions.ConnectionString, _dbOptions.DbProvider);

        return _currentTenant.DbType switch
        {
            TenantDbType.Shared => (_dbOptions.ConnectionString, _dbOptions.DbProvider),
            TenantDbType.PerTenant => ResolvePerTenant(),
            TenantDbType.Hybrid => ResolveHybrid(),
            _ => throw new InvalidOperationException($"Unsupported DbType: {_currentTenant.DbType}")
        };
    }

    private (string ConnectionString, string ProviderKey) ResolvePerTenant()
    {
        if (string.IsNullOrEmpty(_currentTenant.ConnectionString))
            throw new InvalidOperationException("Tenant is PerTenant but has no connection string defined.");

        return (_currentTenant.ConnectionString, NormalizeProvider(_currentTenant.DbProvider));
    }

    private (string ConnectionString, string ProviderKey) ResolveHybrid()
    {
        // Own DB when a connection string is provided, else shared defaults
        if (!string.IsNullOrEmpty(_currentTenant.ConnectionString))
            return (_currentTenant.ConnectionString, NormalizeProvider(_currentTenant.DbProvider));

        return (_dbOptions.ConnectionString, _dbOptions.DbProvider);
    }

    private string NormalizeProvider(string? tenantProvider) => string.IsNullOrWhiteSpace(tenantProvider)
        ? _dbOptions.DbProvider
        : tenantProvider;

    #endregion

    #region Provider Configuration

    private DbContextOptions<T> BuildOptions(string connectionString, string providerKey)
    {
        var provider = _providers.FirstOrDefault(p => p.CanHandle(providerKey))
            ?? throw new NotSupportedException($"Provider '{providerKey}' not supported");

        var builder = new DbContextOptionsBuilder<T>();
        provider.Configure(builder, connectionString, _dbOptions);

        // Optional additional standard flags from options
        if (_dbOptions.EnableDetailedErrors)
            builder.EnableDetailedErrors();
        if (_dbOptions.EnableSensitiveDataLogging)
            builder.EnableSensitiveDataLogging();

        return builder.Options;
    }

    #endregion
}
