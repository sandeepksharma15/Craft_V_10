using Craft.Core;
using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Data;

/// <summary>
/// Factory that creates a <see cref="DbContext"/> instance honoring the current tenant's database strategy.
/// Supports Shared, PerTenant and Hybrid strategies and falls back to shared defaults when multi-tenancy is disabled.
/// </summary>
/// <typeparam name="T">Concrete DbContext type.</typeparam>
/// <remarks>
/// IMPORTANT: The returned DbContext must be disposed by the caller.
/// Consider using 'using' statements or dependency injection scopes to ensure proper disposal.
/// </remarks>
public class TenantDbContextFactory<T> : IDbContextFactory<T> where T : DbContext, IDbContext
{
    private readonly ICurrentTenant _currentTenant;                // Current resolved tenant (may be a lightweight proxy)
    private readonly IEnumerable<IDatabaseProvider> _providers;    // Registered database providers
    private readonly IServiceProvider _sp;                         // Root service provider for context construction
    private readonly DatabaseOptions _dbOptions;                   // Default / shared database options
    private readonly MultiTenantOptions _multiTenantOptions;       // Multi-tenant feature options
    private readonly ILogger<TenantDbContextFactory<T>> _logger;   // Logger for diagnostics

    public TenantDbContextFactory(
        ICurrentTenant currentTenant, 
        IEnumerable<IDatabaseProvider> providers,
        IServiceProvider sp, 
        IOptions<DatabaseOptions> dbOptions, 
        IOptions<MultiTenantOptions> multiTenantOptions,
        ILogger<TenantDbContextFactory<T>> logger)
    {
        _currentTenant = currentTenant;
        _providers = providers;
        _sp = sp;
        _dbOptions = dbOptions.Value;
        _multiTenantOptions = multiTenantOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Creates a DbContext configured for the current tenant (or shared defaults if multi-tenancy disabled).
    /// </summary>
    /// <returns>A configured DbContext instance. Must be disposed by caller.</returns>
    public T CreateDbContext()
    {
        var tenantId = _currentTenant?.GetId() ?? default;
        var tenantIdentifier = _currentTenant?.Identifier ?? "Default";

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Creating DbContext for tenant: {TenantIdentifier} (ID: {TenantId}), DbType: {DbType}",
                tenantIdentifier,
                tenantId,
                _currentTenant?.DbType ?? TenantDbType.Shared);
        }

        var target = DetermineTarget();

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Resolved connection target - Provider: {Provider}, ConnectionString: {MaskedConnectionString}",
                target.ProviderKey,
                MaskConnectionString(target.ConnectionString));
        }

        // Build DbContextOptions for the selected provider/connection
        var options = BuildOptions(target.ConnectionString, target.ProviderKey);

        // IMPORTANT: pass the configured options instance so provider configuration is honored
        var context = ActivatorUtilities.CreateInstance<T>(_sp, options);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "DbContext created successfully for tenant: {TenantIdentifier}",
                tenantIdentifier);
        }

        return context;
    }

    #region Target Resolution

    private (string ConnectionString, string ProviderKey) DetermineTarget()
    {
        // Multi-tenancy disabled OR no tenant context -> always use shared defaults.
        if (_multiTenantOptions.IsEnabled == false || _currentTenant == null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Multi-tenancy disabled or no tenant context. Using shared database configuration.");

            return (_dbOptions.ConnectionString, _dbOptions.DbProvider);
        }

        return _currentTenant.DbType switch
        {
            TenantDbType.Shared => ResolveShared(),
            TenantDbType.PerTenant => ResolvePerTenant(),
            TenantDbType.Hybrid => ResolveHybrid(),
            _ => throw new InvalidOperationException($"Unsupported DbType: {_currentTenant.DbType}")
        };
    }

    private (string ConnectionString, string ProviderKey) ResolveShared()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Tenant uses shared database strategy.");

        return (_dbOptions.ConnectionString, _dbOptions.DbProvider);
    }

    private (string ConnectionString, string ProviderKey) ResolvePerTenant()
    {
        if (string.IsNullOrEmpty(_currentTenant.ConnectionString))
        {
            _logger.LogError(
                "Tenant {TenantIdentifier} is configured as PerTenant but has no connection string defined.",
                _currentTenant.Identifier);

            throw new InvalidOperationException("Tenant is PerTenant but has no connection string defined.");
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Tenant {TenantIdentifier} uses dedicated database (PerTenant strategy).",
                _currentTenant.Identifier);
        }

        return (_currentTenant.ConnectionString, NormalizeProvider(_currentTenant.DbProvider));
    }

    private (string ConnectionString, string ProviderKey) ResolveHybrid()
    {
        // Own DB when a connection string is provided, else shared defaults
        if (!string.IsNullOrEmpty(_currentTenant.ConnectionString))
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Tenant {TenantIdentifier} uses dedicated database (Hybrid strategy).",
                    _currentTenant.Identifier);
            }

            return (_currentTenant.ConnectionString, NormalizeProvider(_currentTenant.DbProvider));
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Tenant {TenantIdentifier} uses shared database (Hybrid strategy fallback).",
                _currentTenant.Identifier);
        }

        return (_dbOptions.ConnectionString, _dbOptions.DbProvider);
    }

    private string NormalizeProvider(string? tenantProvider)
    {
        var provider = string.IsNullOrWhiteSpace(tenantProvider)
            ? _dbOptions.DbProvider
            : tenantProvider;

        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogTrace("Normalized provider: {Provider}", provider);

        return provider;
    }

    #endregion

    #region Provider Configuration

    private DbContextOptions<T> BuildOptions(string connectionString, string providerKey)
    {
        var provider = _providers.FirstOrDefault(p => p.CanHandle(providerKey));

        if (provider == null)
        {
            _logger.LogError(
                "No database provider found for key: {ProviderKey}. Available providers: {AvailableProviders}",
                providerKey,
                string.Join(", ", _providers.Select(p => p.GetType().Name)));

            throw new NotSupportedException($"Provider '{providerKey}' not supported");
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Using database provider: {ProviderType} for key: {ProviderKey}",
                provider.GetType().Name,
                providerKey);
        }

        var builder = new DbContextOptionsBuilder<T>();
        provider.Configure(builder, connectionString, _dbOptions);

        // Optional additional standard flags from options
        if (_dbOptions.EnableDetailedErrors)
        {
            builder.EnableDetailedErrors();
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace("Detailed errors enabled");
        }

        if (_dbOptions.EnableSensitiveDataLogging)
        {
            builder.EnableSensitiveDataLogging();
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace("Sensitive data logging enabled");
        }

        return builder.Options;
    }

    #endregion

    #region Helper Methods

    private static string MaskConnectionString(string connectionString)
    {
        // Simple masking for logging - show only first 50 chars
        return connectionString.Length > 50
            ? $"{connectionString[..50]}..."
            : connectionString;
    }

    #endregion
}
