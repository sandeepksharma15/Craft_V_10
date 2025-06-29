using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Evaluators;

public class SearchEvaluatorTests
{
    private readonly IQueryable<Company> _companies;

    public SearchEvaluatorTests()
    {
        _companies = new List<Company>
        {
            new() { Id = 1, Name = "Alpha" },
            new() { Id = 2, Name = "Beta" },
            new() { Id = 3, Name = "Gamma" },
            new() { Id = 4, Name = "Alphabeta" },
            new() { Id = 5, Name = null }
        }.AsQueryable();
    }

    [Fact]
    public void GetQuery_WithNullQuery_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = SearchEvaluator.Instance;

        // Act
        var result = evaluator.GetQuery(_companies, null);

        // Assert
        Assert.Equal([.. _companies], result?.ToList());
    }

    [Fact]
    public void GetQuery_WithNullQueryable_ReturnsNull()
    {
        // Arrange
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>();

        // Act
        var result = evaluator.GetQuery<Company>(null, query);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetQuery_WithNoSearchCriteria_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>();

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Equal([.. _companies], result?.ToList());
    }

    [Fact]
    public void GetQuery_WithSingleSearchCriteria_FiltersResults()
    {
        // Arrange
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>();
        query.Search(x => x.Name!, "%Alpha%");

        // Act
        var result = evaluator?.GetQuery(_companies, query)?.ToList();

        // Assert
        Assert.Equal(2, result?.Count);
        Assert.All(result!, c => Assert.Contains("Alpha", c.Name));
    }

    [Fact]
    public void GetQuery_WithMultipleSearchCriteria_FiltersResults()
    {
        // Arrange
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>();
        query.Search(x => x.Name!, "%Alpha%");
        query.Search(x => x.Name!, "%Beta%");

        // Act
        var result = evaluator?.GetQuery(_companies, query)?.ToList();

        // Assert
        Assert.Equal(3, result?.Count);
        Assert.Contains(result!, c => c.Name == "Alpha");
        Assert.Contains(result!, c => c.Name == "Alphabeta");
        Assert.Contains(result!, c => c.Name == "Beta");
    }

    [Fact]
    public void GetQuery_WithEmptyQueryable_ReturnsEmpty()
    {
        // Arrange
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>();
        query.Search(x => x.Name!, "%Alpha%");
        var emptyQueryable = new List<Company>().AsQueryable();

        // Act & Assert
        var result = evaluator.GetQuery(emptyQueryable, query);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetQuery_WithNullSearchCriteriaList_ReturnsOriginalQueryable()
    {
        // Arrange
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>
        {
            SqlLikeSearchCriteriaBuilder = null
        };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Equal([.. _companies], result?.ToList());
    }

    [Fact]
    public void GetQuery_WithNullSearchTerm_DoesNotFilter()
    {
        // Arrange
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>();

        // Act
        query.Search(x => x.Name!, null!);
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Equal([.. _companies], result?.ToList());
    }

    [Fact]
    public void GetQuery_WithNullMember_DoesNotFilter()
    {
        // Arrange
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>();

        // Act
        query.Search((System.Linq.Expressions.Expression<Func<Company, object>>)null!, "%Alpha%");
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Equal([.. _companies], result?.ToList());
    }

    [Fact]
    public void GetQuery_WithNullMemberName_DoesNotFilter()
    {
        // Arrange
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>();

        // Act
        query.Search((string)null!, "%Alpha%");
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Equal([.. _companies], result?.ToList());
    }
}
