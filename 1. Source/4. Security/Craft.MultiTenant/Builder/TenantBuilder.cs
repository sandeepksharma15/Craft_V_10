using Craft.Domain;
using Craft.MultiTenant.Stores;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Craft.MultiTenant;

public class TenantBuilder<T>(IServiceCollection services) where T : class, ITenant, IEntity, new()
{
    public IServiceCollection _services = services;

    public TenantBuilder<T> WithBasePathStrategy()
    {
        return WithBasePathStrategy(configureOptions => configureOptions.UpdateRequestPath = true);
    }

    public TenantBuilder<T> WithBasePathStrategy(Action<BasePathStrategyOptions> configureOptions)
    {
        _services.Configure(configureOptions);

        _services.Configure<TenantOptions>(options =>
        {
            var origOnTenantResolved = options.Events.OnTenantResolved;

            options.Events.OnTenantResolved = tenantResolvedContext =>
            {
                var httpContext = tenantResolvedContext.Context as HttpContext ??
                                  throw new MultiTenantException("BasePathStrategy expects HttpContext.");

                if (tenantResolvedContext.StrategyType == typeof(BasePathStrategy) &&
                    httpContext.RequestServices.GetRequiredService<IOptions<BasePathStrategyOptions>>().Value
                        .UpdateRequestPath)
                {
                    httpContext.Request.Path.StartsWithSegments($"/{tenantResolvedContext.Tenant?.Identifier}",
                        out var matched, out var newPath);
                    httpContext.Request.PathBase = httpContext.Request.PathBase.Add(matched);
                    httpContext.Request.Path = newPath;
                }

                return origOnTenantResolved(tenantResolvedContext);
            };
        });

        return WithStrategy<BasePathStrategy>(ServiceLifetime.Singleton);
    }

    public TenantBuilder<T> WithCacheStore()
    {
        return WithStore<CacheStore<T>>(ServiceLifetime.Singleton);
    }

    public TenantBuilder<T> WithClaimStrategy(string tenantKey = TenantConstants.TenantToken,
        string authenticationScheme = "Bearer")
    {
        _services.ConfigureAll<CookieAuthenticationOptions>(options =>
        {
            var origOnValidatePrincipal = options.Events.OnValidatePrincipal;

            options.Events.OnValidatePrincipal = async context =>
            {
                if (context.HttpContext.Items.ContainsKey($"{TenantConstants.TenantToken}__bypass_validate_principal__"))
                    return;

                await origOnValidatePrincipal(context);
            };
        });

        return WithStrategy<ClaimStrategy>(ServiceLifetime.Singleton, tenantKey, authenticationScheme);
    }

    public TenantBuilder<T> WithConfigurationStore()
        => WithStore<ConfigurationStore<T>>(ServiceLifetime.Singleton);

    public TenantBuilder<T> WithConfigurationStore(IConfiguration configuration, string sectionName)
        => WithStore<ConfigurationStore<T>>(ServiceLifetime.Singleton, configuration, sectionName);

    public TenantBuilder<T> WithDelegateStrategy(Func<object, Task<string>> doStrategy)
    {
        ArgumentNullException.ThrowIfNull(doStrategy, nameof(doStrategy));

        return WithStrategy<DelegateStrategy>(ServiceLifetime.Singleton, [doStrategy]);
    }

    public TenantBuilder<T> WithEfCoreStore<TEfCoreStoreDbContext>()
        where TEfCoreStoreDbContext : DbContext, ITenantDbContext<T>
    {
        _services.AddDbContext<TEfCoreStoreDbContext>();
        return WithStore<DbStore<TEfCoreStoreDbContext, T>>(ServiceLifetime.Scoped);
    }

    public TenantBuilder<T> WithHeaderStrategy(string tenantKey = TenantConstants.TenantToken)
    {
        return WithStrategy<HeaderStrategy>(ServiceLifetime.Singleton, tenantKey);
    }

