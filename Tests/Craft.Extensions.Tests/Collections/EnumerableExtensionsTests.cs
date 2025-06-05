namespace Craft.Extensions.Tests.Collections;

public class EnumerableExtensionsTests
{
    [Fact]
    public void GetListDataForSelect_ReturnsEmptyDictionary_WhenItemsIsNull()
    {
        // Arrange
        IEnumerable<TestItem>? items = null;

        // Act
        var result = items.GetListDataForSelect("Id", "Name");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetListDataForSelect_UsesProperties_WhenFieldsAreValid()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Id = 1, Name = "A" },
            new TestItem { Id = 2, Name = "B" }
        };

        // Act
        var result = items.GetListDataForSelect("Id", "Name");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result["1"]);
        Assert.Equal("B", result["2"]);
    }

    [Fact]
    public void GetListDataForSelect_HandlesNullPropertyValues()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Id = 1, Name = null }
        };

        // Act
        var result = items.GetListDataForSelect("Id", "Name");

        // Assert
        Assert.Single(result);
        Assert.Equal(string.Empty, result["1"]);
    }

    [Fact]
    public void GetListDataForSelect_UsesToString_WhenFieldsAreNull()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Id = 1, Name = "A" }
        };

        // Act
        var result = items.GetListDataForSelect(null!, null!);
        var expectedKey = items[0].ToString();

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedKey, result.Keys.First());
        Assert.Equal(expectedKey, result.Values.First());
    }

    [Fact]
    public void GetListDataForSelect_HandlesNullItem_WhenFieldsAreNull()
    {
        // Arrange
        TestItem?[] items = [null];

        // Act
        var result = items.GetListDataForSelect(null!, null!);

        // Assert
        Assert.Single(result);
        Assert.Equal(string.Empty, result.Keys.First());
        Assert.Equal(string.Empty, result.Values.First());
    }

    [Fact]
    public void GetListDataForSelect_HandlesNullItem_WhenFieldsAreNotNull()
    {
        // Arrange
        TestItem?[] items = [null];

        // Act
        var result = items.GetListDataForSelect("Id", "Name");

        // Assert
        Assert.Single(result);
        Assert.Equal(string.Empty, result.Keys.First());
        Assert.Equal(string.Empty, result.Values.First());
    }

    [Fact]
    public void GetListDataForSelect_HandlesMissingProperty()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Id = 1, Name = "A" }
        };

        // Act
        var result = items.GetListDataForSelect("NonExistent", "Name");

        // Assert
        Assert.Single(result);
        Assert.Equal(string.Empty, result.Keys.First());
        Assert.Equal("A", result.Values.First());
    }

    [Fact]
    public void GetListDataForSelect_ThrowsOnDuplicateKeys()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Id = 1, Name = "A" },
            new TestItem { Id = 1, Name = "B" }
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => items.GetListDataForSelect("Id", "Name"));
    }

    [Fact]
    public void IsIn_ReturnsTrue_IfItemIsInCollection()
    {
        // Arrange
        var collection = new[] { 1, 2, 3 };

        // Act & Assert
        Assert.True(2.IsIn(collection));
    }

    [Fact]
    public void IsIn_ReturnsFalse_IfItemIsNotInCollection()
    {
        // Arrange
        var collection = new[] { 1, 2, 3 };

        // Act & Assert
        Assert.False(4.IsIn(collection));
    }

    [Fact]
    public void IsIn_WorksWithReferenceTypes()
    {
        // Arrange
        var a = new TestItem { Id = 1, Name = "A" };
        var b = new TestItem { Id = 2, Name = "B" };
        var collection = new[] { a, b };

        // Act & Assert
        Assert.True(a.IsIn(collection));
        Assert.False(new TestItem { Id = 1, Name = "A" }.IsIn(collection)); // different reference
    }

    private class TestItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public override string ToString() => $"TestItem:{Id}:{Name}";
    }
}
