using Craft.Data.DbContextFeatures;
using Craft.Domain;
using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.Tests.DbContextFeatures;

public class MultiTenancyFeatureTests
{
    private class TestEntity : IHasTenant
    {
        public KeyType Id { get; set; }
        public string? Name { get; set; }
        public KeyType TenantId { get; set; }
    }

    private class FakeTenant : ITenant
    {
        public KeyType Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string DbProvider { get; set; } = string.Empty;
        public TenantType Type { get; set; }
        public string LogoUri { get; set; } = string.Empty;
        public DateTime ValidUpTo { get; set; }
        public bool IsActive { get; set; }
        public TenantDbType DbType { get; set; }
        public bool IsDeleted { get; set; }
        public string? ConcurrencyStamp { get; set; }
    }

    private class TestDbContext : DbContext
    {
        private readonly ITenant _tenant;

        public TestDbContext(DbContextOptions<TestDbContext> options, ITenant tenant) : base(options)
        {
            _tenant = tenant;
        }

        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var feature = new MultiTenancyFeature(_tenant);
            feature.ConfigureModel(modelBuilder);
        }
    }

    [Fact]
    public void ConfigureModel_Should_Apply_Query_Filter_To_Current_Tenant()
    {
        // Arrange
        var dbName = $"TenantFilterTest_{Guid.NewGuid()}"; // More unique name
        var tenant1 = new FakeTenant { Id = 1 };
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging() // Help with debugging
            .Options;

        using var context = new TestDbContext(options, tenant1);
        context.Database.EnsureCreated();
        
        // Verify the model has the query filter applied
        var entityType = context.Model.FindEntityType(typeof(TestEntity));
        var queryFilter = entityType?.GetDeclaredQueryFilters();
        Assert.NotNull(queryFilter); // Ensure filter is applied
        
        // Add test data for multiple tenants
        // The OnBeforeSaveChanges would set tenant ID, but we're explicitly setting it here
        context.TestEntities.AddRange([
            new TestEntity { Id = 1, Name = "Tenant1-A", TenantId = 1 },
            new TestEntity { Id = 2, Name = "Tenant1-B", TenantId = 1 },
            new TestEntity { Id = 3, Name = "Tenant2-A", TenantId = 2 }
        ]);
        context.SaveChanges();

        // Clear change tracker to ensure we're querying from "database"
        context.ChangeTracker.Clear();

        // Act - Query without IgnoreQueryFilters to see filtered results
        var filteredResults = context.TestEntities.ToList();
        
        // Query WITH IgnoreQueryFilters to see all data
        var allResults = context.TestEntities.IgnoreQueryFilters().ToList();

        // Assert
        Assert.Equal(3, allResults.Count); // All data is in the database
        Assert.Equal(2, filteredResults.Count); // Only tenant 1 data is returned by default
        Assert.All(filteredResults, e => Assert.Equal((KeyType)1, e.TenantId));
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Set_TenantId_On_Add()
    {
        // Arrange
        var tenant = new FakeTenant { Id = 5 };
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options, tenant);
        var feature = new MultiTenancyFeature(tenant);
        var entity = new TestEntity { Id = 1, Name = "Test", TenantId = 0 };
        
        context.TestEntities.Add(entity);

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.Equal((KeyType)5, entity.TenantId);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Set_TenantId_For_Multiple_New_Entities()
    {
        // Arrange
        var tenant = new FakeTenant { Id = 3 };
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options, tenant);
        var feature = new MultiTenancyFeature(tenant);
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Test1", TenantId = 0 },
            new TestEntity { Id = 2, Name = "Test2", TenantId = 0 }
        };
        
        context.TestEntities.AddRange(entities);

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.All(entities, e => Assert.Equal((KeyType)3, e.TenantId));
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Not_Affect_Modified_Entities()
    {
        // Arrange
        var tenant = new FakeTenant { Id = 5 };
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options, tenant);
        var feature = new MultiTenancyFeature(tenant);
        var entity = new TestEntity { Id = 1, Name = "Test", TenantId = 3 };
        
        context.TestEntities.Add(entity);
        context.SaveChanges();

        context.Entry(entity).State = EntityState.Modified;
        entity.Name = "Modified";

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.Equal((KeyType)3, entity.TenantId); // Should not change
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Not_Affect_Deleted_Entities()
    {
        // Arrange
        var tenant = new FakeTenant { Id = 5 };
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options, tenant);
        var feature = new MultiTenancyFeature(tenant);
        var entity = new TestEntity { Id = 1, Name = "Test", TenantId = 3 };
        
        context.TestEntities.Add(entity);
        context.SaveChanges();

        context.TestEntities.Remove(entity);

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.Equal((KeyType)3, entity.TenantId);
    }

    [Fact]
    public void Constructor_Should_Store_CurrentTenant()
    {
        // Arrange
        var tenant = new FakeTenant { Id = 7 };

        // Act
        var feature = new MultiTenancyFeature(tenant);

        // Assert - verify by using the feature
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options, tenant);
        var entity = new TestEntity { Id = 1, Name = "Test", TenantId = 0 };
        context.TestEntities.Add(entity);

        feature.OnBeforeSaveChanges(context, 1);
        Assert.Equal((KeyType)7, entity.TenantId);
    }
}
