using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Moq;

namespace Craft.Data.Tests.DbContexts;

public class DbContextFeatureCollectionTests
{
    #region AddFeature Tests

    [Fact]
    public void AddFeature_WithInstance_Should_Add_Feature_And_Return_Collection()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();
        var feature = new Mock<IDbContextFeature>().Object;

        // Act
        var result = collection.AddFeature(feature);

        // Assert
        Assert.Same(collection, result);
        Assert.Single(collection);
        Assert.Contains(feature, collection);
    }

    [Fact]
    public void AddFeature_WithType_Should_Create_And_Add_Feature_And_Return_Collection()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();

        // Act
        var result = collection.AddFeature<TestFeature>();

        // Assert
        Assert.Same(collection, result);
        Assert.Single(collection);
        Assert.IsType<TestFeature>(collection[0]);
    }

    [Fact]
    public void AddFeature_Multiple_Should_Add_All_Features()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();
        var feature1 = new Mock<IDbContextFeature>().Object;
        var feature2 = new Mock<IDbContextFeature>().Object;

        // Act
        collection.AddFeature(feature1).AddFeature(feature2);

        // Assert
        Assert.Equal(2, collection.Count);
        Assert.Contains(feature1, collection);
        Assert.Contains(feature2, collection);
    }

    #endregion

    #region ApplyConventions Tests

    [Fact]
#pragma warning disable EF1001
    public void ApplyConventions_Should_Call_ConfigureConventions_On_All_Features()
    {
        // Arrange
        var conventionSet = new ConventionSet();
        var builder = new ModelConfigurationBuilder(conventionSet, null!);
        var feature1 = new Mock<IDbContextFeature>();
        var feature2 = new Mock<IDbContextFeature>();
        
        var collection = new DbContextFeatureCollection();
        collection.AddFeature(feature1.Object).AddFeature(feature2.Object);

        // Act
        collection.ApplyConventions(builder);

        // Assert
        feature1.Verify(f => f.ConfigureConventions(builder), Times.Once);
        feature2.Verify(f => f.ConfigureConventions(builder), Times.Once);
    }

    [Fact]
    public void ApplyConventions_With_Empty_Collection_Should_Not_Throw()
    {
        // Arrange
        var conventionSet = new ConventionSet();
        var builder = new ModelConfigurationBuilder(conventionSet, null!);
        var collection = new DbContextFeatureCollection();

        // Act & Assert
        collection.ApplyConventions(builder);
    }
