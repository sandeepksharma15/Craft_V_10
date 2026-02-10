using Craft.Value.DbContextFeatures;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Value.Tests.DbContextFeatures;

public class VersionTrackingFeatureTests
{
    private class TestEntity : IHasVersion
    {
        public KeyType Id { get; set; }
        public string? Name { get; set; }
        public long Version { get; set; }
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Set_Version_To_One_On_Add()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new VersionTrackingFeature();
        var entity = new TestEntity { Id = 1, Name = "Test", Version = 0 };
        
        context.TestEntities.Add(entity);

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.Equal(1, entity.Version);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Increment_Version_On_Modify()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new VersionTrackingFeature();
        var entity = new TestEntity { Id = 1, Name = "Test", Version = 3 };
        
        context.TestEntities.Add(entity);
        context.SaveChanges();

        context.Entry(entity).State = EntityState.Modified;
        entity.Name = "Modified";

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.Equal(4, entity.Version);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Handle_Multiple_New_Entities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new VersionTrackingFeature();
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Test1", Version = 0 },
            new TestEntity { Id = 2, Name = "Test2", Version = 0 }
        };
        
        context.TestEntities.AddRange(entities);

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.All(entities, e => Assert.Equal(1, e.Version));
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Handle_Multiple_Modified_Entities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new VersionTrackingFeature();
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Test1", Version = 2 },
            new TestEntity { Id = 2, Name = "Test2", Version = 5 }
        };
        
        context.TestEntities.AddRange(entities);
        context.SaveChanges();

        foreach (var entity in entities)
        {
            context.Entry(entity).State = EntityState.Modified;
            entity.Name += "_Modified";
        }

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.Equal(3, entities[0].Version);
        Assert.Equal(6, entities[1].Version);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Not_Affect_Deleted_Entities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new VersionTrackingFeature();
        var entity = new TestEntity { Id = 1, Name = "Test", Version = 5 };
        
        context.TestEntities.Add(entity);
        context.SaveChanges();

        context.TestEntities.Remove(entity);

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.Equal(5, entity.Version);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Not_Affect_Unchanged_Entities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new VersionTrackingFeature();
        var entity = new TestEntity { Id = 1, Name = "Test", Version = 3 };
        
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Entity is now unchanged
        context.Entry(entity).State = EntityState.Unchanged;

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.Equal(3, entity.Version);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Handle_Mixed_State_Entities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new VersionTrackingFeature();
        
        var newEntity = new TestEntity { Id = 1, Name = "New", Version = 0 };
        var modifiedEntity = new TestEntity { Id = 2, Name = "Existing", Version = 3 };
        
        context.TestEntities.Add(modifiedEntity);
        context.SaveChanges();

        context.TestEntities.Add(newEntity);
        context.Entry(modifiedEntity).State = EntityState.Modified;

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.Equal(1, newEntity.Version);
        Assert.Equal(4, modifiedEntity.Version);
    }
}

