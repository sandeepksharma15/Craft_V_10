using Craft.TestDataStore.Fixtures;
using Craft.TestDataStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.QuerySpec.Tests.Evaluators;

public class SearchEvaluatorTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly IQueryable<Company> _companies;

    public SearchEvaluatorTests()
    {
        var services = new ServiceCollection()
             .AddDbContext<TestDbContext>(options =>
             {
                 options.UseInMemoryDatabase("InMemoryTestDb");
             });

        var serviceProvider = services.BuildServiceProvider();

        // Get The DbContext instance
        _context = serviceProvider.GetRequiredService<TestDbContext>();
        _companies = _context.Companies!.AsNoTracking();
    }

    [Fact]
    public void GetQuery_WithNullQuery_ReturnsOriginalQueryable()
    {
        // Arrange
        _context.Database.EnsureCreated();
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

        // Assert
        Assert.Throws<ArgumentNullException>(() => evaluator.GetQuery<Company>(null, query));
    }

    [Fact]
    public void GetQuery_WithNoSearchCriteria_ReturnsOriginalQueryable()
    {
        // Arrange
        _context.Database.EnsureCreated();
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
        _context.Database.EnsureCreated();
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>();

        // Act
        query.Search(x => x.Name!, "Company%");
        var result = evaluator.GetQuery(_companies, query)?.ToList();

        // Assert
        Assert.Equal(3, result?.Count);
        Assert.All(result!, c => Assert.Contains("Company", c.Name));
    }

    [Fact]
    public void GetQuery_WithMultipleSearchCriteria_FiltersResults()
    {
        // Arrange
        _context.Database.EnsureCreated();
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Store>();

        // Act
        query.Search(x => x.Name!, "A%");
        query.Search(x => x.Name!, "B%");
        var result = evaluator.GetQuery(_context.Stores, query)?.ToList();

        // Assert
        Assert.Equal(3, result?.Count);
        Assert.Contains("ABCEFGH", result?.Select(x => x.Name)!);
        Assert.Contains("AAA", result?.Select(x => x.Name)!);
        Assert.Contains("BBB", result?.Select(x => x.Name)!);
    }

    [Fact]
    public void GetQuery_WithEmptyQueryable_ReturnsEmpty()
    {
        // Arrange
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>();
        var emptyOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var emptyContext = new TestDbContext(emptyOptions);
        emptyContext.Database.EnsureCreated();

        // Act
        query.Search(x => x.Name!, "%Alpha%");
        var emptyQueryable = emptyContext.Companies!;
        var result = evaluator.GetQuery(emptyQueryable, query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetQuery_WithNullSearchCriteriaList_ReturnsOriginalQueryable()
    {
        // Arrange
        _context.Database.EnsureCreated();
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company> { SqlLikeSearchCriteriaBuilder = null };

        // Act
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Equal([.. _companies], result?.ToList());
    }

    [Fact]
    public void GetQuery_WithNullSearchTerm_DoesNotFilter()
    {
        // Arrange
        _context.Database.EnsureCreated();
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
        _context.Database.EnsureCreated();
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
        _context.Database.EnsureCreated();
        var evaluator = SearchEvaluator.Instance;
        var query = new Query<Company>();

        // Act
        query.Search((string)null!, "%Alpha%");
        var result = evaluator.GetQuery(_companies, query);

        // Assert
        Assert.Equal([.. _companies], result?.ToList());
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
