using Craft.Core;
using Craft.Domain;
using Craft.MultiTenant;

namespace Craft.Hosting.Extensions;

/// <summary>
/// Extension methods for configuring multi-tenancy services.
/// </summary>
public static class MultiTenantExtensions
{
    /// <summary>
    /// Adds multi-tenant services with custom tenant type and configuration.
    /// </summary>
    /// <typeparam name="T">The tenant entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="config">Action to configure tenant options.</param>
    /// <returns>A TenantBuilder for additional configuration.</returns>
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

    /// <summary>
    /// Adds multi-tenant services with custom tenant type and default configuration.
    /// </summary>
    /// <typeparam name="T">The tenant entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>A TenantBuilder for additional configuration.</returns>
    public static TenantBuilder<T> AddMultiTenant<T>(this IServiceCollection services)
        where T : class, ITenant, IEntity, new()
        => services.AddMultiTenant<T>(_ => { });

    /// <summary>
    /// Adds multi-tenant services with default Tenant type.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>A TenantBuilder for additional configuration.</returns>
    public static TenantBuilder<Tenant> AddMultiTenant(this IServiceCollection services)
        => services.AddMultiTenant<Tenant>(_ => { });
}
