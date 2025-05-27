namespace Craft.Extensions.Tests.Collections;

public class EnumerableExtensionsTests
{
    [Fact]
    public void GetListDataForSelect_ReturnsEmptyDictionary_WhenItemsIsNull()
    {
        IEnumerable<TestItem>? items = null;
        var result = items.GetListDataForSelect("Id", "Name");
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetListDataForSelect_UsesProperties_WhenFieldsAreValid()
    {
        var items = new[]
        {
            new TestItem { Id = 1, Name = "A" },
            new TestItem { Id = 2, Name = "B" }
        };
        var result = items.GetListDataForSelect("Id", "Name");
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result["1"]);
        Assert.Equal("B", result["2"]);
    }

    [Fact]
    public void GetListDataForSelect_HandlesNullPropertyValues()
    {
        var items = new[]
        {
            new TestItem { Id = 1, Name = null }
        };
        var result = items.GetListDataForSelect("Id", "Name");
        Assert.Single(result);
        Assert.Equal(string.Empty, result["1"]);
    }

    [Fact]
    public void GetListDataForSelect_UsesToString_WhenFieldsAreNull()
    {
        var items = new[]
        {
            new TestItem { Id = 1, Name = "A" }
        };
        var result = items.GetListDataForSelect(null!, null!);
        var expectedKey = items[0].ToString();
        Assert.Single(result);
        Assert.Equal(expectedKey, result.Keys.First());
        Assert.Equal(expectedKey, result.Values.First());
    }

    [Fact]
    public void GetListDataForSelect_HandlesNullItem_WhenFieldsAreNull()
    {
        TestItem?[] items = { null };
        var result = items.GetListDataForSelect(null!, null!);
        Assert.Single(result);
        Assert.Equal(string.Empty, result.Keys.First());
        Assert.Equal(string.Empty, result.Values.First());
    }

    [Fact]
    public void GetListDataForSelect_HandlesNullItem_WhenFieldsAreNotNull()
    {
        TestItem?[] items = { null };

        var result = items.GetListDataForSelect("Id", "Name");

        Assert.Single(result);
        Assert.Equal(string.Empty, result.Keys.First());
        Assert.Equal(string.Empty, result.Values.First());
    }

    [Fact]
    public void GetListDataForSelect_HandlesMissingProperty()
    {
        var items = new[]
        {
            new TestItem { Id = 1, Name = "A" }
        };
        var result = items.GetListDataForSelect("NonExistent", "Name");
        Assert.Single(result);
        Assert.Equal(string.Empty, result.Keys.First());
        Assert.Equal("A", result.Values.First());
    }

    [Fact]
    public void GetListDataForSelect_ThrowsOnDuplicateKeys()
    {
        var items = new[]
        {
            new TestItem { Id = 1, Name = "A" },
            new TestItem { Id = 1, Name = "B" }
        };
        Assert.Throws<ArgumentException>(() => items.GetListDataForSelect("Id", "Name"));
    }

    [Fact]
    public void IsIn_ReturnsTrue_IfItemIsInCollection()
    {
        var collection = new[] { 1, 2, 3 };
        Assert.True(2.IsIn(collection));
    }

    [Fact]
    public void IsIn_ReturnsFalse_IfItemIsNotInCollection()
    {
        var collection = new[] { 1, 2, 3 };
        Assert.False(4.IsIn(collection));
    }

    [Fact]
    public void IsIn_WorksWithReferenceTypes()
    {
        var a = new TestItem { Id = 1, Name = "A" };
        var b = new TestItem { Id = 2, Name = "B" };
        var collection = new[] { a, b };
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
