namespace Craft.Domain.Tests.Helpers;

public class EntityHelperTests
{
    [Fact]
    public void EntityEquals_WithDifferentEntities_ShouldReturnFalse()
    {
        // Arrange
        IEntity entity1 = new TestEntity { Id = 1 };
        IEntity entity2 = new TestEntity { Id = 2 };

        // Act
        bool result = entity1.EntityEquals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EntityEquals_WithEqualEntities_ShouldReturnTrue()
    {
        // Arrange
        IEntity entity1 = new TestEntity { Id = 1 };
        IEntity entity2 = new TestEntity { Id = 1 };

        // Act
        bool result = entity1.EntityEquals(entity2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EntityEquals_WithNullEntity1_ShouldReturnFalse()
    {
        // Arrange
        IEntity entity1 = null!;
        IEntity entity2 = new TestEntity { Id = 1 };

        // Act
        bool result = entity1.EntityEquals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EntityEquals_WithNullEntity2_ShouldReturnFalse()
    {
        // Arrange
        IEntity entity1 = new TestEntity { Id = 1 };
        IEntity entity2 = null!;

        // Act
        bool result = entity1.EntityEquals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EntityEquals_WithSameReference_ShouldReturnTrue()
    {
        // Arrange
        IEntity entity1 = new TestEntity { Id = 1 };
        IEntity entity2 = entity1;

        // Act
        bool result = entity1.EntityEquals(entity2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasDefaultId_ShouldReturnFalse_WhenEntityIdIsNotDefault()
    {
        // Arrange
        var entity = new FakeEntity<int> { Id = 1 };

        // Act
        var result = entity.HasDefaultId();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasDefaultId_ShouldReturnFalse_WhenEntityIdIsPositiveInt()
    {
        // Arrange
        var entity = new FakeEntity<int> { Id = 10 };

        // Act
        var result = entity.HasDefaultId();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasDefaultId_ShouldReturnFalse_WhenEntityIdIsPositiveLong()
    {
        // Arrange
        var entity = new FakeEntity<long> { Id = 1 };

        // Act
        var result = entity.HasDefaultId();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasDefaultId_ShouldReturnTrue_WhenEntityIdIsDefault()
    {
        // Arrange
        var entity = new FakeEntity<int> { Id = default };

        // Act
        var result = entity.HasDefaultId();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasDefaultId_ShouldReturnTrue_WhenEntityImplemented()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var result = entity.HasDefaultId();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasDefaultId_ShouldReturnTrue_WhenEntityIdIsNegativeInt()
    {
        // Arrange
        var entity = new FakeEntity<int> { Id = -1 };

        // Act
        var result = entity.HasDefaultId();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasDefaultId_ShouldReturnTrue_WhenEntityIdIsNegativeLong()
    {
        // Arrange
        var entity = new FakeEntity<long> { Id = -1 };

        // Act
        var result = entity.HasDefaultId();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEntity_WithEntity_ShouldReturnTrue()
    {
        // Arrange
        Type type = typeof(TestEntity);

        // Act
        bool result = type.IsEntity();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEntity_WithNonEntity_ShouldReturnFalse()
    {
        // Arrange
        Type type = typeof(NonEntityClass);

        // Act
        bool result = type.IsEntity();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEntity_WithNullType_ShouldThrowArgumentNullException()
    {
        // Arrange
        Type type = null!;

        // Act
        void action() => type.IsEntity();

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void IsMultiTenant_WithMultiTenantEntity_ShouldReturnTrue()
    {
        // Arrange
        const bool expectedResult = true;

        // Act
        bool result = EntityHelper.IsMultiTenant<MultiTenantEntity>();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void IsMultiTenant_WithNonMultiTenantEntity_ShouldReturnFalse()
    {
        // Arrange
        const bool expectedResult = false;

        // Act
        bool result = EntityHelper.IsMultiTenant<NonMultiTenantEntity>();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void IsMultiTenant_WithNullType_ShouldReturnFalse()
    {
        // Arrange
        Type type = null!;

        // Act
        bool result = type.IsMultiTenant();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetEntityKeyType_NullInput_ReturnsNull()
    {
        // Arrange
        Type type = null!;

        // Act
        var keyType = type.GetEntityKeyType();

        // Assert
        Assert.Null(keyType);
    }

    [Fact]
    public void GetEntityKeyType_NonEntityType_ThrowsArgumentException()
    {
        // Act & Assert
        static void act() => typeof(NonEntityClass).GetEntityKeyType();

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void GetEntityKeyType_ValidEntityType_ReturnsKeyType()
    {
        // Arrange
        // Act
        var keyType = typeof(TestEntity).GetEntityKeyType();

        // Assert
        Assert.Equal(typeof(long), keyType);
    }
}

public class MultiTenantEntity : IEntity, IHasTenant
{
    public KeyType Id { get; set; }
    public KeyType TenantId { get; set; }
}

public class NonEntityClass
{
    public KeyType Id { get; set; }
}

public class NonMultiTenantEntity : IEntity
{
    public KeyType Id { get; set; }
}

public class TestEntity : IEntity
{
    public KeyType Id { get; set; }
}

public class FakeEntity<TKey> : IEntity<TKey>
{
    public TKey Id { get; set; } = default!;
}