    public TenantBuilder<T> WithHostStrategy(string template = TenantConstants.TenantToken)
    {
        return WithStrategy<HostStrategy>(ServiceLifetime.Singleton, template);
    }

    public TenantBuilder<T> WithInMemoryStore()
    {
        return WithInMemoryStore(_ => { });
    }

    public TenantBuilder<T> WithInMemoryStore(Action<InMemoryStoreOptions<T>> config)
    {
        _services.Configure(config);

        return WithStore<InMemoryStore<T>>(ServiceLifetime.Singleton);
    }

    public TenantBuilder<T> WithQueryStringStrategy(string queryStringKey)
    {
        return WithDelegateStrategy(context =>
        {
            if (context is not HttpContext httpContext)
                return Task.FromResult((string?)null!);

            httpContext.Request.Query.TryGetValue(queryStringKey, out StringValues tenantIdParam);

            return Task.FromResult(tenantIdParam.ToString());
        });
    }

    public TenantBuilder<T> WithRemoteApiStore(string endpointTemplate)
    {
        return WithRemoteApiStore(endpointTemplate, null!);
    }

    public TenantBuilder<T> WithRemoteApiStore(string endpointTemplate, Action<IHttpClientBuilder> clientConfig)
    {
        var httpClientBuilder = _services.AddHttpClient(typeof(RemoteApiStoreClient<T>).FullName!);

        clientConfig?.Invoke(httpClientBuilder);

        _services.TryAddSingleton<RemoteApiStoreClient<T>>();

        return WithStore<RemoteApiStore<T>>(ServiceLifetime.Singleton, endpointTemplate);
    }

    public TenantBuilder<T> WithRouteStrategy(string tenantKey = TenantConstants.TenantToken)
    {
        return WithStrategy<RouteStrategy>(ServiceLifetime.Singleton, tenantKey);
    }

    public TenantBuilder<T> WithSessionStrategy(string tenantKey = TenantConstants.TenantToken)
    {
        return WithStrategy<SessionStrategy>(ServiceLifetime.Singleton, tenantKey);
    }

    public TenantBuilder<T> WithStaticStrategy(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier, nameof(identifier));

        return WithStrategy<StaticStrategy>(ServiceLifetime.Singleton, [identifier]);
    }

    public TenantBuilder<T> WithStore<TStore>(ServiceLifetime lifetime)
        where TStore : ITenantStore<T>
    {
        _services.Add(ServiceDescriptor.Describe(typeof(ITenantStore<T>), typeof(TStore), lifetime));
        return this;
    }

    public TenantBuilder<T> WithStore<TStore>(ServiceLifetime lifetime, params object[] parameters)
        where TStore : ITenantStore<T>
        => WithStore(lifetime, sp => ActivatorUtilities.CreateInstance<TStore>(sp, parameters));

    public TenantBuilder<T> WithStore<TStore>(ServiceLifetime lifetime, Func<IServiceProvider, TStore> factory)
        where TStore : ITenantStore<T>
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        _services.Add(ServiceDescriptor.Describe(typeof(ITenantStore<T>),
            sp => factory(sp),
            lifetime));

        return this;
    }

    public TenantBuilder<T> WithStrategy<TStrategy>(ServiceLifetime lifetime, params object[] parameters)
        where TStrategy : ITenantStrategy
    {
        return WithStrategy(lifetime, sp => ActivatorUtilities.CreateInstance<TStrategy>(sp, parameters));
    }

    public TenantBuilder<T> WithStrategy<TStrategy>(ServiceLifetime lifetime, Func<IServiceProvider, TStrategy> factory)
        where TStrategy : ITenantStrategy
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        _services.Add(ServiceDescriptor.Describe(typeof(ITenantStrategy),
            sp => factory(sp),
            lifetime));

        return this;
    }

    public TenantBuilder<T> WithSubDomainStrategy()
    {
        return WithStrategy<SubDomainStrategy>(ServiceLifetime.Singleton);
    }
}
