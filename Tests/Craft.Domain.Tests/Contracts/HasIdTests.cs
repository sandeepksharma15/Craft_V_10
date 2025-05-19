namespace Craft.Domain.Tests.Contracts;

public class HasIdTests
{
    [Fact]
    public void IHasId_IsNew_ReturnsTrueForDefaultId()
    {
        // Arrange
        IHasId instance = new ConcreteHasId();

        // Act & Assert
        Assert.True(instance.IsNew);
    }

    [Fact]
    public void IHasId_IsNew_ReturnsFalseForNonDefaultId()
    {
        // Arrange
        IHasId instance = new ConcreteHasId { Id = 123 };

        // Act & Assert
        Assert.False(instance.IsNew);
    }

    [Fact]
    public void IHasId_GetId_ReturnsIdValue()
    {
        // Arrange
        IHasId instance = new ConcreteHasId { Id = 456 };

        // Act & Assert
        KeyType actualId = instance.GetId();
        Assert.Equal(456, actualId);

    }

    [Fact]
    public void IHasId_SetId_SetsIdValue()
    {
        // Arrange
        IHasId instance = new ConcreteHasId();

        // Act
        instance.SetId(789);

        // Assert
        Assert.Equal(789, instance.Id);
    }

    [Fact]
    public void IHasId_ColumnName_IsConstant()
    {
        // Assert
        Assert.Equal("Id", IHasId.ColumnName);
    }

    [Fact]
    public void IHasId_CastsToIHasIdOfConcreteType()
    {
        // Arrange
        ConcreteHasId instance = new();

        // Act & Assert
        IHasId castInstance = instance;
        Assert.NotNull(castInstance);
    }

    private class ConcreteHasId : IHasId
    {
        public KeyType Id { get; set; }
    }
}
