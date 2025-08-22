using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

public interface IDbContextFactory
{
    DbContext CreateDbContext(ITenant currentTenant);
}
