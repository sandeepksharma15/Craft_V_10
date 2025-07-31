using Microsoft.AspNetCore.Http;

namespace Craft.MultiTenant;

public interface ITenantStrategy
{
    int Priority => 0;

    Task<string?> GetIdentifierAsync(HttpContext context);
}
