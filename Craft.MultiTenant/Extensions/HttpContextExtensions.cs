using Craft.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.MultiTenant;

public static class HttpContextExtensions
{
    public static TenantContext<T> GetTenantContext<T>(this HttpContext httpContext)
        where T : class, ITenant, IEntity, new()
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        var services = httpContext.RequestServices;

        var context = services.GetRequiredService<ITenantContextAccessor<T>>();

        return context?.TenantContext is not TenantContext<T> tenantContext || tenantContext is null
            ? throw new InvalidOperationException($"TenantContext<{typeof(T).Name}> is not available in the current HttpContext.")
            : tenantContext;
    }

    public static ITenantContext GetTenantContext(this HttpContext httpContext)
        => httpContext.GetTenantContext<Tenant>();

    public static bool SetTenant<T>(this HttpContext httpContext, T tenant, bool resetServiceProviderScope)
            where T : class, ITenant, IEntity, new()
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        if (resetServiceProviderScope)
            httpContext.RequestServices = httpContext.RequestServices.CreateScope().ServiceProvider;

        var tenantContext = new TenantContext<T>
        {
            Tenant = tenant,
            Strategy = null,
            Store = null
        };

        var accessor = httpContext.RequestServices.GetRequiredService<ITenantContextAccessor<T>>();
        accessor.TenantContext = tenantContext;

        return true;
    }

    public static bool SetTenant(this HttpContext httpContext, Tenant tenant, bool resetServiceProviderScope)
        => httpContext.SetTenant<Tenant>(tenant, resetServiceProviderScope);
}
