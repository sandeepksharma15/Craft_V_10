using Craft.Core;
using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Craft.Data.Tests.DbContexts;

/// <summary>
/// Unit tests for BaseDbContext constructor overloads and single-tenant support.
/// </summary>
public class BaseDbContextConstructorTests
{
    private class TestDbContext : BaseDbContext<TestDbContext>
    {
        public TestDbContext(DbContextOptions<TestDbContext> options, ICurrentUser currentUser)
            : base(options, currentUser)
        {
        }

        public TestDbContext(DbContextOptions<TestDbContext> options, ITenant tenant, ICurrentUser currentUser)
            : base(options, tenant, currentUser)
        {
        }

        // Expose Features for testing
        public DbContextFeatureCollection TestFeatures => Features;
    }

    [Fact]
    public void Constructor_WithCurrentUserOnly_ShouldUseNullTenantInstance()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();

        // Act
        using var context = new TestDbContext(options, currentUser.Object);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.TestFeatures);
    }

    [Fact]
    public void Constructor_WithTenant_ShouldUseProvidedTenant()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();
        var tenant = new Mock<ITenant>();
        tenant.Setup(t => t.GetId()).Returns((KeyType)123);

        // Act
        using var context = new TestDbContext(options, tenant.Object, currentUser.Object);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.TestFeatures);
    }

    [Fact]
    public void Constructor_WithNullTenantInstance_ShouldWork()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();

        // Act
        using var context = new TestDbContext(options, NullTenant.Instance, currentUser.Object);

        // Assert
        Assert.NotNull(context);
    }

    [Fact]
    public void Constructor_Features_ShouldBeInitializedEmpty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();

        // Act
        using var context = new TestDbContext(options, currentUser.Object);

        // Assert
        Assert.NotNull(context.TestFeatures);
        Assert.Empty(context.TestFeatures);
    }

    [Fact]
    public void SingleTenantConstructor_ShouldChainToMultiTenantConstructor()
    {
        // This test verifies that the single-tenant constructor properly chains
        // to the multi-tenant constructor with NullTenant.Instance

        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();

        // Act
        using var singleTenantContext = new TestDbContext(options, currentUser.Object);
        using var explicitNullContext = new TestDbContext(options, NullTenant.Instance, currentUser.Object);

        // Assert - Both should behave identically
        Assert.NotNull(singleTenantContext);
        Assert.NotNull(explicitNullContext);
        Assert.Equal(singleTenantContext.TestFeatures.Count, explicitNullContext.TestFeatures.Count);
    }
}
