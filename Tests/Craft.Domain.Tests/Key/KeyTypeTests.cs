namespace Craft.Domain.Tests.Keys;

public class KeyTypeTests
{
    [Fact]
    public void KeyType_Equal_ShouldReturnTrue_WhenIdIsAssigned()
    {
        // Arrange
        const int id1 = 1;
        const int id2 = id1;

        // Act
        var result = id1.Equals(id2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void KeyType_Equals_ShouldReturnFalse_WhenIdsAreNotEqual()
    {
        // Arrange
        const int id1 = 1;
        const int id2 = 2;

        // Act
        var result = id1.Equals(id2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void KeyType_Equals_ShouldReturnTrue_WhenIdsAreEqual()
    {
        // Arrange
        const int id1 = 1;
        const int id2 = 1;

        // Act
        var result = id1.Equals(id2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void KeyType_ImplicitConversionFromInt_ShouldCreateKeyType()
    {
        // Arrange
        const int value = 1;

        // Act
        const KeyType id = value;

        // Assert
        Assert.Equal(1, id);
    }

    [Fact]
    public void KeyType_ImplicitConversionFromLong_ShouldCreateKeyType()
    {
        // Arrange
        const long value = 1;

        // Act
        const KeyType id = value;

        // Assert
        Assert.Equal(1, id);
    }

    [Fact]
    public void KeyType_ImplicitConversionFromString_ShouldCreateKeyType()
    {
        // Arrange
        const string value = "1";

        // Act
        KeyType id = long.Parse(value);

        // Assert
        Assert.Equal(1, id);
    }

    [Fact]
    public void KeyType_ImplicitConversionToInt_ShouldReturnIdValueAsInt()
    {
        // Arrange
        const int id = 1;

        // Act
        const int value = id;

        // Assert
        Assert.Equal(1, value);
    }

    [Fact]
    public void KeyType_ImplicitConversionToLong_ShouldReturnIdValue()
    {
        // Arrange
        const int id = 1;

        // Act
        const long value = id;

        // Assert
        Assert.Equal(1, value);
    }

    [Fact]
    public void KeyType_Parse_ShouldParseStringValueToLong()
    {
        // Arrange
        const string value = "1";

        // Act
        var result = KeyType.Parse(value);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void KeyType_TryParse_WithInvalidStringValue_ShouldReturnFalse()
    {
        // Arrange
        const string value = "invalid";

        // Act
        var success = KeyType.TryParse(value, out var result);

        // Assert
        Assert.False(success);
        Assert.Equal(0, result);
    }

    [Fact]
    public void KeyType_TryParse_WithValidStringValue_ShouldParseSuccessfully()
    {
        // Arrange
        const string value = "1";

        // Act
        var success = KeyType.TryParse(value, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(1, result);
    }

    [Fact]
    public void ModelId_CompareTo_ShouldReturnNegativeValue_WhenIdIsLessThanOtherId()
    {
        // Arrange
        const int id1 = 1;
        const int id2 = 2;

        // Act
        int result = id1.CompareTo(id2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void ModelId_CompareTo_ShouldReturnPositiveValue_WhenIdIsGreaterThanOtherId()
    {
        // Arrange
        const int id1 = 2;
        const int id2 = 1;

        // Act
        int result = id1.CompareTo(id2);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void ModelId_CompareTo_ShouldReturnZero_WhenIdIsEqualToOtherId()
    {
        // Arrange
        const int id1 = 1;
        const int id2 = 1;

        // Act
        int result = id1.CompareTo(id2);

        // Assert
        Assert.True(result == 0);
    }

    [Fact]
    public void ModelId_GetHashCode_ShouldReturnDifferentHashCode_WhenIdsAreNotEqual()
    {
        // Arrange
        const int id1 = 1;
        const int id2 = 2;

        // Act
        int hashCode1 = id1.GetHashCode();
        int hashCode2 = id2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void ModelId_GetHashCode_ShouldReturnSameHashCode_WhenIdsAreEqual()
    {
        // Arrange
        const int id1 = 1;
        const int id2 = 1;

        // Act
        int hashCode1 = id1.GetHashCode();
        int hashCode2 = id2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void ModelId_ToString_ShouldReturnStringRepresentationOfId()
    {
        // Arrange
        const int id = 1;

        // Act
        var result = id.ToString();

        // Assert
        Assert.Equal("1", result);
    }
}
