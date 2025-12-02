using System.Linq.Expressions;
using Craft.Testing.Fixtures;

namespace Craft.QuerySpec.Tests.Core;

public class QueryWhereExtensionTests
{
    private readonly IQueryable<Company> queryable;

    public QueryWhereExtensionTests()
    {
        queryable = new List<Company>
        {
            new() { Id = 1, Name = "Company 1" },
            new() { Id = 2, Name = "Company 2" }
        }.AsQueryable();
    }

    [Fact]
    public void WhereWithPropertyName_WhenQueryIsNotNull_AddsToWhereBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        var result = query.Where("Name", "Company 2", ComparisonType.EqualTo);
        var expr = result?.EntityFilterBuilder?.EntityFilterList[0];

        // Act
        var filtered = queryable.Where(expr?.Filter!).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(filtered);
        Assert.Single(filtered);
        Assert.Equal("Company 2", filtered[0].Name);
    }

    [Fact]
    public void WhereWithPropertyName_WhenPropertyNameIsNull_DoesNotAddToWhereBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        string propName = null!;
        var expected = query?.EntityFilterBuilder?.EntityFilterList.Count;

        // Act
        var result = query?.Where(propName, null!, ComparisonType.EqualTo);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected, query?.EntityFilterBuilder?.EntityFilterList.Count);
    }

    [Fact]
    public void WhereWithPropertyName_WhenQueryIsNull_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;
        string propName = null!;

        // Act
        var result = query.Where(propName, null!, ComparisonType.EqualTo);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void WhereWithPropertyName_WhenEntityFilterBuilderIsNull_DoesNotThrow()
    {
        // Arrange
        var query = new QueryWithNullEntityFilterBuilder();

        // Act & Assert
        var result = query.Where("Name", "Company 2", ComparisonType.EqualTo);
        Assert.Same(query, result);
    }

    [Fact]
    public void WhereWithProperty_WhenQueryIsNotNull_AddsToWhereBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, object>> propExpr = c => c.Id;

        // Act
        var result = query.Where(propExpr, 2);
        var expr = result?.EntityFilterBuilder?.EntityFilterList[0];
        var filtered = queryable.Where(expr?.Filter!).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(filtered);
        Assert.Single(filtered);
        Assert.Equal("Company 2", filtered[0].Name);
    }

    [Fact]
    public void WhereWithProperty_WhenExpressionIsNull_DoesNotAddToWhereBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, object>> propExpr = null!;
        var expected = query?.EntityFilterBuilder?.EntityFilterList.Count;

        // Act
        var result = query?.Where(propExpr, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected, query?.EntityFilterBuilder?.EntityFilterList.Count);
    }

    [Fact]
    public void WhereWithProperty_WhenQueryIsNull_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;
        Expression<Func<Company, object>> propExpr = c => c.Id;

        // Act
        var result = query.Where(propExpr, 10);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void WhereWithProperty_WhenEntityFilterBuilderIsNull_DoesNotThrow()
    {
        // Arrange
        var query = new QueryWithNullEntityFilterBuilder();
        Expression<Func<Company, object>> propExpr = c => c.Id;

        // Act & Assert
        var result = query.Where(propExpr, 2);
        Assert.Same(query, result);
    }

    [Fact]
    public void WhereWithProperty_WhenCompareWithIsNull_AddsToWhereBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, object>> propExpr = c => c.Name!;

        // Act
        var result = query.Where(propExpr, null!);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result?.EntityFilterBuilder?.EntityFilterList!);
    }

    [Fact]
    public void WhereWithPropertyName_WhenCompareWithIsNull_AddsToWhereBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        var result = query.Where("Name", null!);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result?.EntityFilterBuilder?.EntityFilterList!);
    }

    [Fact]
    public void WhereWithExpression_WhenQueryIsNotNull_AddsToWhereBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, bool>> expression = c => c.Id == 1;

        // Act
        var result = query.Where(expression);
        var expr = result?.EntityFilterBuilder?.EntityFilterList[0];
        var filtered = queryable.Where(expr?.Filter!).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(filtered);
        Assert.Single(filtered);
        Assert.Equal("Company 1", filtered[0].Name);
    }

    [Fact]
    public void WhereWithExpression_WhenExpressionIsNull_DoesNotAddToWhereBuilder()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();
        Expression<Func<Company, bool>> expression = null!;
        var expected = query.EntityFilterBuilder?.EntityFilterList.Count;

        // Act
        var result = query.Where(expression);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected, query.EntityFilterBuilder?.EntityFilterList.Count);
    }

    [Fact]
    public void WhereWithExpression_WhenQueryIsNull_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;
        Expression<Func<Company, bool>> expression = c => c.Id == 1;

        // Act
        var result = query.Where(expression);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void WhereWithExpression_WhenEntityFilterBuilderIsNull_DoesNotThrow()
    {
        // Arrange
        var query = new QueryWithNullEntityFilterBuilder();
        Expression<Func<Company, bool>> expression = c => c.Id == 1;

        // Act & Assert
        var result = query.Where(expression);
        Assert.Same(query, result);
    }

    private class QueryWithNullEntityFilterBuilder : IQuery<Company>
    {
        public bool AsNoTracking { get; set; }
        public bool AsSplitQuery { get; set; }
        public bool IgnoreAutoIncludes { get; set; }
        public bool IgnoreQueryFilters { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public SortOrderBuilder<Company>? SortOrderBuilder { get; set; } = null;
        public Func<IEnumerable<Company>, IEnumerable<Company>>? PostProcessingAction { get; set; }
        public SqlLikeSearchCriteriaBuilder<Company>? SqlLikeSearchCriteriaBuilder { get; set; } = null;
        public EntityFilterBuilder<Company>? EntityFilterBuilder { get; set; } = null;
        public void Clear() { }
        public bool IsSatisfiedBy(Company entity) => true;
        public void SetPage(int page, int pageSize) { }
    }
}
