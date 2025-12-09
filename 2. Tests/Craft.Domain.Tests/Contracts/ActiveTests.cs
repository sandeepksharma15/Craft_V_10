namespace Craft.Domain.Tests.Contracts;

public class ActiveTests
{
    [Fact]
    public void Activate_Should_SetIsActiveToTrue()
    {
        // Arrange
        IHasActive entity = new TestEntity();

        // Act
        entity.Activate();

        // Assert
        Assert.True(entity.IsActive);
    }

    [Fact]
    public void Deactivate_Should_SetIsActiveToFalse()
    {
        // Arrange
        IHasActive entity = new TestEntity();
        entity.Activate(); // Set IsActive to true initially

        // Act
        entity.Deactivate();

        // Assert
        Assert.False(entity.IsActive);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetActive_Should_SetIsActive(bool isActive)
    {
        // Arrange
        IHasActive entity = new TestEntity();

        // Act
        entity.SetActive(isActive);

        // Assert
        Assert.Equal(isActive, entity.IsActive);
    }

    [Fact]
    public void ToggleActive_Should_InvertIsActive()
    {
        // Arrange
        IHasActive entity = new TestEntity();
        entity.Activate(); // Set IsActive to true initially

        // Act
        entity.ToggleActive();

        // Assert
        Assert.False(entity.IsActive);

        // Act again
        entity.ToggleActive();

        // Assert again
        Assert.True(entity.IsActive);
    }

    private class TestEntity : IHasActive
    {
        public bool IsActive { get; set; }
    }
}
