using Craft.Value.DbContextFeatures;
using Craft.MultiTenant;
using Craft.Security;
using Moq;

namespace Craft.Value.Tests.Extensions;

public class DbContextFeatureExtensionsTests
{
    private class FakeTenant : ITenant
    {
        public KeyType Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string DbProvider { get; set; } = string.Empty;
        public TenantType Type { get; set; }
        public string LogoUri { get; set; } = string.Empty;
        public DateTime ValidUpTo { get; set; }
        public bool IsActive { get; set; }
        public TenantDbType DbType { get; set; }
        public bool IsDeleted { get; set; }
        public string? ConcurrencyStamp { get; set; }
    }

    [Fact]
    public void AddAuditTrail_Should_Add_AuditTrailFeature()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();

        // Act
        var result = collection.AddAuditTrail();

        // Assert
        Assert.Same(collection, result);
        Assert.Single(collection);
        Assert.IsType<AuditTrailFeature>(collection[0]);
    }

    [Fact]
    public void AddSoftDelete_Should_Add_SoftDeleteFeature()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();

        // Act
        var result = collection.AddSoftDelete();

        // Assert
        Assert.Same(collection, result);
        Assert.Single(collection);
        Assert.IsType<SoftDeleteFeature>(collection[0]);
    }

    [Fact]
    public void AddMultiTenancy_Should_Add_MultiTenancyFeature_With_Tenant()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();
        var tenant = new FakeTenant { Id = 1 };

        // Act
        var result = collection.AddMultiTenancy(tenant);

        // Assert
        Assert.Same(collection, result);
        Assert.Single(collection);
        Assert.IsType<MultiTenancyFeature>(collection[0]);
    }

    [Fact]
    public void AddConcurrency_Should_Add_ConcurrencyFeature()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();

        // Act
        var result = collection.AddConcurrency();

        // Assert
        Assert.Same(collection, result);
        Assert.Single(collection);
        Assert.IsType<ConcurrencyFeature>(collection[0]);
    }

    [Fact]
    public void AddVersionTracking_Should_Add_VersionTrackingFeature()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();

        // Act
        var result = collection.AddVersionTracking();

        // Assert
        Assert.Same(collection, result);
        Assert.Single(collection);
        Assert.IsType<VersionTrackingFeature>(collection[0]);
    }

    [Fact]
    public void AddIdentity_Generic_Should_Add_IdentityFeature_With_Custom_Types()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();
        var prefix = "Custom_";

        // Act
        var result = collection.AddIdentity<CraftUser, CraftRole, KeyType>(prefix);

        // Assert
        Assert.Same(collection, result);
        Assert.Single(collection);
    }

    [Fact]
    public void AddIdentity_Default_Should_Add_IdentityFeature_With_Default_Types()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();
        var prefix = "Id_";

        // Act
        var result = collection.AddIdentity(prefix);

        // Assert
        Assert.Same(collection, result);
        Assert.Single(collection);
        Assert.IsType<IdentityFeature>(collection[0]);
    }

    [Fact]
    public void AddIdentity_Should_Use_Default_Prefix_When_Not_Specified()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();

        // Act
        var result = collection.AddIdentity();

        // Assert
        Assert.Same(collection, result);
        Assert.Single(collection);
        Assert.IsType<IdentityFeature>(collection[0]);
    }

    [Fact]
    public void AddCommonFeatures_Should_Add_All_Common_Features()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();

        // Act
        var result = collection.AddCommonFeatures();

        // Assert
        Assert.Same(collection, result);
        Assert.Equal(4, collection.Count);
        Assert.Contains(collection, f => f is AuditTrailFeature);
        Assert.Contains(collection, f => f is SoftDeleteFeature);
        Assert.Contains(collection, f => f is ConcurrencyFeature);
        Assert.Contains(collection, f => f is VersionTrackingFeature);
    }

    [Fact]
    public void AddCommonFeatures_Should_Maintain_Order()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();

        // Act
        collection.AddCommonFeatures();

        // Assert
        Assert.IsType<AuditTrailFeature>(collection[0]);
        Assert.IsType<SoftDeleteFeature>(collection[1]);
        Assert.IsType<ConcurrencyFeature>(collection[2]);
        Assert.IsType<VersionTrackingFeature>(collection[3]);
    }

    [Fact]
    public void AddAllFeatures_Should_Add_All_Features_Including_MultiTenancy()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();
        var tenant = new FakeTenant { Id = 1 };

        // Act
        var result = collection.AddAllFeatures(tenant);

        // Assert
        Assert.Same(collection, result);
        Assert.Equal(5, collection.Count);
        Assert.Contains(collection, f => f is AuditTrailFeature);
        Assert.Contains(collection, f => f is SoftDeleteFeature);
        Assert.Contains(collection, f => f is ConcurrencyFeature);
        Assert.Contains(collection, f => f is VersionTrackingFeature);
        Assert.Contains(collection, f => f is MultiTenancyFeature);
    }

    [Fact]
    public void AddAllFeatures_Should_Maintain_Order()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();
        var tenant = new FakeTenant { Id = 1 };

        // Act
        collection.AddAllFeatures(tenant);

        // Assert
        Assert.IsType<AuditTrailFeature>(collection[0]);
        Assert.IsType<SoftDeleteFeature>(collection[1]);
        Assert.IsType<ConcurrencyFeature>(collection[2]);
        Assert.IsType<VersionTrackingFeature>(collection[3]);
        Assert.IsType<MultiTenancyFeature>(collection[4]);
    }

    [Fact]
    public void Extensions_Should_Support_Fluent_Chaining()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();
        var tenant = new FakeTenant { Id = 1 };

        // Act
        var result = collection
            .AddAuditTrail()
            .AddSoftDelete()
            .AddMultiTenancy(tenant)
            .AddConcurrency()
            .AddVersionTracking()
            .AddIdentity();

        // Assert
        Assert.Same(collection, result);
        Assert.Equal(6, collection.Count);
    }

    [Fact]
    public void Extensions_Can_Be_Called_Multiple_Times()
    {
        // Arrange
        var collection = new DbContextFeatureCollection();

        // Act
        collection.AddSoftDelete();
        collection.AddSoftDelete();

        // Assert - should allow duplicates (up to consumer to manage)
        Assert.Equal(2, collection.Count);
    }
}

