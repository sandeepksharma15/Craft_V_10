using Craft.Data.DbContextFeatures;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.Tests.DbContextFeatures;

public class ConcurrencyFeatureTests
{
    private class TestEntity : IHasConcurrency
    {
        public KeyType Id { get; set; }
        public string? Name { get; set; }
        public string? ConcurrencyStamp { get; set; }
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var feature = new ConcurrencyFeature();
            feature.ConfigureModel(modelBuilder);
        }
    }

    [Fact]
    public void ConfigureModel_Should_Set_ConcurrencyStamp_As_ConcurrencyToken()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(TestEntity));
        var property = entityType?.FindProperty(nameof(IHasConcurrency.ConcurrencyStamp));

        // Assert
        Assert.NotNull(property);
        Assert.True(property!.IsConcurrencyToken);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Set_ConcurrencyStamp_On_Add()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new ConcurrencyFeature();
        var entity = new TestEntity { Id = 1, Name = "Test" };
        
        context.TestEntities.Add(entity);

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.NotNull(entity.ConcurrencyStamp);
        Assert.NotEmpty(entity.ConcurrencyStamp);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Update_ConcurrencyStamp_On_Modify()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new ConcurrencyFeature();
        var entity = new TestEntity { Id = 1, Name = "Test", ConcurrencyStamp = "original" };
        
        context.TestEntities.Add(entity);
        context.SaveChanges();

        context.Entry(entity).State = EntityState.Modified;
        entity.Name = "Modified";

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.NotNull(entity.ConcurrencyStamp);
        Assert.NotEqual("original", entity.ConcurrencyStamp);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Generate_Different_Stamps_For_Multiple_Entities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new ConcurrencyFeature();
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Test1" },
            new TestEntity { Id = 2, Name = "Test2" }
        };
        
        context.TestEntities.AddRange(entities);

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.NotNull(entities[0].ConcurrencyStamp);
        Assert.NotNull(entities[1].ConcurrencyStamp);
        Assert.NotEqual(entities[0].ConcurrencyStamp, entities[1].ConcurrencyStamp);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Not_Affect_Deleted_Entities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new ConcurrencyFeature();
        var entity = new TestEntity { Id = 1, Name = "Test", ConcurrencyStamp = "original" };
        
        context.TestEntities.Add(entity);
        context.SaveChanges();

        context.TestEntities.Remove(entity);

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.Equal("original", entity.ConcurrencyStamp);
    }

    [Fact]
    public void OnBeforeSaveChanges_Should_Generate_Valid_Guid_Strings()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var feature = new ConcurrencyFeature();
        var entity = new TestEntity { Id = 1, Name = "Test" };
        
        context.TestEntities.Add(entity);

        // Act
        feature.OnBeforeSaveChanges(context, 1);

        // Assert
        Assert.True(Guid.TryParse(entity.ConcurrencyStamp, out _));
    }
}
