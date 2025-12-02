using Craft.Core;
using Microsoft.EntityFrameworkCore;

namespace Craft.Testing.Fixtures;

public class TestDbContext : DbContext, IDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<Company>? Companies { get; set; }
    public DbSet<Country>? Countries { get; set; }
    public DbSet<Store>? Stores { get; set; }
    public DbSet<TenantEntity>? TenantEntities { get; set; }

    public DbSet<NoSoftDeleteEntity>? NoSoftDeleteEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Country>().Navigation(c => c.Companies).AutoInclude();

        _ = builder.Entity<Country>().HasData(CountrySeed.Get());
        _ = builder.Entity<Company>().HasData(CompanySeed.Get());
        _ = builder.Entity<Store>().HasData(StoreSeed.Get());
        _ = builder.Entity<TenantEntity>().HasData(TenantEntitySeed.Get());
    }
}
