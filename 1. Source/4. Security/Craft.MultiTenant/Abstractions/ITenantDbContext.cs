using Microsoft.EntityFrameworkCore;

namespace Craft.MultiTenant;

public interface ITenantDbContext<T> where T : class, ITenant, new()
{
    DbSet<T> Tenants { get; set; }
}

public interface ITenantDbContext : ITenantDbContext<Tenant>;
