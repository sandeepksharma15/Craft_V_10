using Craft.Utilities.Helpers;

namespace Craft.Utilities.Tests.Helpers;

public class RandomHelperTests
{
    [Fact]
    public void GenerateRandomizedList_ReturnsPermutationOfInput()
    {
        // Arrange
        var input = Enumerable.Range(1, 10).ToList();

        // Act
        var result = RandomHelper.GenerateRandomizedList(input);

        // Assert
        Assert.Equal(input.Count, result.Count);
        Assert.True(input.All(result.Contains));
        Assert.False(input.SequenceEqual(result));
    }

    [Fact]
    public void GenerateRandomizedList_ThrowsOnNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => RandomHelper.GenerateRandomizedList<int>(null!));
    }

    [Fact]
    public void GetRandom_MinMax_ReturnsInRange()
    {
        // Test multiple times to ensure randomness
        for (int i = 0; i < 100; i++)
        {
            int value = RandomHelper.GetRandom(5, 10);
            Assert.InRange(value, 5, 9);
        }
    }

    [Fact]
    public void GetRandom_Max_ReturnsInRange()
    {
        // Test multiple times to ensure randomness
        for (int i = 0; i < 100; i++)
        {
            int value = RandomHelper.GetRandom(10);
            Assert.InRange(value, 0, 9);
        }
    }

    [Fact]
    public void GetRandom_NoArgs_ReturnsNonNegative()
    {
        // Test multiple times to ensure randomness
        for (int i = 0; i < 100; i++)
        {
            int value = RandomHelper.GetRandom();
            Assert.InRange(value, 0, int.MaxValue);
        }
    }

    [Fact]
    public void GetRandomOf_ReturnsOneOfValues()
    {
        // Test multiple times to ensure randomness
        var values = new[] { "a", "b", "c" };
        var result = RandomHelper.GetRandomOf(values);

        // Assert that the result is one of the values
        Assert.Contains(result, values);
    }

    [Fact]
    public void GetRandomOf_ThrowsOnNullOrEmpty()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => RandomHelper.GetRandomOf<string>(null!));
        Assert.Throws<ArgumentException>(() => RandomHelper.GetRandomOf<string>());
    }

    [Fact]
    public void GetRandomOfList_ReturnsOneOfList()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        var result = RandomHelper.GetRandomOfList(list);

        // Assert
        Assert.Contains(result, list);
    }

    [Fact]
    public void GetRandomOfList_ThrowsOnNullOrEmpty()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => RandomHelper.GetRandomOfList<int>(null!));
        Assert.Throws<ArgumentException>(() => RandomHelper.GetRandomOfList(new List<int>()));
    }

    [Fact]
    public void GenerateRandomizedList_IsRandom()
    {
        // Arrange
        var input = Enumerable.Range(1, 20).ToList();

        // Act
        var r1 = RandomHelper.GenerateRandomizedList(input);
        var r2 = RandomHelper.GenerateRandomizedList(input);
        
        // Assert
        Assert.False(r1.SequenceEqual(r2));
    }

    [Fact]
    public void GetRandomOf_IsRandom()
    {
        // Arrange
        var values = new[] { 1, 2, 3, 4, 5 };
        var results = new HashSet<int>();

        // Act
        for (int i = 0; i < 20; i++)
            results.Add(RandomHelper.GetRandomOf(values));

        // Assert
        Assert.True(results.Count > 1);
    }
}
