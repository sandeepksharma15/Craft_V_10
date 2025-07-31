using Microsoft.AspNetCore.Builder;

namespace Craft.MultiTenant;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMultiTenant(this IApplicationBuilder builder)
        => builder.UseMiddleware<TenantMiddleware>();
}
