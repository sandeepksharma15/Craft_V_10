using Craft.Domain;
using Microsoft.AspNetCore.Http;

namespace Craft.MultiTenant;

public interface ITenantResolver
{
    Task<ITenantContext> ResolveAsync(HttpContext context);
}

public interface ITenantResolver<T> where T : class, ITenant, IEntity, new()
{
    IEnumerable<ITenantStore<T>> Stores { get; }
    IEnumerable<ITenantStrategy> Strategies { get; }

    Task<ITenantContext<T>> ResolveAsync(HttpContext context);
}
