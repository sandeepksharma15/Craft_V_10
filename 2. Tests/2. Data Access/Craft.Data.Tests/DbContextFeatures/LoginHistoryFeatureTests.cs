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

            modelBuilder.Entity<LoginHistory<KeyType>>(b => b.HasKey(lh => lh.Id));

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
        var entity = context.Model.FindEntityType(typeof(LoginHistory<KeyType>));

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
        var entity = context.Model.FindEntityType(typeof(LoginHistory<KeyType>));
        var indexes = entity!.GetIndexes().ToList();

        // Assert
        Assert.Contains(indexes, i =>
            i.Properties.Any(p => p.Name == nameof(LoginHistory<KeyType>.UserId)));
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
        var entity = context.Model.FindEntityType(typeof(LoginHistory<KeyType>));
        var indexes = entity!.GetIndexes().ToList();

        // Assert
        Assert.Contains(indexes, i =>
            i.Properties.Any(p => p.Name == nameof(LoginHistory<KeyType>.LastLoginOn)));
    }

    [Fact]
    public void DefaultFeature_Should_Derive_From_Generic_With_KeyType()
    {
        // Assert
        Assert.True(typeof(LoginHistoryFeature).IsAssignableTo(typeof(LoginHistoryFeature<KeyType>)));
    }

    [Fact]
    public void GenericFeature_Should_Implement_IDbContextFeature()
    {
        // Assert
        Assert.True(typeof(LoginHistoryFeature<KeyType>).IsAssignableTo(typeof(IDbContextFeature)));
    }
}
