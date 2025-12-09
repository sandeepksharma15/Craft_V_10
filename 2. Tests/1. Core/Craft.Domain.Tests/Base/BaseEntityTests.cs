namespace Craft.Domain.Tests.Base;

public class BaseEntityTests
{
    [Fact]
    public void ConcurrencyStamp_Should_Be_NewGuid()
    {
        // Arrange
        var entity = new MockEntity(1);

        // Act
        // Assert
        Assert.NotNull(entity.ConcurrencyStamp);
        Assert.True(Guid.TryParse(entity.ConcurrencyStamp, out _));
    }

    [Fact]
    public void SetConcurrencyStamp_Should_Be_SetValue()
    {
        // Arrange
        var entity = new MockEntity { Id = 1, ConcurrencyStamp = "test" };

        // Assert
        Assert.NotNull(entity.ConcurrencyStamp);
        Assert.Equal("test", entity.ConcurrencyStamp);
    }

    [Fact]
    public void EqualityOperator_Should_Return_False_For_Different_Id()
    {
        // Arrange
        var entity1 = new MockEntity(1);
        var entity2 = new MockEntity(2);

        // Act
        bool result = entity1 == entity2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualityOperator_Should_Return_True_For_Same_Id()
    {
        // Arrange
        var entity1 = new MockEntity(1);
        var entity2 = new MockEntity(1);

        // Act
        bool result = entity1 == entity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenDifferentEntities()
    {
        // Arrange
        var entity1 = new MockEntity(1);
        var entity2 = new MockEntity(2);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenDifferentTenants()
    {
        // Arrange
        var entity1 = new MockTenantEntity(1, 1001);
        var entity2 = new MockTenantEntity(1, 1002);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenDifferentTypes()
    {
        // Arrange
        var entity = new MockEntity(1);
        var otherObject = new object();

        // Act
        var result = entity.Equals(otherObject);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenDifferentTypes_DerivedFromEntity()
    {
        // Arrange
        var entity = new MockEntity(1);
        var otherObject = new MockTenantEntity(1, 1);

        // Act
        var result = entity.Equals(otherObject);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenNull()
    {
        // Arrange
        var entity = new MockEntity(1);

        // Act
        var result = entity.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenEqualEntities()
    {
        // Arrange
        var entity1 = new MockEntity(1);
        var entity2 = new MockEntity(1);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenSameInstance()
    {
        // Arrange
        var entity = new MockEntity(1);

        // Act
        var result = entity.Equals(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetHashCode_Should_Not_Return_Zero()
    {
        // Arrange
        var entity = new MockEntity(1);

        // Act
        // Assert
        Assert.NotEqual(0, entity.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentValue_WhenDifferentEntities()
    {
        // Arrange
        var entity1 = new MockEntity(1);
        var entity2 = new MockEntity(2);

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameValue_WhenEqualEntities()
    {
        // Arrange
        var entity1 = new MockEntity(1);
        var entity2 = new MockEntity(1);

        // Act
        var hashCode1 = entity1.GetHashCode();
        var hashCode2 = entity2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void Id_Should_Have_Value()
    {
        // Arrange
        var entity = new MockEntity(1);
        const int expectedId = 1;

        // Act
        // entity.Id = expectedId;

        // Assert
        Assert.Equal(expectedId, entity.Id);
    }

    [Fact]
    public void InequalityOperator_Should_Return_False_For_Same_Id()
    {
        // Arrange
        var entity1 = new MockEntity(1);
        var entity2 = new MockEntity(1);

        // Act
        bool result = entity1 != entity2;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void InequalityOperator_Should_Return_True_For_Different_Id()
    {
        // Arrange
        var entity1 = new MockEntity(1);
        var entity2 = new MockEntity(2);

        // Act
        bool result = entity1 != entity2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDeleted_Should_Be_False_By_Default()
    {
        // Arrange
        var entity = new MockEntity(1);

        // Act

        // Assert
        Assert.False(entity.IsDeleted);
    }

    [Fact]
    public void SetIsDeleted_Should_Be_SetValue()
    {
        // Arrange
        var entity = new MockEntity(1)
        {
            // Act
            IsDeleted = true
        };

        // Assert
        Assert.True(entity.IsDeleted);
    }

    [Fact]
    public void IsNew_Should_Return_False_For_Non_Default_Id()
    {
        // Arrange
        var entity = new MockEntity(1);

        // Act
        bool result = entity.IsNew();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNew_Should_Return_True_For_Default_Id()
    {
        // Arrange
        var entity = new MockEntity();

        // Act
        bool result = entity.IsNew();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ToString_ShouldReturnExpectedString()
    {
        // Arrange
        var entity = new MockEntity(1);

        // Act
        var result = entity.ToString();

        // Assert
        Assert.Equal("[ENTITY: MockEntity] Key = 1", result);
    }

    private class MockEntity : BaseEntity
    {
        public MockEntity()
        { }

        public MockEntity(int id) : base(id)
        {
        }
    }

    private class MockTenantEntity(long id, long tenantId) : BaseEntity(id), IHasTenant
    {
        public KeyType TenantId { get; set; } = tenantId;
    }
}
