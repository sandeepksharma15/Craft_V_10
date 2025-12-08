using Craft.Core;
using Craft.MultiTenant;
using Moq;

namespace Craft.MultiTenant.Tests.ServiceTests;

public class CurrentTenantTests
{
    [Fact]
    public void CurrentTenant_WithResolvedTenant_ReturnsProperties()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = 1,
            Identifier = "test-tenant",
            Name = "Test Tenant",
            IsActive = true
        };

        var tenantContext = new TenantContext { Tenant = tenant };
        var mockAccessor = new Mock<ITenantContextAccessor>();
        mockAccessor.Setup(x => x.TenantContext).Returns(tenantContext);

        var currentTenant = new CurrentTenant(mockAccessor.Object);

        // Act & Assert
        Assert.Equal(1, currentTenant.Id);
        Assert.Equal("test-tenant", currentTenant.Identifier);
        Assert.Equal("Test Tenant", currentTenant.Name);
        Assert.True(currentTenant.IsAvailable);
        Assert.True(currentTenant.IsActive);
        Assert.Equal(1, currentTenant.GetId());
    }

    [Fact]
    public void CurrentTenant_WithNoTenant_ReturnsDefaults()
    {
        // Arrange
        var mockAccessor = new Mock<ITenantContextAccessor>();
        mockAccessor.Setup(x => x.TenantContext).Returns((ITenantContext?)null);

        var currentTenant = new CurrentTenant(mockAccessor.Object);

        // Act & Assert
        Assert.Equal(default(long), currentTenant.Id);
        Assert.Null(currentTenant.Identifier);
        Assert.Null(currentTenant.Name);
        Assert.False(currentTenant.IsAvailable);
        Assert.False(currentTenant.IsActive);
        Assert.Equal(default(long), currentTenant.GetId());
    }

    [Fact]
    public void CurrentTenant_WithInactiveTenant_ReturnsInactiveStatus()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = 2,
            Identifier = "inactive-tenant",
            Name = "Inactive Tenant",
            IsActive = false
        };

        var tenantContext = new TenantContext { Tenant = tenant };
        var mockAccessor = new Mock<ITenantContextAccessor>();
        mockAccessor.Setup(x => x.TenantContext).Returns(tenantContext);

        var currentTenant = new CurrentTenant(mockAccessor.Object);

        // Act & Assert
        Assert.Equal(2, currentTenant.Id);
        Assert.Equal("inactive-tenant", currentTenant.Identifier);
        Assert.Equal("Inactive Tenant", currentTenant.Name);
        Assert.True(currentTenant.IsAvailable);
        Assert.False(currentTenant.IsActive);
    }

    [Fact]
    public void CurrentTenant_WithEmptyTenantContext_ReturnsNotAvailable()
    {
        // Arrange
        var tenantContext = new TenantContext { Tenant = null };
        var mockAccessor = new Mock<ITenantContextAccessor>();
        mockAccessor.Setup(x => x.TenantContext).Returns(tenantContext);

        var currentTenant = new CurrentTenant(mockAccessor.Object);

        // Act & Assert
        Assert.False(currentTenant.IsAvailable);
    }

    [Fact]
    public void CurrentTenant_Generic_WithResolvedTenant_ReturnsProperties()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = 100,
            Identifier = "generic-tenant",
            Name = "Generic Tenant",
            IsActive = true
        };

        var tenantContext = new TenantContext { Tenant = tenant };
        var mockAccessor = new Mock<ITenantContextAccessor>();
        mockAccessor.Setup(x => x.TenantContext).Returns(tenantContext);

        var currentTenant = new CurrentTenant<long>(mockAccessor.Object);

        // Act & Assert
        Assert.Equal(100L, currentTenant.Id);
        Assert.Equal("generic-tenant", currentTenant.Identifier);
        Assert.Equal("Generic Tenant", currentTenant.Name);
        Assert.True(currentTenant.IsAvailable);
        Assert.True(currentTenant.IsActive);
        Assert.Equal(100L, currentTenant.GetId());
    }
}
