namespace Craft.Domain.Tests.Contracts;

public class SoftDeleteTests
{
    [Fact]
    public void ISoftDelete_ColumnName_IsConstant()
    {
        // Assert
        Assert.Equal("IsDeleted", ISoftDelete.ColumnName);
    }

    [Fact]
    public void IsDeleted_ReturnsFalseInitially()
    {
        // Arrange
        ISoftDelete instance = new ConcreteSoftDelete();

        // Act & Assert
        Assert.False(instance.IsDeleted);
    }

    [Fact]
    public void Delete_SetsIsDeletedToTrue()
    {
        // Arrange
        ISoftDelete instance = new ConcreteSoftDelete();

        // Act
        instance.Delete();

        // Assert
        Assert.True(instance.IsDeleted);
    }

    [Fact]
    public void Restore_SetsIsDeletedToFalse()
    {
        // Arrange
        ISoftDelete instance = new ConcreteSoftDelete();
        instance.Delete(); // Set IsDeleted to true first

        // Act
        instance.Restore();

        // Assert
        Assert.False(instance.IsDeleted); // Should be restored to false
    }

    [Fact]
    public void Restore_HasNoEffectOnAlreadyRestoredObject()
    {
        // Arrange
        ISoftDelete instance = new ConcreteSoftDelete();

        // Act & Assert
        instance.Restore(); // Should have no effect

        // Assert
        Assert.False(instance.IsDeleted); // Remains false
    }

    private class ConcreteSoftDelete : ISoftDelete
    {
        public bool IsDeleted { get; set; }
    }
}
