// builder.Services.AddMultiTenancy<Tenant>().WithDelegateStrategy(_ => Task.FromResult("localhost"));

using Microsoft.AspNetCore.Http;

namespace Craft.MultiTenant;

public class DelegateStrategy(Func<object, Task<string?>> doStrategy) : ITenantStrategy
{
    private readonly Func<object, Task<string?>> _doStrategy = doStrategy;

    public Task<string?> GetIdentifierAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        return _doStrategy(context);
    }
}
