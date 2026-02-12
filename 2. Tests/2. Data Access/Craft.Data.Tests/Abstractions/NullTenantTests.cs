using Craft.MultiTenant;

namespace Craft.Data.Tests.Abstractions;

/// <summary>
/// Unit tests for NullTenant singleton.
/// </summary>
public class NullTenantTests
{
    [Fact]
    public void Instance_ShouldReturnSingletonInstance()
    {
        // Arrange & Act
        var instance1 = NullTenant.Instance;
        var instance2 = NullTenant.Instance;

        // Assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
        Assert.Same(instance1, instance2); // Should be same reference
    }

    [Fact]
    public void Instance_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var tenant = NullTenant.Instance;

        // Assert
        Assert.Equal("default", tenant.Identifier);
        Assert.Equal("Default Tenant", tenant.Name);
        Assert.Equal(string.Empty, tenant.AdminEmail);
        Assert.Equal(string.Empty, tenant.LogoUri);
        Assert.Equal(string.Empty, tenant.ConnectionString);
        Assert.Equal(string.Empty, tenant.DbProvider);
        Assert.Equal(TenantType.Host, tenant.Type);
        Assert.Equal(TenantDbType.Shared, tenant.DbType);
        Assert.Equal(DateTime.MaxValue, tenant.ValidUpTo);
        Assert.False(tenant.IsDeleted);
        Assert.Equal(string.Empty, tenant.ConcurrencyStamp);
        Assert.True(tenant.IsActive);
    }

    [Fact]
    public void Instance_ShouldImplementITenant()
    {
        // Arrange & Act
        var tenant = NullTenant.Instance;

        // Assert
        Assert.IsType<ITenant>(tenant, exactMatch: false);
    }

    [Fact]
    public void Instance_Properties_ShouldBeSettable()
    {
        // Arrange
        var tenant = NullTenant.Instance;
        var originalIdentifier = tenant.Identifier;

        // Act - Properties should be settable even on singleton
        // (Though in practice, you shouldn't modify the singleton)
        tenant.Identifier = "test";

        // Assert
        Assert.Equal("test", tenant.Identifier);

        // Cleanup - restore original value
        tenant.Identifier = originalIdentifier;
    }

    [Fact]
    public void Id_Property_ShouldBeDefault()
    {
        // Arrange
        var tenant = NullTenant.Instance;

        // Act
        var id = tenant.Id;

        // Assert
        Assert.Equal(default, id);
    }

    [Fact]
    public void Identifier_Property_ShouldReturnDefaultString()
    {
        // Arrange
        var tenant = NullTenant.Instance;

        // Act
        var identifier = tenant.Identifier;

        // Assert
        Assert.Equal("default", identifier);
    }

    [Fact]
    public void IsActive_ShouldBeTrue()
    {
        // Arrange & Act
        var tenant = NullTenant.Instance;

        // Assert
        Assert.True(tenant.IsActive);
    }

    [Fact]
    public void Type_ShouldBeHost()
    {
        // Arrange & Act
        var tenant = NullTenant.Instance;

        // Assert
        Assert.Equal(TenantType.Host, tenant.Type);
    }
}
