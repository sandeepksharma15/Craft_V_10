namespace Craft.Domain.Tests.Contracts;

public class HasVersionTests
{
    [Fact]
    public void IHasVersion_ColumnName_IsConstant()
    {
        // Assert
        Assert.Equal(IHasVersion.ColumnName, "Version");
    }

    [Fact]
    public void GetVersion_ReturnsInitialVersion()
    {
        // Arrange
        // Act
        IHasVersion hasVersion = (ConcreteHasVersion)new();
        long actualVersion = hasVersion.GetVersion();

        // Assert
        Assert.Equal(0, actualVersion); // Default initial value
    }

    [Fact]
    public void SetVersion_SetsVersionValue()
    {
        // Arrange
        ConcreteHasVersion instance = new();
        const long expectedVersion = 123;

        // Act
        IHasVersion hasVersion = instance;
        hasVersion.SetVersion(expectedVersion);

        // Assert
        Assert.Equal(expectedVersion, instance.Version);
    }

    [Fact]
    public void IncrementVersion_IncreasesVersionByOne()
    {
        // Arrange
        ConcreteHasVersion instance = new() { Version = 10 };

        // Act
        IHasVersion hasVersion = instance;
        hasVersion.IncrementVersion();

        // Assert
        Assert.Equal(11, instance.Version);
    }

    [Fact]
    public void DecrementVersion_DecreasesVersionByOne()
    {
        // Arrange
        ConcreteHasVersion instance = new() { Version = 20 };

        // Act
        IHasVersion hasVersion = instance;
        hasVersion.DecrementVersion();

        // Assert
        Assert.Equal(19, instance.Version);
    }

    [Fact]
    public void DecrementVersion_DoesNotGoBelowZero()
    {
        // Arrange
        ConcreteHasVersion instance = new() { Version = 0 };

        // Act
        IHasVersion hasVersion = instance;
        hasVersion.DecrementVersion();

        // Assert
        Assert.Equal(0, instance.Version);
    }

    private class ConcreteHasVersion : IHasVersion
    {
        public long Version { get; set; }
    }
}
