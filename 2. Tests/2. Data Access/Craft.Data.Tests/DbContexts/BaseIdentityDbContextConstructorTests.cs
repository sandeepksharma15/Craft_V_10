using Craft.Core;
using Craft.MultiTenant;
using Craft.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Craft.Data.Tests.DbContexts;

/// <summary>
/// Unit tests for BaseIdentityDbContext constructor overloads and single-tenant support.
/// </summary>
public class BaseIdentityDbContextConstructorTests
{
    private class TestUser : CraftUser<KeyType>
    {
    }

    private class TestRole : CraftRole<KeyType>
    {
    }

    private class TestIdentityDbContext : BaseIdentityDbContext<TestIdentityDbContext, TestUser, TestRole, KeyType>
    {
        public TestIdentityDbContext(DbContextOptions options, ICurrentUser currentUser)
            : base(options, currentUser)
        {
        }

        public TestIdentityDbContext(DbContextOptions options, ITenant tenant, ICurrentUser currentUser)
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
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();

        // Act
        using var context = new TestIdentityDbContext(options, currentUser.Object);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.TestFeatures);
    }

    [Fact]
    public void Constructor_WithTenant_ShouldUseProvidedTenant()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();
        var tenant = new Mock<ITenant>();
        tenant.Setup(t => t.GetId()).Returns((KeyType)456);

        // Act
        using var context = new TestIdentityDbContext(options, tenant.Object, currentUser.Object);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.TestFeatures);
    }

    [Fact]
    public void Constructor_WithNullTenantInstance_ShouldWork()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();

        // Act
        using var context = new TestIdentityDbContext(options, NullTenant.Instance, currentUser.Object);

        // Assert
        Assert.NotNull(context);
    }

    [Fact]
    public void Constructor_ShouldInitializeIdentityDbSets()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();

        // Act
        using var context = new TestIdentityDbContext(options, currentUser.Object);

        // Assert
        Assert.NotNull(context.Users);
        Assert.NotNull(context.Roles);
        Assert.NotNull(context.LoginHistories);
        Assert.NotNull(context.RefreshTokens);
    }

    [Fact]
    public void Constructor_Features_ShouldBeInitializedEmpty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();

        // Act
        using var context = new TestIdentityDbContext(options, currentUser.Object);

        // Assert
        Assert.NotNull(context.TestFeatures);
        Assert.Empty(context.TestFeatures);
    }

    [Fact]
    public void SingleTenantConstructor_ShouldChainToMultiTenantConstructor()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();

        // Act
        using var singleTenantContext = new TestIdentityDbContext(options, currentUser.Object);
        using var explicitNullContext = new TestIdentityDbContext(options, NullTenant.Instance, currentUser.Object);

        // Assert - Both should behave identically
        Assert.NotNull(singleTenantContext);
        Assert.NotNull(explicitNullContext);
        Assert.Equal(singleTenantContext.TestFeatures.Count, explicitNullContext.TestFeatures.Count);
    }

    [Fact]
    public void Constructor_ShouldInheritFromIdentityDbContext()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var currentUser = new Mock<ICurrentUser>();

        // Act
        using var context = new TestIdentityDbContext(options, currentUser.Object);

        // Assert
        Assert.IsType<IdentityDbContext<TestUser, TestRole, KeyType>>(context, exactMatch: false);
        Assert.IsType<IDbContext>(context, exactMatch: false);
    }
}
