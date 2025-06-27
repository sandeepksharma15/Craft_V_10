using System.Linq.Expressions;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Builders;

public class ExpressionBuilderTests
{
    private readonly IQueryable<Company> queryable;

    public ExpressionBuilderTests()
    {
        queryable = new List<Company>
        {
            new() { Id = 1, Name = "Company 1" },
            new() { Id = 2, Name = "Company 2" }
        }.AsQueryable();
    }

    [Fact]
    public void CreateNonStringExpressionBody_ShouldReturnCorrectComparisonExpression()
    {
        // Arrange
        FilterCriteria filterInfo = new(typeof(long).FullName!, "Id", "1", ComparisonType.EqualTo);

        // Act
        var expression = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expression).ToList();

        // Assert
        Assert.NotNull(expression);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Company 1", result[0].Name);
    }

    [Fact]
    public void CreateWhereExpression_ShouldCreateValidExpression()
    {
        // Arrange
        Expression<Func<Company, object>> propertyExpression = (x) => x.Name!;
        const string dataValue = "Company 1";
        const ComparisonType comparison = ComparisonType.EqualTo;

        // Act
        var expression = ExpressionBuilder.CreateWhereExpression(propertyExpression, dataValue, comparison);
        var result = queryable.Where(expression).ToList();

        // Assert
        Assert.NotNull(expression);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Company 1", result[0].Name);
    }

    [Fact]
    public void CreateWhereExpression_ShouldReturnCorrectExpression()
    {
        // Arrange
        FilterCriteria filterInfo = new(typeof(string).FullName!, "Name", "Company 1", ComparisonType.EqualTo);

        // Act
        var expression = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expression).ToList();

        // Assert
        Assert.NotNull(expression);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Company 1", result[0].Name);
    }

    [Fact]
    public void GetPropertyExpression_ShouldReturnLambdaExpressionForValidProperty()
    {
        // Arrange
        Company instance = new() { Name = "Sample" };

        // Act
        var expression = ExpressionBuilder.GetPropertyExpression<Company>("Name");

        // Assert
        Assert.NotNull(expression);
        var compiled = expression.Compile();
        Assert.Equal("Sample", compiled(instance));
    }

    [Fact]
    public void GetPropertyExpression_ShouldReturnNullForInvalidProperty()
    {
        // Act
        var expression = ExpressionBuilder.GetPropertyExpression<Company>("InvalidProperty");

        // Assert
        Assert.Null(expression);
    }

    [Fact]
    public void CreateWhereExpression_NullFilterCriteria_ReturnsAlwaysTrue()
    {
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(null!);
        var compiled = expr.Compile();
        Assert.True(compiled(new Company()));
    }

    [Fact]
    public void CreateWhereExpression_InvalidPropertyName_ThrowsArgumentException()
    {
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Invalid", "Test", ComparisonType.EqualTo);
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression<Company>(filterInfo));
    }

    [Fact]
    public void CreateWhereExpression_InvalidTypeName_ThrowsTypeLoadException()
    {
        var filterInfo = new FilterCriteria("NotAType", "Name", "Test", ComparisonType.EqualTo);
        Assert.Throws<NullReferenceException>(() => ExpressionBuilder.CreateWhereExpression<Company>(filterInfo));
    }

    [Fact]
    public void CreateWhereExpression_StringContainsComparison_Works()
    {
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Name", "Company", ComparisonType.Contains);
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void CreateWhereExpression_StringStartsWithComparison_Works()
    {
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Name", "Company", ComparisonType.StartsWith);
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void CreateWhereExpression_StringEndsWithComparison_Works()
    {
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Name", "1", ComparisonType.EndsWith);
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();
        Assert.Single(result);
        Assert.Equal("Company 1", result[0].Name);
    }

    [Fact]
    public void CreateWhereExpression_StringNotEqualToComparison_Works()
    {
        var filterInfo = new FilterCriteria(typeof(string).FullName!, "Name", "Company 1", ComparisonType.NotEqualTo);
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();
        Assert.Single(result);
        Assert.Equal("Company 2", result[0].Name);
    }

    [Fact]
    public void CreateWhereExpression_IntGreaterThanComparison_Works()
    {
        var filterInfo = new FilterCriteria(typeof(long).FullName!, "Id", "1", ComparisonType.GreaterThan);
        var expr = ExpressionBuilder.CreateWhereExpression<Company>(filterInfo);
        var result = queryable.Where(expr).ToList();
        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public void GetPropertyExpression_PrimitiveType_ReturnsNull()
    {
        var expr = ExpressionBuilder.GetPropertyExpression<int>("Length");
        Assert.Null(expr);
    }

    [Fact]
    public void CreateWhereExpression_PropertyExpressionWithInvalidProperty_Throws()
    {
        Expression<Func<Company, object>> propExpr = x => x.GetType(); // Not a mapped property
        Assert.Throws<ArgumentException>(() => ExpressionBuilder.CreateWhereExpression(propExpr, "Test", ComparisonType.EqualTo));
    }
}
