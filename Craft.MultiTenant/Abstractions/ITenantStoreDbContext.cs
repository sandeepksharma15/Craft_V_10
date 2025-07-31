using Microsoft.EntityFrameworkCore;

namespace Craft.MultiTenant;

public interface ITenantStoreDbContext<T> where T : class, ITenant, new()
{
    DbSet<T> Tenants { get; set; }
}

public interface ITenantStoreDbContext : ITenantStoreDbContext<Tenant>;
