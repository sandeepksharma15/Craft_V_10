namespace Craft.Extensions.Tests.Collections;

public class EnumerableExtensionsTests
{
    // --- GetListDataForSelect Tests ---

    [Fact]
    public void GetListDataForSelect_ReturnsDictionary_WhenValueAndDisplayFieldsAreProvided()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new() { Id = 1, Name = "Alpha" },
            new() { Id = 2, Name = "Beta" }
        };

        // Act
        var result = items.GetListDataForSelect("Id", "Name");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Alpha", result["1"]);
        Assert.Equal("Beta", result["2"]);
    }

    [Fact]
    public void GetListDataForSelect_ReturnsDictionary_WhenValueAndDisplayFieldsAreNull()
    {
        // Arrange
        var items = new List<string> { "A", "B" };

        // Act
        var result = items.GetListDataForSelect(null!, null!);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result["A"]);
        Assert.Equal("B", result["B"]);
    }

    [Fact]
    public void GetListDataForSelect_ReturnsEmptyDictionary_WhenItemsIsNull()
    {
        // Arrange
        List<TestItem>? items = null;

        // Act
        var result = items.GetListDataForSelect("Id", "Name");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetListDataForSelect_ReturnsEmptyDictionary_WhenItemsIsEmpty()
    {
        // Arrange
        var items = new List<TestItem>();

        // Act
        var result = items.GetListDataForSelect("Id", "Name");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetListDataForSelect_HandlesNonexistentProperties_Gracefully()
    {
        // Arrange
        var items = new List<TestItem> { new() { Id = 1, Name = "Alpha" } };

        // Act
        var result = items.GetListDataForSelect("NonExistent", "AlsoMissing");

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(string.Empty));
        Assert.Equal(string.Empty, result[string.Empty]);
    }

    [Fact]
    public void GetListDataForSelect_ThrowsOnDuplicateKeys()
    {
        // Arrange
        var items = new List<TestItem>
        {
            new() { Id = 1, Name = "Alpha" },
            new() { Id = 1, Name = "Beta" }
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => items.GetListDataForSelect("Id", "Name"));
        Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetListDataForSelect_HandlesNullItemInCollection()
    {
        // Arrange
        var items = new List<string?> { "A", null };

        // Act
        var result = items.GetListDataForSelect(null!, null!);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result["A"]);
        Assert.Equal(string.Empty, result[string.Empty]);
    }

    [Fact]
    public void GetListDataForSelect_HandlesEmptyValueFieldOrDisplayField()
    {
        // Arrange
        var items = new List<TestItem> { new() { Id = 1, Name = "Alpha" } };

        // Act
        var result1 = items.GetListDataForSelect("", "Name");
        var result2 = items.GetListDataForSelect("Id", "");

        // Assert
        Assert.Single(result1);
        Assert.Equal("Alpha", result1[string.Empty]);
        Assert.Single(result2);
        Assert.Equal(string.Empty, result2["1"]);
    }

    // --- IsIn Tests ---

    [Fact]
    public void IsIn_ReturnsTrue_WhenItemIsInCollection()
    {
        // Arrange
        int item = 2;
        var collection = new List<int> { 1, 2, 3 };

        // Act
        var result = item.IsIn(collection);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsIn_ReturnsFalse_WhenItemIsNotInCollection()
    {
        // Arrange
        string item = "zebra";
        var collection = new List<string> { "cat", "dog", "elephant" };

        // Act
        var result = item.IsIn(collection);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsIn_ReturnsFalse_WhenCollectionIsEmpty()
    {
        // Arrange
        int item = 1;
        var collection = new List<int>();

        // Act
        var result = item.IsIn(collection);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsIn_ReturnsFalse_WhenCollectionIsNull()
    {
        // Arrange
        string item = "test";
        List<string>? collection = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => item.IsIn(collection!));
    }

    [Fact]
    public void IsIn_WorksWithReferenceTypes()
    {
        // Arrange
        var obj = new TestItem { Id = 1, Name = "Alpha" };
        var collection = new List<TestItem>
        {
            new() { Id = 2, Name = "Beta" },
            obj
        };

        // Act
        var result = obj.IsIn(collection);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsIn_WorksWithValueTypes()
    {
        // Arrange
        var item = DayOfWeek.Monday;
        var collection = new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Monday };

        // Act
        var result = item.IsIn(collection);

        // Assert
        Assert.True(result);
    }

    private class TestItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}

