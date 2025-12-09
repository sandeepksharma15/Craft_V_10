using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.MultiTenant;

internal class TenantMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate next = next;

    public async Task Invoke(HttpContext context)
    {
        var accessor = context.RequestServices.GetRequiredService<ITenantContextAccessor>();

        if (accessor.TenantContext is null)
        {
            var tenantResolver = context.RequestServices.GetRequiredService<ITenantResolver>();
            accessor.TenantContext = await tenantResolver.ResolveAsync(context);
        }

        await next(context);
    }
}
