using Craft.Domain;

namespace Craft.MultiTenant;

public interface ITenantContext
{
    bool HasResolvedTenant => Tenant != null;
    ITenantStrategy Strategy { get; }
    ITenant Tenant { get; }
}

public interface ITenantContext<T> where T : class, ITenant, IEntity, new()
{
    bool HasResolvedTenant => Tenant != null;
    ITenantStore<T> Store { get; set; }
    ITenantStrategy Strategy { get; set; }
    T Tenant { get; set; }
}
