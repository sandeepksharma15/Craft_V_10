using System;
using System.Collections.Generic;
using Xunit;

namespace Craft.Extensions.Tests.Collections;

public class EnumerableExtensionsTests
{
    [Fact]
    public void GetListDataForSelect_Should_Return_Dictionary_With_Correct_Values_When_ValueField_And_DisplayField_Are_Null()
    {
        // Arrange
        var items = new List<string> { "Item 1", "Item 2" };

        // Act
        var result = items.GetListDataForSelect(null!, null!);

        // Assert
        var expected = new Dictionary<string, string>
        {
            { "Item 1", "Item 1" },
            { "Item 2", "Item 2" }
        };

        Assert.Equal(expected.Count, result.Count);

        foreach (var kvp in expected)
        {
            Assert.True(result.ContainsKey(kvp.Key));
            Assert.Equal(kvp.Value, result[kvp.Key]);
        }
    }

    [Fact]
    public void GetListDataForSelect_Should_Return_Dictionary_With_Correct_Values_When_ValueField_And_DisplayField_Are_Provided()
    {
        // Arrange
        var items = new List<ListItem>
        {
            new() { Id = 1, Name = "Item 1" },
            new() { Id = 2, Name = "Item 2" }
        };

        // Act
        var result = items.GetListDataForSelect("Id", "Name");

        // Assert
        var expected = new Dictionary<string, string>
        {
            { "1", "Item 1" },
            { "2", "Item 2" }
        };

        Assert.Equal(expected.Count, result.Count);

        foreach (var kvp in expected)
        {
            Assert.True(result.ContainsKey(kvp.Key));
            Assert.Equal(kvp.Value, result[kvp.Key]);
        }
    }

    [Fact]
    public void GetListDataForSelect_Should_Return_Empty_Dictionary_When_Items_Collection_Is_Empty()
    {
        // Arrange
        var items = new List<ListItem>();

        // Act
        var result = items.GetListDataForSelect("Id", "Name");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetListDataForSelect_Should_Return_Empty_Dictionary_When_Items_Is_Null()
    {
        // Arrange
        List<ListItem>? items = null;

        // Act
        var result = items.GetListDataForSelect("Id", "Name");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetListDataForSelect_Should_Handle_Nonexistent_Properties_Gracefully()
    {
        // Arrange
        var items = new List<ListItem>
        {
            new() { Id = 1, Name = "Item 1" }
        };

        // Act
        var result = items.GetListDataForSelect("NonExistent", "AlsoMissing");

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(string.Empty));
        Assert.Equal(string.Empty, result[string.Empty]);
    }

    [Fact]
    public void GetListDataForSelect_Should_Throw_When_Duplicate_Keys_Are_Produced()
    {
        // Arrange
        var items = new List<ListItem>
        {
            new() { Id = 1, Name = "A" },
            new() { Id = 1, Name = "B" }
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => items.GetListDataForSelect("Id", "Name"));
        Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetListDataForSelect_Should_Handle_Null_Item_In_Collection()
    {
        // Arrange
        var items = new List<string?> { "A", null };

        // Act
        var result = items.GetListDataForSelect(null!, null!);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("A"));
        Assert.Equal("A", result["A"]);
        Assert.True(result.ContainsKey(string.Empty));
        Assert.Equal(string.Empty, result[string.Empty]);
    }

    [Fact]
    public void GetListDataForSelect_Should_Handle_Empty_ValueField_Or_DisplayField()
    {
        // Arrange
        var items = new List<ListItem>
        {
            new() { Id = 1, Name = "A" }
        };

        // Act
        var result1 = items.GetListDataForSelect("", "Name");
        var result2 = items.GetListDataForSelect("Id", "");

        // Assert
        Assert.Single(result1);
        Assert.True(result1.ContainsKey(string.Empty));
        Assert.Equal("A", result1[string.Empty]);

        Assert.Single(result2);
        Assert.True(result2.ContainsKey("1"));
        Assert.Equal(string.Empty, result2["1"]);
    }
}

public class ListItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
