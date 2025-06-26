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
}
