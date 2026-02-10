using Craft.Auditing;
using Craft.Value.DbContextFeatures;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Value.Tests.DbContextFeatures;

[Audit]
public class AuditableTestEntity : BaseEntity
{
    public string? Name { get; set; }
}

public class NonAuditableTestEntity : BaseEntity
{
    public string? Description { get; set; }
}

public class AuditTrailFeatureTests
{
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<AuditableTestEntity> AuditableEntities => Set<AuditableTestEntity>();
        public DbSet<NonAuditableTestEntity> NonAuditableEntities => Set<NonAuditableTestEntity>();
        public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var feature = new AuditTrailFeature();
            feature.ConfigureModel(modelBuilder);
        }
    }

    [Fact]
    public void RequiresDbSet_Should_Return_True()
    {
        // Arrange
        var feature = new AuditTrailFeature();

        // Assert
        Assert.True(feature.RequiresDbSet);
    }

    [Fact]
    public void EntityType_Should_Return_AuditTrail()
    {
        // Arrange
        var feature = new AuditTrailFeature();

        // Assert
        Assert.Equal(typeof(AuditTrail), feature.EntityType);
    }

    [Fact]
    public void ConfigureModel_Should_Configure_AuditTrail_Entity()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(AuditTrail));

        // Assert
        Assert.NotNull(entityType);
        
        // Check indexes
        var indexes = entityType.GetIndexes().ToList();
        Assert.Contains(indexes, i => i.Properties.Any(p => p.Name == nameof(AuditTrail.TableName)));
        Assert.Contains(indexes, i => i.Properties.Any(p => p.Name == nameof(AuditTrail.DateTimeUTC)));
        Assert.Contains(indexes, i => i.Properties.Any(p => p.Name == nameof(AuditTrail.UserId)));
        Assert.Contains(indexes, i => i.Properties.Any(p => p.Name == nameof(AuditTrail.ChangeType)));
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Create_AuditTrail_For_Added_Entity()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new AuditTrailFeature();
        var userId = (KeyType)123;

        var entity = new AuditableTestEntity { Id = 1, Name = "Test" };
        context.AuditableEntities.Add(entity);

        // Act
        feature.OnBeforeSaveChanges(context, userId);

        // Assert
        var auditTrails = context.ChangeTracker.Entries<AuditTrail>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        Assert.Single(auditTrails);
        Assert.Equal(userId, auditTrails[0].UserId);
        Assert.Equal(EntityChangeType.Created, auditTrails[0].ChangeType);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Create_AuditTrail_For_Modified_Entity()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new AuditTrailFeature();
        var userId = (KeyType)456;

        var entity = new AuditableTestEntity { Id = 1, Name = "Original" };
        context.AuditableEntities.Add(entity);
        context.SaveChanges();

        entity.Name = "Modified";
        context.Entry(entity).State = EntityState.Modified;

        // Act
        feature.OnBeforeSaveChanges(context, userId);

        // Assert
        var auditTrails = context.ChangeTracker.Entries<AuditTrail>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        Assert.Single(auditTrails);
        Assert.Equal(userId, auditTrails[0].UserId);
        Assert.Equal(EntityChangeType.Updated, auditTrails[0].ChangeType);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Create_AuditTrail_For_Deleted_Entity()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new AuditTrailFeature();
        var userId = (KeyType)789;

        var entity = new AuditableTestEntity { Id = 1, Name = "ToDelete" };
        context.AuditableEntities.Add(entity);
        context.SaveChanges();

        context.AuditableEntities.Remove(entity);

        // Act
        feature.OnBeforeSaveChanges(context, userId);

        // Assert
        var auditTrails = context.ChangeTracker.Entries<AuditTrail>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        Assert.Single(auditTrails);
        Assert.Equal(userId, auditTrails[0].UserId);
        Assert.Equal(EntityChangeType.Deleted, auditTrails[0].ChangeType);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Not_Create_AuditTrail_For_Non_Auditable_Entity()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new AuditTrailFeature();
        var userId = (KeyType)111;

        var entity = new NonAuditableTestEntity { Id = 1, Description = "Test" };
        context.NonAuditableEntities.Add(entity);

        // Act
        feature.OnBeforeSaveChanges(context, userId);

        // Assert
        var auditTrails = context.ChangeTracker.Entries<AuditTrail>()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        Assert.Empty(auditTrails);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Not_Audit_AuditTrail_Itself()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new AuditTrailFeature();
        var userId = (KeyType)222;

        var auditTrail = new AuditTrail 
        { 
            UserId = userId,
            ChangeType = EntityChangeType.Created,
            TableName = "Test"
        };
        context.AuditTrails.Add(auditTrail);

        // Act
        feature.OnBeforeSaveChanges(context, userId);

        // Assert
        var newAuditTrails = context.ChangeTracker.Entries<AuditTrail>()
            .Where(e => e.State == EntityState.Added && e.Entity != auditTrail)
            .ToList();

        Assert.Empty(newAuditTrails);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Handle_Multiple_Changes()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new AuditTrailFeature();
        var userId = (KeyType)333;

        var entity1 = new AuditableTestEntity { Id = 1, Name = "Test1" };
        var entity2 = new AuditableTestEntity { Id = 2, Name = "Test2" };
        context.AuditableEntities.AddRange([entity1, entity2]);

        // Act
        feature.OnBeforeSaveChanges(context, userId);

        // Assert
        var auditTrails = context.ChangeTracker.Entries<AuditTrail>()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        Assert.Equal(2, auditTrails.Count);
    }
}

