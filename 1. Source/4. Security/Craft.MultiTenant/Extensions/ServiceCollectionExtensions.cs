using Craft.Core;
using Craft.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.MultiTenant;

public static class ServiceCollectionExtensions
{
    public static TenantBuilder<T> AddMultiTenant<T>(this IServiceCollection services, Action<TenantOptions> config)
        where T : class, ITenant, IEntity, new()
    {
        services.AddScoped<ITenantResolver<T>, TenantResolver<T>>();
        services.AddScoped(sp => (ITenantResolver)sp.GetRequiredService<ITenantResolver<T>>());

        services.AddSingleton<ITenantContextAccessor<T>, TenantContextAccessor<T>>();
        services.AddSingleton(sp => (ITenantContextAccessor)sp.GetRequiredService<ITenantContextAccessor<T>>());

        // Register TenantContext<T> - returns null if not available (e.g., during migrations)
        services.AddScoped(typeof(ITenantContext<T>), sp =>
            sp.GetRequiredService<ITenantContextAccessor<T>>().TenantContext!);

        // Register concrete tenant type T - returns null if not available (e.g., during migrations)
        services.AddScoped(typeof(T), sp =>
            sp.GetRequiredService<ITenantContextAccessor<T>>().TenantContext?.Tenant!);

        // Register ITenant interface - returns null if concrete type is null
        services.AddScoped(typeof(ITenant), sp => sp.GetService<T>()!);

        services.AddScoped<ICurrentTenant, CurrentTenant>();
        services.AddScoped<ICurrentTenant<KeyType>, CurrentTenant>();

        services.Configure(config);

        return new TenantBuilder<T>(services);
    }

    public static TenantBuilder<T> AddMultiTenant<T>(this IServiceCollection services)
        where T : class, ITenant, IEntity, new()
        => services.AddMultiTenant<T>(_ => { });

    public static TenantBuilder<Tenant> AddMultiTenant(this IServiceCollection services)
        => services.AddMultiTenant<Tenant>(_ => { });
}
