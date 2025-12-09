using Craft.Domain;

namespace Craft.MultiTenant;

public class TenantContext<T> : ITenantContext<T>, ITenantContext where T : class, ITenant, IEntity, new()
{
    public ITenantStore<T>? Store { get; set; }

    public ITenantStrategy? Strategy { get; set; }

    public T? Tenant { get; set; }

    ITenant? ITenantContext.Tenant => Tenant;

    public TenantContext() { }

    public TenantContext(T? tenant, ITenantStore<T>? store, ITenantStrategy? strategy)
    {
        Tenant = tenant;
        Store = store;
        Strategy = strategy;
    }
}

public class TenantContext : TenantContext<Tenant>;
