using Craft.Core;
using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Data;

public class TenantDbContextFactory<T> : IDbContextFactory<T> where T : DbContext, IDbContext
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IEnumerable<IDatabaseProvider> _providers;
    private readonly IServiceProvider _sp;
    private readonly DatabaseOptions _dbOptions;

    public TenantDbContextFactory(ICurrentTenant currentTenant, IEnumerable<IDatabaseProvider> providers, IServiceProvider sp, IOptions<DatabaseOptions> defaults)
    {
        _currentTenant = currentTenant;
        _providers = providers;
        _sp = sp;
        _dbOptions = defaults.Value;
    }

    public T CreateDbContext()
    {
        string connectionString;
        string dbProvider;

        // Helper local function to pick provider honoring fallback to defaults when tenant value missing/blank
        string ResolveTenantProvider(string? tenantProvider) => string.IsNullOrWhiteSpace(tenantProvider)
            ? _dbOptions.DbProvider
            : tenantProvider;

        switch (_currentTenant.DbType)
        {
            case TenantDbType.Shared:
                connectionString = _dbOptions.ConnectionString;
                dbProvider = _dbOptions.DbProvider;
                break;

            case TenantDbType.PerTenant:
                if (string.IsNullOrEmpty(_currentTenant.ConnectionString))
                    throw new InvalidOperationException("Tenant is PerTenant but has no connection string defined.");

                connectionString = _currentTenant.ConnectionString;
                dbProvider = ResolveTenantProvider(_currentTenant.DbProvider);
                break;

            case TenantDbType.Hybrid:
                if (string.IsNullOrEmpty(_currentTenant.ConnectionString) == false)
                {
                    // Tenant Has Own DB
                    connectionString = _currentTenant.ConnectionString;
                    dbProvider = ResolveTenantProvider(_currentTenant.DbProvider);
                }
                else
                {
                    // Uses Shared DB
                    connectionString = _dbOptions.ConnectionString;
                    dbProvider = _dbOptions.DbProvider;
                }
                break;

            default:
                throw new InvalidOperationException($"Unsupported DbType: {_currentTenant.DbType}");
        }

        var provider = _providers.FirstOrDefault(p => p.CanHandle(dbProvider))
                ?? throw new NotSupportedException($"Provider '{dbProvider}' not supported");

        var builder = new DbContextOptionsBuilder<T>();
        provider.Configure(builder, connectionString, _dbOptions);

        return ActivatorUtilities.CreateInstance<T>(_sp);
    }
}
