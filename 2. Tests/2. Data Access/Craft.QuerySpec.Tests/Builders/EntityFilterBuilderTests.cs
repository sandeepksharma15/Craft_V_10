using System.Linq.Expressions;

namespace Craft.QuerySpec.Tests.Builders;

public class EntityFilterBuilderTests
{
    // Test entity for filter operations
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    [Fact]
    public void Ctor_Initializes_EmptyList_And_CountIsZero()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Assert.NotNull(builder.EntityFilterList);
        Assert.Empty(builder.EntityFilterList);
        Assert.Equal(0, builder.Count);
    }

    [Fact]
    public void Add_ByExpression_AddsCriteria_And_ReturnsSelf()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Expression<Func<TestEntity, bool>> expr = x => x.Age > 18;
        var result = builder.Add(expr);
        Assert.Single(builder.EntityFilterList);
        Assert.Equal(builder, result);
        Assert.Equal(expr.ToString(), builder.EntityFilterList[0].Filter.ToString());
    }

    [Fact]
    public void Add_ByExpression_ThrowsOnNull()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<TestEntity, bool>>)null!));
    }

    [Fact]
    public void Add_ByPropertyExpression_AddsCriteria_And_ReturnsSelf()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Expression<Func<TestEntity, object>> prop = x => x.Name;
        var result = builder.Add(prop, "John");
        Assert.Single(builder.EntityFilterList);
        Assert.Equal(builder, result);
    }

    [Fact]
    public void Add_ByPropertyExpression_ThrowsOnNull()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<TestEntity, object>>)null!, "John"));
    }

    [Fact]
    public void Add_ByPropertyName_AddsCriteria_And_ReturnsSelf()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        var result = builder.Add("Name", "John");
        Assert.Single(builder.EntityFilterList);
        Assert.Equal(builder, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_ByPropertyName_ThrowsOnNullOrWhitespace(string? propName)
    {
        var builder = new EntityFilterBuilder<TestEntity>();

        if (propName is null)
            Assert.Throws<ArgumentNullException>(() => builder.Add(propName!, "John"));
        else
            Assert.Throws<ArgumentException>(() => builder.Add(propName!, "John"));
    }

    [Fact]
    public void Add_ByPropertyName_ThrowsOnNonExistentProperty()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Assert.Throws<ArgumentException>(() => builder.Add("NotAProp", "value"));
    }

    [Fact]
    public void Add_MultipleCriteria_IncreasesCount()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        builder.Add(x => x.Age > 18)
               .Add(x => x.Name == "John")
               .Add("IsActive", true);
        Assert.Equal(3, builder.Count);
    }

    [Fact]
    public void Add_DuplicateCriteria_AllowsDuplicates()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        builder.Add(x => x.Age > 18)
               .Add(x => x.Age > 18);
        Assert.Equal(2, builder.Count);
    }

    [Fact]
    public void Remove_ByExpression_RemovesMatchingCriteria()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Expression<Func<TestEntity, bool>> expr = x => x.Age > 18;
        builder.Add(expr);
        builder.Remove(expr);
        Assert.Empty(builder.EntityFilterList);
    }

    [Fact]
    public void Remove_ByExpression_ThrowsOnNull()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Assert.Throws<ArgumentNullException>(() => builder.Remove(null!));
    }

    [Fact]
    public void Remove_ByExpression_DoesNothingIfNotFound()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        builder.Add(x => x.Age > 18);
        builder.Remove(x => x.Name == "John");
        Assert.Single(builder.EntityFilterList);
    }

    [Fact]
    public void Remove_ByPropertyExpression_RemovesMatchingCriteria()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Expression<Func<TestEntity, object>> prop = x => x.Name;
        builder.Add(prop, "John");
        builder.Remove(prop, "John");
        Assert.Empty(builder.EntityFilterList);
    }

    [Fact]
    public void Remove_ByPropertyExpression_ThrowsOnNull()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Assert.Throws<ArgumentNullException>(() => builder.Remove((Expression<Func<TestEntity, object>>)null!, "John"));
    }

    [Fact]
    public void Remove_ByPropertyName_RemovesMatchingCriteria()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        builder.Add("Name", "John");
        builder.Remove("Name", "John");
        Assert.Empty(builder.EntityFilterList);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Remove_ByPropertyName_ThrowsOnNullOrWhitespace(string? propName)
    {
        var builder = new EntityFilterBuilder<TestEntity>();

        if (propName is null)
            Assert.Throws<ArgumentNullException>(() => builder.Remove(propName!, "John"));
        else
            Assert.Throws<ArgumentException>(() => builder.Remove(propName!, "John"));
    }

    [Fact]
    public void Remove_ByPropertyName_ThrowsOnNonExistentProperty()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Assert.Throws<ArgumentException>(() => builder.Remove("NotAProp", "value"));
    }

    [Fact]
    public void Remove_ByPropertyName_DoesNothingIfNotFound()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        builder.Add("Name", "John");
        builder.Remove("Name", "Jane");
        Assert.Single(builder.EntityFilterList);
    }

    [Fact]
    public void Clear_RemovesAllCriteria_And_ReturnsSelf()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        builder.Add(x => x.Age > 18).Add(x => x.Name == "John");
        var result = builder.Clear();
        Assert.Empty(builder.EntityFilterList);
        Assert.Equal(builder, result);
    }

    [Fact]
    public void Clear_OnEmptyBuilder_DoesNotThrow()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        builder.Clear();
        Assert.Empty(builder.EntityFilterList);
    }

    [Fact]
    public void Add_WithAllComparisonTypes_DoesNotThrow()
    {
        var strCompareres = new[]
        {
            ComparisonType.EqualTo,
            ComparisonType.NotEqualTo,
            ComparisonType.Contains,
            ComparisonType.StartsWith,
            ComparisonType.EndsWith
        };

        var builder = new EntityFilterBuilder<TestEntity>();

        foreach (ComparisonType cmp in strCompareres)
            builder.Add(x => x.Name, "test", cmp);

        Assert.Equal(strCompareres.Length, builder.Count);
    }

    [Fact]
    public void Remove_WithAllComparisonTypes_DoesNotThrow()
    {
        var strCompareres = new[]
        {
            ComparisonType.EqualTo,
            ComparisonType.NotEqualTo,
            ComparisonType.Contains,
            ComparisonType.StartsWith,
            ComparisonType.EndsWith
        };

        var builder = new EntityFilterBuilder<TestEntity>();

        foreach (ComparisonType cmp in strCompareres)
            builder.Add(x => x.Name, "test", cmp);

        foreach (ComparisonType cmp in strCompareres)
            builder.Remove(x => x.Name, "test", cmp);

        Assert.Empty(builder.EntityFilterList);
    }

    [Fact]
    public void Add_ByExpression_WithReducibleExpression_ReducesBeforeAdd()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        // Expression that can be reduced: (x => (x.Age > 18))
        Expression<Func<TestEntity, bool>> expr = x => (x.Age > 18);
        builder.Add(expr);
        Assert.Single(builder.EntityFilterList);
        Assert.Equal(expr.ToString(), builder.EntityFilterList[0].Filter.ToString());
    }

    [Fact]
    public void Remove_ByExpression_WithReducibleExpression_ReducesBeforeRemove()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        Expression<Func<TestEntity, bool>> expr = x => (x.Age > 18);
        builder.Add(expr);
        builder.Remove(expr);
        Assert.Empty(builder.EntityFilterList);
    }
}
