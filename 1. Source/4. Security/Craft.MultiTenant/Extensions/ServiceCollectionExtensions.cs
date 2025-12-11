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

        services.AddScoped(sp =>
            sp.GetRequiredService<ITenantContextAccessor<T>>().TenantContext
            ?? throw new InvalidOperationException("TenantContext is not available in the current context."));

        services.AddScoped(sp =>
            sp.GetRequiredService<ITenantContextAccessor<T>>().TenantContext?.Tenant
            ?? throw new InvalidOperationException("Tenant is not resolved in the current context."));

        services.AddScoped<ITenant>(sp => sp.GetRequiredService<T>());

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
