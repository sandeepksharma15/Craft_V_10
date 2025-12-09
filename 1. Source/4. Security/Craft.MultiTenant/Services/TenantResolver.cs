using Craft.Domain;
using Craft.MultiTenant.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Craft.MultiTenant;

public class TenantResolver<T> : ITenantResolver<T>, ITenantResolver where T : class, ITenant, IEntity, new()
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly IOptionsMonitor<TenantOptions> _options;

    public IEnumerable<ITenantStore<T>> Stores { get; }
    public IEnumerable<ITenantStrategy> Strategies { get; }

    public TenantResolver(IEnumerable<ITenantStrategy> strategies,
        IEnumerable<ITenantStore<T>> stores,
        IOptionsMonitor<TenantOptions> options) : this(strategies, stores, options, null!) { }

    public TenantResolver(IEnumerable<ITenantStrategy> strategies, IEnumerable<ITenantStore<T>> stores,
        IOptionsMonitor<TenantOptions> options, ILoggerFactory? loggerFactory)
    {
        Strategies = strategies.OrderByDescending(s => s.Priority);
        Stores = stores;
        _options = options;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
    }

    public async Task<ITenantContext<T>?> ResolveAsync(HttpContext context)
    {
        ITenantContext<T>? result = null;
        T? tenant = null;
        var ignoredIdentifiers = _options.CurrentValue.IgnoredIdentifiers;

        foreach (var strategy in Strategies)
        {
            var _strategy = new StrategyWrapper(strategy, _loggerFactory?.CreateLogger(strategy.GetType()) ?? NullLogger.Instance);
            var identifier = await _strategy.GetIdentifierAsync(context);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Identifier \"{Identifier}\" found  using strategy {Strategy}",
                                            identifier, strategy.GetType().Name);

            if (identifier is not null && ignoredIdentifiers.Contains(identifier, StringComparer.OrdinalIgnoreCase))
            {
                identifier = null;

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("Identifier \"{Identifier}\" is ignored for request at {RequestPath}",
                                       identifier, context.Request.Path);
            }

            if (identifier is not null)
                foreach (var store in Stores)
                {
                    var _store = new StoreWrapper<T>(store, _loggerFactory?.CreateLogger(store.GetType()) ?? NullLogger.Instance);
                    tenant = await _store.GetByIdentifierAsync(identifier);

                    if (tenant is not null)
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.LogDebug("Tenant \"{Tenant}\" found in store {Store} for identifier \"{Identifier}\"",
                            tenant?.Name, store.GetType().Name, identifier);

                        result = new TenantContext<T>(tenant, store, strategy);

                        await _options.CurrentValue.Events.OnTenantResolved(new ResolvedContext(
                                context, tenant, strategy.GetType(), store.GetType()));
                        break;
                    }
                }

            if (result is not null)
                break;

            if (identifier is not null && tenant is null)
                await _options.CurrentValue.Events.OnTenantNotResolved(new NotResolvedContext(context, identifier));
        }

        return result;
    }

    async Task<ITenantContext?> ITenantResolver.ResolveAsync(HttpContext context)
    {
        return await ResolveAsync(context) as ITenantContext;
    }
}
