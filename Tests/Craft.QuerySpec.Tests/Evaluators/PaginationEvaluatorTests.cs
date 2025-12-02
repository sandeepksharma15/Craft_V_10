using Craft.Testing.Fixtures;

namespace Craft.QuerySpec.Tests.Evaluators;

public class PaginationEvaluatorTests
{
    private readonly IQueryable<Company> _companies;

    public PaginationEvaluatorTests()
    {
        _companies = new List<Company>
        {
            new() { Id = 1, Name = "Company 1" },
            new() { Id = 2, Name = "Company 2" }
        }.AsQueryable();
    }

    [Fact]
    public void GetQuery_WithSkip_ReturnsQueryableWithSkip()
    {
        // Arrange
        var evaluator = PaginationEvaluator.Instance;
        var query = new Query<Company> { Skip = 1, Take = null };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Count());
        Assert.Equal("Company 2", result.First().Name);
    }

    [Fact]
    public void GetQuery_WithTake_ReturnsQueryableWithTake()
    {
        // Arrange
        var evaluator = PaginationEvaluator.Instance;
        var query = new Query<Company> { Skip = null, Take = 1 };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Company 1", result.First().Name);
    }

    [Fact]
    public void GetQuery_WithSkipAndTake_ReturnsQueryableWithSkipAndTake()
    {
        // Arrange
        var evaluator = PaginationEvaluator.Instance;
        var query = new Query<Company> { Skip = 1, Take = 1 };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Company 2", result.First().Name);
    }

    [Fact]
    public void GetQuery_WithSkipZero_DoesNotSkip()
    {
        // Arrange
        var evaluator = PaginationEvaluator.Instance;
        var query = new Query<Company> { Skip = 0, Take = 2 };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("Company 1", result.First().Name);
    }

    [Fact]
    public void GetQuery_WithNullQuery_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = PaginationEvaluator.Instance;

        // Act
        var result = evaluator.GetQuery(_companies, null);

        // Assert
        Assert.Equal([.. _companies], result.ToList());
    }

    [Fact]
    public void GetQuery_WithNullQueryable_ThrowsArgumentNullException()
    {
        // Arrange
        var evaluator = PaginationEvaluator.Instance;
        var query = new Query<Company> { Skip = 1, Take = 1 };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => evaluator.GetQuery<Company>(null!, query));
    }

    [Fact]
    public void GetQuery_WithEmptyQueryable_ReturnsEmpty()
    {
        // Arrange
        var evaluator = PaginationEvaluator.Instance;
        var query = new Query<Company> { Skip = 1, Take = 1 };
        var emptyQueryable = new List<Company>().AsQueryable();

        // Act
        var result = evaluator.GetQuery(emptyQueryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetQuery_WithSkipGreaterThanCount_ReturnsEmpty()
    {
        // Arrange
        var evaluator = PaginationEvaluator.Instance;
        var query = new Query<Company> { Skip = 10, Take = 2 };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetQuery_WithTakeGreaterThanCount_ReturnsAllAfterSkip()
    {
        // Arrange
        var evaluator = PaginationEvaluator.Instance;
        var query = new Query<Company> { Skip = 1, Take = 10 };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Single(result);
        Assert.Equal("Company 2", result.First().Name);
    }

    [Fact]
    public void GetQuery_WithNegativeSkip_Throws()
    {
        // Arrange
        var evaluator = PaginationEvaluator.Instance;

        // Act
        var ex = Record.Exception(() => new Query<Company> { Skip = -1, Take = 1 });

        // Assert
        Assert.NotNull(ex);
    }

    [Fact]
    public void GetQuery_WithNegativeTake_Throws()
    {
        // Arrange
        var evaluator = PaginationEvaluator.Instance;

        // Act
        var ex = Record.Exception(() => new Query<Company> { Skip = 0, Take = -1 });

        // Assert
        Assert.NotNull(ex);
    }
}
