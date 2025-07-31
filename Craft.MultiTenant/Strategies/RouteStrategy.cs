using Microsoft.AspNetCore.Http;

namespace Craft.MultiTenant;

public class RouteStrategy(string tenantKey = TenantConstants.TenantToken) : ITenantStrategy
{
    private readonly string _tenantKey = tenantKey;

    public Task<string?> GetIdentifierAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        context.Request.RouteValues.TryGetValue(_tenantKey, out object? identifier);

        return Task.FromResult(identifier as string);
    }
}
