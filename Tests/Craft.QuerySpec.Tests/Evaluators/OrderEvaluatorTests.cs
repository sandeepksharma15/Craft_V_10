using Craft.Testing.Fixtures;

namespace Craft.QuerySpec.Tests.Evaluators;

public class OrderEvaluatorTests
{
    private readonly IQueryable<Company> _companies;

    public OrderEvaluatorTests()
    {
        _companies = new List<Company>
        {
            new() { Id = 1, Name = "Company 1" },
            new() { Id = 2, Name = "Company 2" }
        }.AsQueryable();
    }

    [Fact]
    public void GetQuery_WithNoOrderExpressions_ShouldReturnOriginalQueryable()
    {
        // Arrange
        var evaluator = OrderEvaluator.Instance;
        var query = new Query<Company>();

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Same(_companies, result);
    }

    [Fact]
    public void GetQuery_WithSingleOrderBy_ShouldSortAscending()
    {
        // Arrange
        var evaluator = OrderEvaluator.Instance;
        var query = new Query<Company>();
        query.OrderBy(x => x.Name!);

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Equal(2, result?.Count());
        Assert.Equal("Company 1", result?.First().Name);
        Assert.Equal("Company 2", result?.Last().Name);
    }

    [Fact]
    public void GetQuery_WithOrderByDescending_ShouldSortDescending()
    {
        // Arrange
        var evaluator = OrderEvaluator.Instance;
        var query = new Query<Company>();
        query.OrderByDescending(x => x.Id);

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Equal(2, result?.Count());
        Assert.Equal(2, result?.First().Id);
        Assert.Equal(1, result?.Last().Id);
    }

    [Fact]
    public void GetQuery_WithThenBy_ShouldSortByMultipleFields()
    {
        // Arrange
        var evaluator = OrderEvaluator.Instance;
        var companies = new List<Company>
        {
            new() { Id = 1, Name = "B" },
            new() { Id = 2, Name = "A" },
            new() { Id = 3, Name = "A" }
        }.AsQueryable();
        var query = new Query<Company>();
        query?.OrderBy(x => x.Name!)?.ThenBy(x => x.Id);

        // Act
        var result = evaluator.GetQuery(companies, query)?.ToList();

        // Assert
        Assert.Equal(3, result?.Count);
        Assert.Equal(2, result?[0].Id); // A, 2
        Assert.Equal(3, result?[1].Id); // A, 3
        Assert.Equal(1, result?[2].Id); // B, 1
    }

    [Fact]
    public void GetQuery_WithThenByDescending_ShouldSortByMultipleFieldsDescending()
    {
        // Arrange
        var evaluator = OrderEvaluator.Instance;
        var companies = new List<Company>
        {
            new() { Id = 1, Name = "B" },
            new() { Id = 2, Name = "A" },
            new() { Id = 3, Name = "A" }
        }.AsQueryable();
        var query = new Query<Company>();
        query.OrderBy(x => x.Name!)?.ThenByDescending(x => x.Id);

        // Act
        var result = evaluator.GetQuery(companies, query)?.ToList();

        // Assert
        Assert.Equal(3, result?.Count);
        Assert.Equal(3, result?[0].Id); // A, 3
        Assert.Equal(2, result?[1].Id); // A, 2
        Assert.Equal(1, result?[2].Id); // B, 1
    }

    [Fact]
    public void GetQuery_WithDuplicateOrderBy_ThrowsArgumentException()
    {
        // Arrange
        var evaluator = OrderEvaluator.Instance;
        var query = new Query<Company>();

        query.OrderBy(x => x.Id);
        query.OrderBy(x => x.Name!);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => query.OrderByDescending(x => x.Id));
    }

    [Fact]
    public void GetQuery_NullQuery_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = OrderEvaluator.Instance;

        // Act
        var result = evaluator.GetQuery(_companies, null);

        // Assert
        Assert.Same(_companies, result);
    }

    [Fact]
    public void GetQuery_NullQueryable_ReturnsNull()
    {
        // Arrange
        var evaluator = OrderEvaluator.Instance;
        var query = new Query<Company>();
        query.OrderBy(x => x.Id);

        // Assert
        Assert.Throws<ArgumentNullException>(() => evaluator.GetQuery<Company>(null, query));
    }

    [Fact]
    public void GetQuery_EmptyQueryable_ReturnsEmpty()
    {
        // Arrange
        var evaluator = OrderEvaluator.Instance;
        var query = new Query<Company>();
        query.OrderBy(x => x.Id);
        var emptyQueryable = new List<Company>().AsQueryable();

        // Act
        var result = evaluator.GetQuery(emptyQueryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
