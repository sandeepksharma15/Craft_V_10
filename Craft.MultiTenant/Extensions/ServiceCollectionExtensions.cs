using Craft.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.MultiTenant;

/// <summary>
/// Extension methods for registering multi-tenant services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds multi-tenant support to the service collection.
    /// </summary>
    /// <typeparam name="T">The tenant type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="config">Tenant options configuration.</param>
    /// <returns>A <see cref="TenantBuilder{T}"/> for further configuration.</returns>
    public static TenantBuilder<T> AddMultiTenant<T>(this IServiceCollection services, Action<TenantOptions> config)
        where T : class, ITenant, IEntity, new()
    {
        // Register tenant resolver
        services.AddScoped<ITenantResolver<T>, TenantResolver<T>>();
        services.AddScoped(sp => (ITenantResolver)sp.GetRequiredService<ITenantResolver<T>>());

        // Register tenant context accessor (singleton, as it holds per-request context)
        services.AddSingleton<ITenantContextAccessor<T>, TenantContextAccessor<T>>();
        services.AddSingleton(sp => (ITenantContextAccessor)sp.GetRequiredService<ITenantContextAccessor<T>>());

        // Register tenant context (scoped, per request)
        services.AddScoped(sp =>
            sp.GetRequiredService<ITenantContextAccessor<T>>().TenantContext
            ?? throw new InvalidOperationException("TenantContext is not available in the current context."));

        // Register tenant (scoped, per request)
        services.AddScoped(sp =>
            sp.GetRequiredService<ITenantContextAccessor<T>>().TenantContext?.Tenant
            ?? throw new InvalidOperationException("Tenant is not resolved in the current context."));

        services.AddScoped<ITenant>(sp => sp.GetRequiredService<T>());

        // Configure tenant options
        services.Configure(config);

        return new TenantBuilder<T>(services);
    }

    /// <summary>
    /// Adds multi-tenant support with default options.
    /// </summary>
    public static TenantBuilder<T> AddMultiTenant<T>(this IServiceCollection services)
        where T : class, ITenant, IEntity, new()
        => services.AddMultiTenant<T>(_ => { });

    /// <summary>
    /// Adds multi-tenant support for the default Tenant type.
    /// </summary>
    public static TenantBuilder<Tenant> AddMultiTenant(this IServiceCollection services)
        => services.AddMultiTenant<Tenant>(_ => { });
}
