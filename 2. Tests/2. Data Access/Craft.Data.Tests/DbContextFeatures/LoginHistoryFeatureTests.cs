using Craft.Data.DbContextFeatures;
using Craft.Security;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.Tests.DbContextFeatures;

public class LoginHistoryFeatureTests
{
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var feature = new LoginHistoryFeature();
            feature.ConfigureModel(modelBuilder);
        }
    }

    [Fact]
    public void ConfigureModel_Should_Set_Table_Name()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);

        // Act
        var entity = context.Model.FindEntityType(typeof(LoginHistory));

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("ID_LoginHistory", entity.GetTableName());
    }

    [Fact]
    public void ConfigureModel_Should_Add_Index_On_UserId()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);

        // Act
        var entity = context.Model.FindEntityType(typeof(LoginHistory));
        var indexes = entity!.GetIndexes().ToList();

        // Assert
        Assert.Contains(indexes, i =>
            i.Properties.Any(p => p.Name == nameof(LoginHistory.UserId)));
    }

    [Fact]
    public void ConfigureModel_Should_Add_Index_On_LastLoginOn()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);

        // Act
        var entity = context.Model.FindEntityType(typeof(LoginHistory));
        var indexes = entity!.GetIndexes().ToList();

        // Assert
        Assert.Contains(indexes, i =>
            i.Properties.Any(p => p.Name == nameof(LoginHistory.LastLoginOn)));
    }

    [Fact]
    public void DefaultFeature_Should_Implement_IDbContextFeature()
    {
        // Assert
        Assert.True(typeof(LoginHistoryFeature).IsAssignableTo(typeof(IDbContextFeature)));
    }

    [Fact]
    public void GenericFeature_Should_Implement_IDbContextFeature()
    {
        // Assert
        Assert.True(typeof(LoginHistoryFeature<KeyType>).IsAssignableTo(typeof(IDbContextFeature)));
    }
}
