namespace Craft.Domain.Tests.Contracts;

public class HasUserTests
{
    [Fact]
    public void IHasUser_ColumnName_IsConstant()
    {
        // Assert
        Assert.Equal(IHasUser.ColumnName, "UserId");
    }

    [Fact]
    public void IHasUser_CastsToIHasUserOfConcreteType()
    {
        // Arrange
        // Act & Assert
        IHasUser castInstance = (ConcreteHasUser)new();
        Assert.IsType<ConcreteHasUser>(castInstance);
    }

    [Fact]
    public void IHasUserOfTKey_GetUserId_ReturnsUserIdValue()
    {
        // Arrange
        // Act & Assert
        IHasUser hasUser = (ConcreteHasUser)new() { UserId = 123 };

        var actualId = hasUser.GetUserId();
        Assert.Equal(123, actualId);
    }

    [Fact]
    public void IHasUserOfTKey_SetUserId_SetsUserIdValue()
    {
        // Arrange
        ConcreteHasUser instance = new() { UserId = 123 };

        // Act
        IHasUser hasUser = instance;
        hasUser.SetUserId(456);

        // Assert
        Assert.Equal(456, instance.UserId);
    }

    [Fact]
    public void IHasUserOfTKey_IsUserIdSet_ReturnsTrueForSetUserId()
    {
        // Arrange
        // Act & Assert
        IHasUser hasUser = (ConcreteHasUser)new() { UserId = 123 };
        bool isSet = hasUser.IsUserIdSet();
        Assert.True(isSet);
    }

    [Fact]
    public void IHasUserOfTKey_IsUserIdSet_ReturnsFalseForDefaultUserId()
    {
        // Arrange
        // Act & Assert
        IHasUser hasUser = (ConcreteHasUser)new();
        bool isSet = hasUser.IsUserIdSet();
        Assert.False(isSet);
    }

    private class ConcreteHasUser : IHasUser
    {
        public KeyType UserId { get; set; }
    }
}
