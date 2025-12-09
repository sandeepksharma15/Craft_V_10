namespace Craft.Extensions.Tests.Collections;

public class CollectionExtensionsTests
{
    [Fact]
    public void IsNullOrEmpty_WithEmptyCollection_ReturnsTrue()
    {
        // Arrange
        ICollection<int> collection = [];

        // Assert
        Assert.True(collection.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_WithNonEmptyCollection_ReturnsFalse()
    {
        // Arrange
        ICollection<int> collection = [1, 2, 3];

        // Assert
        Assert.False(collection.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_WithNullCollection_ReturnsTrue()
    {
        // Arrange
        ICollection<int>? collection = null;

        // Act & Assert
        Assert.True(collection!.IsNullOrEmpty());
    }

    [Fact]
    public void AddIfNotContains_AddsItemIfNotPresent_ReturnsTrue()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        bool added = list.AddIfNotContains(4);

        // Assert
        Assert.True(added);
        Assert.Contains(4, list);
    }

    [Fact]
    public void AddIfNotContains_DoesNotAddIfPresent_ReturnsFalse()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        bool added = list.AddIfNotContains(2);

        // Assert
        Assert.False(added);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void AddIfNotContains_ThrowsIfSourceIsNull()
    {
        // Arrange
        List<int>? list = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => list!.AddIfNotContains(1));
    }

    [Fact]
    public void AddIfNotContains_Enumerable_AddsOnlyMissingItems()
    {
        // Arrange
        var list = new List<int> { 1, 2 };

        // Act
        var added = list.AddIfNotContains([2, 3, 4]).ToList();

        // Assert
        Assert.Contains(3, list);
        Assert.Contains(4, list);
        Assert.DoesNotContain(5, list);
        Assert.Equal(new[] { 3, 4 }, added);
    }

    [Fact]
    public void AddIfNotContains_Enumerable_ThrowsIfSourceIsNull()
    {
        // Arrange
        List<int>? list = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => list!.AddIfNotContains([1]));
    }

    [Fact]
    public void AddIfNotContains_Enumerable_ThrowsIfItemsIsNull()
    {
        // Arrange
        var list = new List<int> { 1 };
        IEnumerable<int>? items = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => list.AddIfNotContains(items!));
    }

    [Fact]
    public void AddIfNotContains_Enumerable_AddsNothingIfAllPresent()
    {
        // Arrange
        var list = new List<int> { 1, 2 };

        // Act
        var added = list.AddIfNotContains([1, 2]);

        // Assert
        Assert.Empty(added);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void AddIfNotContains_Predicate_AddsIfPredicateNotSatisfied()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        bool added = list.AddIfNotContains(x => x == 4, () => 4);

        // Assert
        Assert.True(added);
        Assert.Contains(4, list);
    }

    [Fact]
    public void AddIfNotContains_Predicate_DoesNotAddIfPredicateSatisfied()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        bool added = list.AddIfNotContains(x => x == 2, () => 2);

        // Assert
        Assert.False(added);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void AddIfNotContains_Predicate_ThrowsIfSourceIsNull()
    {
        // Arrange
        List<int>? list = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => list!.AddIfNotContains(x => true, () => 1));
    }

    [Fact]
    public void AddIfNotContains_Predicate_ThrowsIfPredicateIsNull()
    {
        // Arrange
        var list = new List<int> { 1 };
        Func<int, bool>? predicate = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => list.AddIfNotContains(predicate!, () => 2));
    }

    [Fact]
    public void AddIfNotContains_Predicate_ThrowsIfItemFactoryIsNull()
    {
        // Arrange
        var list = new List<int> { 1 };
        Func<int>? factory = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => list.AddIfNotContains(x => true, factory!));
    }

    [Fact]
    public void RemoveAll_Enumerable_RemovesAllSpecifiedItems()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4 };

        // Act
        list.RemoveAll([2, 3]);

        // Assert
        Assert.DoesNotContain(2, list);
        Assert.DoesNotContain(3, list);
        Assert.Contains(1, list);
        Assert.Contains(4, list);
    }

    [Fact]
    public void RemoveAll_Enumerable_ThrowsIfSourceIsNull()
    {
        // Arrange
        List<int>? list = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => list!.RemoveAll([1]));
    }

    [Fact]
    public void RemoveAll_Enumerable_ThrowsIfItemsIsNull()
    {
        // Arrange
        var list = new List<int> { 1 };
        IEnumerable<int>? items = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => list.RemoveAll(items!));
    }

    [Fact]
    public void RemoveAll_Enumerable_DoesNothingIfItemsNotPresent()
    {
        // Arrange
        var list = new List<int> { 1, 2 };

        // Act
        list.RemoveAll([3, 4]);

        // Assert
        Assert.Equal(new[] { 1, 2 }, list);
    }
}
