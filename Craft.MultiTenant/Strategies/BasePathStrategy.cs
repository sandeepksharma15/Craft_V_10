using Microsoft.AspNetCore.Http;

namespace Craft.MultiTenant;

public class BasePathStrategy : ITenantStrategy
{
    public Task<string?> GetIdentifierAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var path = context.Request.Path;

        var pathSegments = path
            .Value?
            .Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if (pathSegments is null || pathSegments.Length == 0)
            return Task.FromResult<string?>(null);

        return Task.FromResult<string?>(pathSegments[0]);
    }
}
