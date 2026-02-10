using Craft.Core;
using Craft.TestHost.Entities;
using Microsoft.EntityFrameworkCore;

namespace Craft.TestHost.Data;

/// <summary>
/// Database context for the test host application.
/// </summary>
public class TestHostDbContext : DbContext, IDbContext
{
    public TestHostDbContext(DbContextOptions<TestHostDbContext> options) : base(options) { }

    public DbSet<TestProduct> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Seed data
        modelBuilder.Entity<TestProduct>().HasData(
            new TestProduct { Id = 1, Name = "Laptop", Price = 999.99m, TenantId = 1 },
            new TestProduct { Id = 2, Name = "Phone", Price = 599.99m, TenantId = 1 },
            new TestProduct { Id = 3, Name = "Tablet", Price = 399.99m, TenantId = 2 }
        );
    }
}

