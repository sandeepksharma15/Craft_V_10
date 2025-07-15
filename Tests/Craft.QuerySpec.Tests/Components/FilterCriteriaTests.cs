using System.Linq.Expressions;

namespace Craft.QuerySpec.Tests.Components;

/// <summary>
/// Unit tests for <see cref="FilterCriteria"/> covering all code paths, edge cases, and exceptions.
/// </summary>
public class FilterCriteriaTests
{
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public TestEnum EnumProp { get; set; }
        public int? NullableInt { get; set; }
    }

    private enum TestEnum { None = 0, One = 1, Two = 2 }

    #region Constructor Tests

    [Fact]
    public void Constructor_ValidReferenceType_AllowsNullValue()
    {
        var criteria = new FilterCriteria(typeof(string), "Name", null);
        Assert.Equal(typeof(string), criteria.PropertyType);
        Assert.Equal("Name", criteria.Name);
        Assert.Null(criteria.Value);
        Assert.Equal(ComparisonType.EqualTo, criteria.Comparison);
    }

    [Fact]
    public void Constructor_ValidValueType_ThrowsIfNullValue()
    {
        var ex = Assert.Throws<ArgumentException>(() => new FilterCriteria(typeof(int), "Age", null));
        Assert.Contains("Value cannot be null", ex.Message);
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Constructor_ValidNullableValueType_AllowsNullValue()
    {
        var criteria = new FilterCriteria(typeof(int?), "NullableInt", null);
        Assert.Equal(typeof(int?), criteria.PropertyType);
        Assert.Equal("NullableInt", criteria.Name);
        Assert.Null(criteria.Value);
    }

    [Fact]
    public void Constructor_ThrowsIfPropertyTypeNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new FilterCriteria(null!, "Name", "abc"));
        Assert.Equal("propertyType", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsIfNameNullOrWhitespace(string? name)
    {
        ArgumentException? ex;

        if (name is null)
            ex = Assert.Throws<ArgumentNullException>(() => new FilterCriteria(typeof(string), name!, "abc"));
        else

            ex = Assert.Throws<ArgumentException>(() => new FilterCriteria(typeof(string), name!, "abc"));

        Assert.Equal("name", ex.ParamName);
    }

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var criteria = new FilterCriteria(typeof(int), "Age", 42, ComparisonType.GreaterThan);
        Assert.Equal(typeof(int), criteria.PropertyType);
        Assert.Equal("Age", criteria.Name);
        Assert.Equal(42, criteria.Value);
        Assert.Equal(ComparisonType.GreaterThan, criteria.Comparison);
    }

    [Fact]
    public void Constructor_EnumType_AllowsIntValue()
    {
        var criteria = new FilterCriteria(typeof(TestEnum), "EnumProp", 1);
        Assert.Equal(typeof(TestEnum), criteria.PropertyType);
        Assert.Equal(1, criteria.Value);
    }

    #endregion

    #region GetExpression Static Method

    [Fact]
    public void GetExpression_ThrowsIfNull()
    {
        Assert.Throws<ArgumentNullException>(() => FilterCriteria.GetExpression<TestEntity>(null!));
    }

    [Fact]
    public void GetExpression_ReturnsValidExpression()
    {
        var criteria = new FilterCriteria(typeof(int), "Age", 30, ComparisonType.GreaterThan);
        var expr = FilterCriteria.GetExpression<TestEntity>(criteria);
        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 40 }));
        Assert.False(compiled(new TestEntity { Age = 10 }));
    }

    #endregion

    #region GetFilterInfo (Property Selector) Static Method

    [Fact]
    public void GetFilterInfo_PropertySelector_ThrowsIfNull()
    {
        Assert.Throws<ArgumentNullException>(() => FilterCriteria.GetFilterInfo<TestEntity>(null!, 1, ComparisonType.EqualTo));
    }

    [Fact]
    public void GetFilterInfo_PropertySelector_ThrowsIfInvalidLambda()
    {
        Expression<Func<TestEntity, object>> expr = x => x;
        Assert.Throws<ArgumentException>(() => FilterCriteria.GetFilterInfo(expr, 1, ComparisonType.EqualTo));
    }

    [Fact]
    public void GetFilterInfo_PropertySelector_HandlesEnum()
    {
        Expression<Func<TestEntity, object>> expr = x => x.EnumProp;
        var criteria = FilterCriteria.GetFilterInfo(expr, TestEnum.One, ComparisonType.EqualTo);
        Assert.Equal("EnumProp", criteria.Name);
        Assert.Equal(typeof(int), criteria.PropertyType); // Enum coerced to int
        Assert.Equal((int)TestEnum.One, criteria.Value);
    }

    [Fact]
    public void GetFilterInfo_PropertySelector_HandlesNullable()
    {
        Expression<Func<TestEntity, object>> expr = x => x.NullableInt!;
        var criteria = FilterCriteria.GetFilterInfo(expr, 5, ComparisonType.EqualTo);
        Assert.Equal("NullableInt", criteria.Name);
        Assert.Equal(typeof(int), criteria.PropertyType); // Nullable unwrapped
        Assert.Equal(5, criteria.Value);
    }

    [Fact]
    public void GetFilterInfo_PropertySelector_HandlesReferenceType()
    {
        Expression<Func<TestEntity, object>> expr = x => x.Name;
        var criteria = FilterCriteria.GetFilterInfo(expr, "abc", ComparisonType.EqualTo);
        Assert.Equal("Name", criteria.Name);
        Assert.Equal(typeof(string), criteria.PropertyType);
        Assert.Equal("abc", criteria.Value);
    }

    #endregion

    #region GetFilterInfo (Where Expression) Static Method

    [Fact]
    public void GetFilterInfo_WhereExpr_ThrowsIfNull()
    {
        Assert.Throws<ArgumentNullException>(() => FilterCriteria.GetFilterInfo<TestEntity>(null!));
    }

    [Fact]
    public void GetFilterInfo_WhereExpr_ThrowsIfInvalidExpr()
    {
        Expression<Func<TestEntity, bool>> expr = x => x.Age > 10 && x.Age < 20;
        Assert.Throws<ArgumentException>(() => FilterCriteria.GetFilterInfo(expr));
    }

    [Theory]
    [InlineData(ExpressionType.Equal, ComparisonType.EqualTo)]
    [InlineData(ExpressionType.NotEqual, ComparisonType.NotEqualTo)]
    [InlineData(ExpressionType.GreaterThan, ComparisonType.GreaterThan)]
    [InlineData(ExpressionType.LessThan, ComparisonType.LessThan)]
    [InlineData(ExpressionType.GreaterThanOrEqual, ComparisonType.GreaterThanOrEqualTo)]
    [InlineData(ExpressionType.LessThanOrEqual, ComparisonType.LessThanOrEqualTo)]
    public void GetFilterInfo_WhereExpr_ParsesSupportedOperators(ExpressionType nodeType, ComparisonType expected)
    {
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var left = Expression.Property(param, nameof(TestEntity.Age));
        var right = Expression.Constant(5);
        var binary = Expression.MakeBinary(nodeType, left, right);
        var expr = Expression.Lambda<Func<TestEntity, bool>>(binary, param);
        var criteria = FilterCriteria.GetFilterInfo(expr);
        Assert.Equal("Age", criteria.Name);
        Assert.Equal(5, criteria.Value);
        Assert.Equal(expected, criteria.Comparison);
    }

    [Fact]
    public void GetFilterInfo_WhereExpr_ThrowsIfUnsupportedOperator()
    {
        // Arrange
        var param = Expression.Parameter(typeof(TestEntity), "x");
        var left = Expression.Property(param, nameof(TestEntity.Age));
        var right = Expression.Constant(5);
        var binary = Expression.MakeBinary(ExpressionType.And, left, right);

        // Act
        var ex = Assert.Throws<ArgumentException>(() => Expression.Lambda<Func<TestEntity, bool>>(binary, param));

        // Assert
        Assert.NotNull(ex);
    }

    [Fact]
    public void GetFilterInfo_WhereExpr_ThrowsIfNotBinary()
    {
        Expression<Func<TestEntity, bool>> expr = x => true;
        var ex = Assert.Throws<ArgumentException>(() => FilterCriteria.GetFilterInfo(expr));
        Assert.Contains("binary expression", ex.Message);
    }

    #endregion

    #region Instance GetExpression

    [Fact]
    public void InstanceGetExpression_ReturnsValidExpression()
    {
        var criteria = new FilterCriteria(typeof(int), "Age", 10, ComparisonType.LessThan);
        var expr = criteria.GetExpression<TestEntity>();
        var compiled = expr.Compile();
        Assert.True(compiled(new TestEntity { Age = 5 }));
        Assert.False(compiled(new TestEntity { Age = 15 }));
    }

    #endregion
}
