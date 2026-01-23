namespace Craft.Domain.Tests.Extensions;

public class DomainExtensionsTests
{
    // Test classes
    private class TestEntity : IEntity<long>
    {
        public long Id { get; set; }
    }

    private class TestEntityGeneric<TKey> : IEntity<TKey>
    {
        public TKey Id { get; set; } = default!;
    }

    private class TestHasUser : IHasUser<long>
    {
        public long UserId { get; set; }
    }

    private class TestHasUserGeneric<TKey> : IHasUser<TKey>
    {
        public TKey UserId { get; set; } = default!;
    }

    private class TestHasTenant : IHasTenant<long>
    {
        public long TenantId { get; set; }
    }

    private class TestHasTenantGeneric<TKey> : IHasTenant<TKey>
    {
        public TKey TenantId { get; set; } = default!;
    }

    [Theory]
    [InlineData("123", 123)]
    [InlineData("456.78", 456.78)]
    [InlineData("0", 0)]
    [InlineData("-789", -789)]
    [InlineData("3.14", 3.14)]
    public void Parse_PositiveTestCases_ReturnsExpectedResult(string? input, float expectedResult)
    {
        // Act
        var result = input!.Parse<float>();

        // Assert
        Assert.Equal(expectedResult, result);
    }


    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void Parse_ShouldReturnExpectedValue(string? input, object? expected)
    {
        // Act
        var result = input!.Parse<object>();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, default(int))]
    [InlineData("123", 123)]
    [InlineData("abc", default(int))] // Parsing non-numeric value to int should return default
    [InlineData("456.78", default(int))] // Parsing non-integer value to int should return default
    public void Parse_ValidString_ReturnsExpectedResult(string? input, int expectedResult)
    {
        // Act
        var result = input!.Parse<int>();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void IsNullOrDefault_NullEntity_ReturnsTrue()
    {
        // Arrange
        TestEntity entity = null!;

        // Act
        var result = entity.IsNullOrDefault();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrDefault_EntityWithDefaultId_ReturnsTrue()
    {
        // Arrange
        var entity = new TestEntity { Id = 0 };

        // Act
        var result = entity.IsNullOrDefault();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrDefault_EntityWithNonDefaultId_ReturnsFalse()
    {
        // Arrange
        var entity = new TestEntity { Id = 1 };

        // Act
        var result = entity.IsNullOrDefault();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNullOrDefault_Generic_NullEntity_ReturnsTrue()
    {
        // Arrange
        TestEntityGeneric<string> entity = null!;

        // Act
        var result = entity.IsNullOrDefault();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrDefault_Generic_EntityWithDefaultId_ReturnsTrue()
    {
        // Arrange
        var entity = new TestEntityGeneric<string> { Id = null };

        // Act
        var result = entity.IsNullOrDefault();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrDefault_Generic_EntityWithNonDefaultId_ReturnsFalse()
    {
        // Arrange
        var entity = new TestEntityGeneric<string> { Id = "1" };

        // Act
        var result = entity.IsNullOrDefault();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void SetCreatedBy_Generic_SetsUserId()
    {
        // Arrange
        var entity = new TestHasUserGeneric<string>();
        string userId = "123";

        // Act
        entity.SetCreatedBy(userId);

        // Assert
        Assert.Equal(userId, entity.UserId);
    }

    [Fact]
    public void SetCreatedBy_SetsUserId()
    {
        // Arrange
        var entity = new TestHasUser();
        long userId = 456;

        // Act
        entity.SetCreatedBy(userId);

        // Assert
        Assert.Equal(userId, entity.UserId);
    }

    [Fact]
    public void BelongsToTenant_Generic_MatchingTenant_ReturnsTrue()
    {
        // Arrange
        var entity = new TestHasTenantGeneric<string> { TenantId = "123" };
        string tenantId = "123";

        // Act
        var result = entity.BelongsToTenant(tenantId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BelongsToTenant_Generic_NonMatchingTenant_ReturnsFalse()
    {
        // Arrange
        var entity = new TestHasTenantGeneric<string> { TenantId = "123" };
        string tenantId = "456";

        // Act
        var result = entity.BelongsToTenant(tenantId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BelongsToTenant_Generic_NullTenantId_ReturnsFalse()
    {
        // Arrange
        var entity = new TestHasTenantGeneric<string> { TenantId = null };
        string tenantId = "123";

        // Act
        var result = entity.BelongsToTenant(tenantId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BelongsToTenant_MatchingTenant_ReturnsTrue()
    {
        // Arrange
        var entity = new TestHasTenant { TenantId = 123 };
        long tenantId = 123;

        // Act
        var result = entity.BelongsToTenant(tenantId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BelongsToTenant_NonMatchingTenant_ReturnsFalse()
    {
        // Arrange
        var entity = new TestHasTenant { TenantId = 123 };
        long tenantId = 456;

        // Act
        var result = entity.BelongsToTenant(tenantId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BelongsToTenant_NullTenantId_ReturnsFalse()
    {
        // Arrange
        var entity = new TestHasTenant { TenantId = default };
        long tenantId = 123;

        // Act
        var result = entity.BelongsToTenant(tenantId);

        // Assert
        Assert.False(result);
    }

}
