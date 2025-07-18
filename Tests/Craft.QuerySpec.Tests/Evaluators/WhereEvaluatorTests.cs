using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Evaluators;

public class WhereEvaluatorTests
{
    private readonly IQueryable<Company> _companies;

    public WhereEvaluatorTests()
    {
        _companies = new List<Company>
        {
            new() { Id = 1, Name = "Company 1" },
            new() { Id = 2, Name = "Company 2" }
        }.AsQueryable();
    }

    [Fact]
    public void GetQuery_WhereExpressions_ReturnsFilteredQueryable()
    {
        // Arrange
        var evaluator = WhereEvaluator.Instance;
        var query = new Query<Company>();
        query.Where(u => u.Name == "Company 1");

        // Act
        var result = evaluator.GetQuery(_companies, query)?.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Company 1", result[0].Name);
    }

    [Fact]
    public void GetQuery_WithNoWhereExpressions_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = WhereEvaluator.Instance;
        var query = new Query<Company>();

        // Act
        var result = evaluator.GetQuery(_companies, query)?.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal([.. _companies], result);
    }

    [Fact]
    public void GetQuery_WithNullQuery_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = WhereEvaluator.Instance;

        // Act
        var result = evaluator.GetQuery(_companies, null)?.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal([.. _companies], result);
    }

    [Fact]
    public void GetQuery_WithNullQueryable_ReturnsNull()
    {
        // Arrange
        var evaluator = WhereEvaluator.Instance;
        var query = new Query<Company>();

        // Assert
        Assert.Throws<ArgumentNullException>(() => evaluator.GetQuery<Company>(null, query));
    }

    [Fact]
    public void GetQuery_WithEmptyQueryable_ReturnsEmpty()
    {
        // Arrange
        var evaluator = WhereEvaluator.Instance;
        var query = new Query<Company>();
        query.Where(u => u.Name == "Company 1");
        var emptyQueryable = new List<Company>().AsQueryable();

        // Act
        var result = evaluator.GetQuery(emptyQueryable, query)?.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetQuery_WithMultipleWhereExpressions_ReturnsCorrectResults()
    {
        // Arrange
        var evaluator = WhereEvaluator.Instance;
        var query = new Query<Company>();
        query.Where(u => u.Name!.Contains("Company"));
        query.Where(u => u.Id == 2);

        // Act
        var result = evaluator.GetQuery(_companies, query)?.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
        Assert.Equal("Company 2", result[0].Name);
    }
}
