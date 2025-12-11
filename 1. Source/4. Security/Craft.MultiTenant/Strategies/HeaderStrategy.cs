using Microsoft.AspNetCore.Http;

namespace Craft.MultiTenant;

public class HeaderStrategy(string headerKey = TenantConstants.TenantToken) : ITenantStrategy
{
    private readonly string _headerKey = headerKey;

    public Task<string?> GetIdentifierAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        return Task.FromResult<string?>(context?.Request?.Headers[_headerKey].FirstOrDefault());
    }
}
