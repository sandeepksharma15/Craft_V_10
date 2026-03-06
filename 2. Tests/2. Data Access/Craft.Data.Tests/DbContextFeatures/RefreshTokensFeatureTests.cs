using Craft.Data.DbContextFeatures;
using Craft.Security;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.Tests.DbContextFeatures;

public class RefreshTokensFeatureTests
{
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken<KeyType>>(b => b.HasKey(rt => rt.Id));

            var feature = new RefreshTokensFeature();
            feature.ConfigureModel(modelBuilder);
        }
    }

    private class TestDbContextFiltered : DbContext
    {
        public DbSet<RefreshToken<KeyType>> RefreshTokens { get; set; } = null!;

        public TestDbContextFiltered(DbContextOptions<TestDbContextFiltered> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken<KeyType>>(b => b.HasKey(rt => rt.Id));

            var feature = new RefreshTokensFeature();
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
        var entity = context.Model.FindEntityType(typeof(RefreshToken<KeyType>));

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("ID_RefreshTokens", entity.GetTableName());
    }

    [Fact]
    public void ConfigureModel_Should_Add_Unique_Index_On_Token()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);

        // Act
        var entity = context.Model.FindEntityType(typeof(RefreshToken<KeyType>));
        var indexes = entity!.GetIndexes().ToList();

        // Assert
        Assert.Contains(indexes, i =>
            i.Properties.Any(p => p.Name == nameof(RefreshToken<KeyType>.Token)) && i.IsUnique);
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
        var entity = context.Model.FindEntityType(typeof(RefreshToken<KeyType>));
        var indexes = entity!.GetIndexes().ToList();

        // Assert
        Assert.Contains(indexes, i =>
            i.Properties.Any(p => p.Name == nameof(RefreshToken<KeyType>.UserId)));
    }

    [Fact]
    public void ConfigureModel_Should_Apply_GlobalQueryFilter_ExcludingSoftDeleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextFiltered>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContextFiltered(options);

        var active = new RefreshToken<KeyType> { Id = 1, Token = "active", IsDeleted = false };
        var deleted = new RefreshToken<KeyType> { Id = 2, Token = "deleted", IsDeleted = true };

        context.RefreshTokens.Add(active);
        context.RefreshTokens.Add(deleted);
        context.SaveChanges();

        // Act
        var results = context.RefreshTokens.ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("active", results[0].Token);
    }

    [Fact]
    public void DefaultFeature_Should_Derive_From_Generic_With_KeyType()
    {
        // Assert
        Assert.True(typeof(RefreshTokensFeature).IsAssignableTo(typeof(RefreshTokensFeature<KeyType>)));
    }

    [Fact]
    public void GenericFeature_Should_Implement_IDbContextFeature()
    {
        // Assert
        Assert.True(typeof(RefreshTokensFeature<KeyType>).IsAssignableTo(typeof(IDbContextFeature)));
    }
}
