using Microsoft.EntityFrameworkCore;

namespace Craft.Data;


public class BaseDbContext<TContext> : DbContext where TContext : DbContext
{
    protected BaseDbContext(DbContextOptions<TContext> options) : base(options) { }


}
