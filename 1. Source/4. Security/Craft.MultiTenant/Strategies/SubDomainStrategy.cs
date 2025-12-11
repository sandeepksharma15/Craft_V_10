using Microsoft.AspNetCore.Http;

namespace Craft.MultiTenant;

public class SubDomainStrategy : ITenantStrategy
{
    public Task<string?> GetIdentifierAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        string? _identifier = null;

        var _hostList = context.Request.Host.Host.Split('.');

        if (_hostList.Length > 2)
            _identifier = _hostList[0].ToLower();

        return Task.FromResult(_identifier);
    }
}