#pragma warning restore EF1001

    #endregion

    #region ApplyModel Tests

    [Fact]
    public void ApplyModel_Should_Call_ConfigureModel_On_All_Features()
    {
        // Arrange
        var conventionSet = new ConventionSet();
        var builder = new ModelBuilder(conventionSet);
        var feature1 = new Mock<IDbContextFeature>();
        var feature2 = new Mock<IDbContextFeature>();
        
        var collection = new DbContextFeatureCollection();
        collection.AddFeature(feature1.Object).AddFeature(feature2.Object);

        // Act
        collection.ApplyModel(builder);

        // Assert
        feature1.Verify(f => f.ConfigureModel(builder), Times.Once);
        feature2.Verify(f => f.ConfigureModel(builder), Times.Once);
    }

    [Fact]
    public void ApplyModel_With_Empty_Collection_Should_Not_Throw()
    {
        // Arrange
        var conventionSet = new ConventionSet();
        var builder = new ModelBuilder(conventionSet);
        var collection = new DbContextFeatureCollection();

        // Act & Assert
        collection.ApplyModel(builder);
    }

    #endregion

    #region BeforeSaveChanges Tests

    [Fact]
    public void BeforeSaveChanges_Should_Call_OnBeforeSaveChanges_On_All_Features()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DbContext(options);
        var userId = (KeyType)123;

        var feature1 = new Mock<IDbContextFeature>();
        var feature2 = new Mock<IDbContextFeature>();
        
        var collection = new DbContextFeatureCollection();
        collection.AddFeature(feature1.Object).AddFeature(feature2.Object);

        // Act
        collection.BeforeSaveChanges(context, userId);

        // Assert
        feature1.Verify(f => f.OnBeforeSaveChanges(context, userId), Times.Once);
        feature2.Verify(f => f.OnBeforeSaveChanges(context, userId), Times.Once);
    }

    [Fact]
    public void BeforeSaveChanges_With_Empty_Collection_Should_Not_Throw()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DbContext(options);
        var userId = (KeyType)123;
        var collection = new DbContextFeatureCollection();

        // Act & Assert
        collection.BeforeSaveChanges(context, userId);
    }

    #endregion

    #region GetRequiredDbSetTypes Tests

    [Fact]
    public void GetRequiredDbSetTypes_Should_Return_Empty_When_No_Features_Require_DbSet()
    {
        // Arrange
        var feature = new Mock<IDbContextFeature>();
        feature.SetupGet(f => f.RequiresDbSet).Returns(false);
        
        var collection = new DbContextFeatureCollection();
        collection.AddFeature(feature.Object);

        // Act
        var types = collection.GetRequiredDbSetTypes().ToList();

        // Assert
        Assert.Empty(types);
    }

    [Fact]
    public void GetRequiredDbSetTypes_Should_Return_Types_From_DbSetProvider_Features()
    {
        // Arrange
        var feature1 = new TestDbSetProviderFeature(typeof(TestEntity1));
        var feature2 = new TestDbSetProviderFeature(typeof(TestEntity2));
        
        var collection = new DbContextFeatureCollection();
        collection.AddFeature(feature1).AddFeature(feature2);

        // Act
        var types = collection.GetRequiredDbSetTypes().ToList();

        // Assert
        Assert.Equal(2, types.Count);
        Assert.Contains(typeof(TestEntity1), types);
        Assert.Contains(typeof(TestEntity2), types);
    }

    [Fact]
    public void GetRequiredDbSetTypes_Should_Return_Distinct_Types()
    {
        // Arrange
        var feature1 = new TestDbSetProviderFeature(typeof(TestEntity1));
        var feature2 = new TestDbSetProviderFeature(typeof(TestEntity1));
        
        var collection = new DbContextFeatureCollection();
        collection.AddFeature(feature1).AddFeature(feature2);

        // Act
        var types = collection.GetRequiredDbSetTypes().ToList();

        // Assert
        Assert.Single(types);
        Assert.Contains(typeof(TestEntity1), types);
    }

    [Fact]
    public void GetRequiredDbSetTypes_Should_Ignore_Features_Not_Implementing_IDbSetProvider()
    {
        // Arrange
        var feature = new Mock<IDbContextFeature>();
        feature.SetupGet(f => f.RequiresDbSet).Returns(true);
        // Not implementing IDbSetProvider
        
        var collection = new DbContextFeatureCollection();
        collection.AddFeature(feature.Object);

        // Act
        var types = collection.GetRequiredDbSetTypes().ToList();

        // Assert
        Assert.Empty(types);
    }

    #endregion

    #region Test Helpers

    private class TestFeature : IDbContextFeature
    {
        public bool RequiresDbSet => false;
        public void ConfigureConventions(ModelConfigurationBuilder builder) { }
        public void ConfigureModel(ModelBuilder modelBuilder) { }
        public void OnBeforeSaveChanges(DbContext context, KeyType userId) { }
    }

    private class TestDbSetProviderFeature : IDbContextFeature, IDbSetProvider
    {
        public TestDbSetProviderFeature(Type entityType)
        {
            EntityType = entityType;
        }

        public bool RequiresDbSet => true;
        public Type EntityType { get; }
        public void ConfigureConventions(ModelConfigurationBuilder builder) { }
        public void ConfigureModel(ModelBuilder modelBuilder) { }
        public void OnBeforeSaveChanges(DbContext context, KeyType userId) { }
    }

    private class TestEntity1 { public int Id { get; set; } }
    private class TestEntity2 { public int Id { get; set; } }

    #endregion
}

