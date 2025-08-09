using Craft.Data;
using Craft.TestDataStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Craft.TestDataStore.Fixtures;

public class TestDbContext : IdentityDbContext<TestUser, TestRole, int>, IDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<Company>? Companies { get; set; }
    public DbSet<Country>? Countries { get; set; }
    public DbSet<Store>? Stores { get; set; }

    public DbSet<NoSoftDeleteEntity>? NoSoftDeleteEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Country>().Navigation(c => c.Companies).AutoInclude();

        _ = builder.Entity<Country>().HasData(CountrySeed.Get());
        _ = builder.Entity<Company>().HasData(CompanySeed.Get());
        _ = builder.Entity<Store>().HasData(StoreSeed.Get());
    }
}
