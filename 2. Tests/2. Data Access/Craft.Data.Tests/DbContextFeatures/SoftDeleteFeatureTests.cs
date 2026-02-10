using Craft.Data.DbContextFeatures;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.Tests.DbContextFeatures;

public class SoftDeleteFeatureTests
{
    private class TestEntity : ISoftDelete
    {
        public KeyType Id { get; set; }
        public string? Name { get; set; }
        public bool IsDeleted { get; set; }
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var feature = new SoftDeleteFeature();
            feature.ConfigureModel(modelBuilder);
        }
    }

    [Fact]
    public void ConfigureModel_Should_Apply_Query_Filter_To_Exclude_Deleted_Entities()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        using var context = new TestDbContext(options);
        context.Database.EnsureCreated();

        // Add test data
        context.TestEntities.AddRange([
            new TestEntity { Id = 1, Name = "Active", IsDeleted = false },
            new TestEntity { Id = 2, Name = "Deleted", IsDeleted = true }
        ]);
        context.SaveChanges();

        // Act
        var results = context.TestEntities.ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(1, results[0].Id);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Convert_Hard_Delete_To_Soft_Delete()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        using var context = new TestDbContext(options);
        var feature = new SoftDeleteFeature();
        var entity = new TestEntity { Id = 1, Name = "Test", IsDeleted = false };
        
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Act
        context.TestEntities.Remove(entity);
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        var entry = context.Entry(entity);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(entity.IsDeleted);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Handle_Multiple_Deletions()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        using var context = new TestDbContext(options);
        var feature = new SoftDeleteFeature();
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Test1", IsDeleted = false },
            new TestEntity { Id = 2, Name = "Test2", IsDeleted = false }
        };
        
        context.TestEntities.AddRange(entities);
        context.SaveChanges();

        // Act
        context.TestEntities.RemoveRange(entities);
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        foreach (var entity in entities)
        {
            Assert.Equal(EntityState.Modified, context.Entry(entity).State);
            Assert.True(entity.IsDeleted);
        }
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Not_Affect_Non_Deleted_Entities()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        using var context = new TestDbContext(options);
        var feature = new SoftDeleteFeature();
        var entity = new TestEntity { Id = 1, Name = "Test", IsDeleted = false };
        
        context.TestEntities.Add(entity);
        context.SaveChanges();

        entity.Name = "Modified";
        context.Entry(entity).State = EntityState.Modified;

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.Equal(EntityState.Modified, context.Entry(entity).State);
        Assert.False(entity.IsDeleted);
        Assert.Equal("Modified", entity.Name);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Handle_Multiple_Changes()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new SoftDeleteFeature();
        var entity1 = new TestEntity { Id = 1, Name = "Test1", IsDeleted = false };
        var entity2 = new TestEntity { Id = 2, Name = "Test2", IsDeleted = false };
        
        context.TestEntities.AddRange([entity1, entity2]);
        context.SaveChanges();

        context.TestEntities.RemoveRange([entity1, entity2]);

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        foreach (var entity in new[] { entity1, entity2 })
        {
            Assert.Equal(EntityState.Modified, context.Entry(entity).State);
            Assert.True(entity.IsDeleted);
        }
    }
}

