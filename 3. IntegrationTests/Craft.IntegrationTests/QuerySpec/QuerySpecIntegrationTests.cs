using Craft.IntegrationTests.Entities;
using Craft.IntegrationTests.Fixtures;
using Craft.QuerySpec;
using Microsoft.EntityFrameworkCore;

namespace Craft.IntegrationTests.QuerySpec;

/// <summary>
/// Integration tests for QuerySpec with real database operations.
/// Tests specification pattern functionality with EF Core.
/// </summary>
[Collection(nameof(DatabaseTestCollection))]
public class QuerySpecIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IntegrationTestDbContext _dbContext = null!;

    public QuerySpecIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
        _dbContext = _fixture.DbContext;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region Basic Query Tests

    [Fact]
    public async Task Query_WithWhereClause_FiltersCorrectly()
    {
        // Arrange
        var query = new Query<Product>();
        query.Where(p => p.Price > 500m);

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, p => Assert.True(p.Price > 500m));
    }

    [Fact]
    public async Task Query_WithMultipleWhereClauses_AppliesAllFilters()
    {
        // Arrange
        var query = new Query<Product>();
        query.Where(p => p.Price > 100m);
        query.Where(p => p.CategoryId == 1);

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, p =>
        {
            Assert.True(p.Price > 100m);
            Assert.Equal(1, p.CategoryId);
        });
    }

    [Fact]
    public async Task Query_WithOrderBy_SortsCorrectly()
    {
        // Arrange
        var query = new Query<Product>();
        query.OrderBy(p => p.Price);

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.True(results.Count >= 2);
        for (int i = 1; i < results.Count; i++)
            Assert.True(results[i].Price >= results[i - 1].Price);
    }

    [Fact]
    public async Task Query_WithOrderByDescending_SortsCorrectly()
    {
        // Arrange
        var query = new Query<Product>();
        query.OrderByDescending(p => p.Price);

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.True(results.Count >= 2);
        for (int i = 1; i < results.Count; i++)
            Assert.True(results[i].Price <= results[i - 1].Price);
    }

    [Fact]
    public async Task Query_WithTake_LimitsResults()
    {
        // Arrange
        var query = new Query<Product>
        {
            Take = 2
        };

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task Query_WithSkip_SkipsResults()
    {
        // Arrange
        var totalCount = await _dbContext.Products.CountAsync();
        var query = new Query<Product>
        {
            Skip = 2
        };

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.Equal(totalCount - 2, results.Count);
    }

    [Fact]
    public async Task Query_WithSkipAndTake_ReturnsPaginatedResults()
    {
        // Arrange
        var query = new Query<Product>();
        query.OrderBy(p => p.Id);
        query.Skip = 1;
        query.Take = 2;

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task Query_SetPage_ConfiguresCorrectPagination()
    {
        // Arrange
        var query = new Query<Product>();
        query.OrderBy(p => p.Id);
        query.SetPage(page: 1, pageSize: 2);

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal(0, query.Skip);
        Assert.Equal(2, query.Take);
    }

    [Fact]
    public async Task Query_SetPage_SecondPage_SkipsCorrectly()
    {
        // Arrange
        var query = new Query<Product>();
        query.OrderBy(p => p.Id);
        query.SetPage(page: 2, pageSize: 2);

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.Equal(2, query.Skip);
        Assert.Equal(2, query.Take);
    }

    #endregion

    #region Complex Query Tests

    [Fact]
    public async Task Query_ComplexCombination_WorksCorrectly()
    {
        // Arrange
        var query = new Query<Product>();
        query.Where(p => p.TenantId == 1);
        query.Where(p => p.Price >= 50m);
        query.OrderByDescending(p => p.Price);
        query.Take = 3;

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.True(results.Count <= 3);
        Assert.All(results, p =>
        {
            Assert.Equal(1, p.TenantId);
            Assert.True(p.Price >= 50m);
        });

        // Verify ordering
        for (int i = 1; i < results.Count; i++)
            Assert.True(results[i].Price <= results[i - 1].Price);
    }

    [Fact]
    public async Task Query_WithSelect_ProjectsCorrectly()
    {
        // Arrange
        var query = new Query<Product>();
        query.Where(p => p.Price > 100m);

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .Select(p => new { p.Name, p.Price })
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r =>
        {
            Assert.NotEmpty(r.Name);
            Assert.True(r.Price > 100m);
        });
    }

    #endregion

    #region Search Criteria Tests

    [Fact]
    public async Task Query_WithContainsSearch_FindsMatches()
    {
        // Arrange
        var query = new Query<Product>();
        query.Where(p => p.Name.Contains("Laptop"));

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, p => Assert.Contains("Laptop", p.Name));
    }

    [Fact]
    public async Task Query_WithStartsWithSearch_FindsMatches()
    {
        // Arrange
        var query = new Query<Product>();
        query.Where(p => p.Name.StartsWith("Smart"));

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, p => Assert.StartsWith("Smart", p.Name));
    }

    #endregion

    #region Aggregate Tests

    [Fact]
    public async Task Query_Count_ReturnsCorrectCount()
    {
        // Arrange
        var query = new Query<Product>();
        query.Where(p => p.CategoryId == 1);

        // Act
        var count = await _dbContext.Products
            .WithQuery(query)
            .CountAsync();

        // Assert
        Assert.True(count > 0);
    }

    [Fact]
    public async Task Query_Any_ReturnsCorrectResult()
    {
        // Arrange
        var query = new Query<Product>();
        query.Where(p => p.Price > 1000m);

        // Act
        var exists = await _dbContext.Products
            .WithQuery(query)
            .AnyAsync();

        // Assert - we should have no products > 1000
        Assert.False(exists);
    }

    [Fact]
    public async Task Query_FirstOrDefault_ReturnsFirstMatch()
    {
        // Arrange
        var query = new Query<Product>();
        query.Where(p => p.CategoryId == 1);
        query.OrderBy(p => p.Price);

        // Act
        var result = await _dbContext.Products
            .WithQuery(query)
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.CategoryId);
    }

    #endregion

    #region Query Options Tests

    [Fact]
    public async Task Query_AsNoTracking_DoesNotTrackEntities()
    {
        // Arrange
        var query = new Query<Product>
        {
            AsNoTracking = true
        };
        query.Where(p => p.Id == 1);

        // Act
        var product = await _dbContext.Products
            .WithQuery(query)
            .FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(product);

        var entry = _dbContext.Entry(product);
        Assert.Equal(EntityState.Detached, entry.State);
    }

    [Fact]
    public async Task Query_IgnoreQueryFilters_IncludesSoftDeleted()
    {
        // Arrange
        var query = new Query<Product>
        {
            IgnoreQueryFilters = true
        };

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert - should include the soft-deleted product (Id = 5)
        Assert.Contains(results, p => p.IsDeleted);
    }

    [Fact]
    public async Task Query_WithoutIgnoreQueryFilters_ExcludesSoftDeleted()
    {
        // Arrange
        var query = new Query<Product>
        {
            IgnoreQueryFilters = false
        };

        // Act
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Assert - should not include any soft-deleted products
        Assert.All(results, p => Assert.False(p.IsDeleted));
    }

    #endregion

    #region IsSatisfiedBy Tests

    [Fact]
    public void Query_IsSatisfiedBy_ReturnsTrueForMatchingEntity()
    {
        // Arrange
        var query = new Query<Product>();
        query.Where(p => p.Price > 500m);
        query.Where(p => p.CategoryId == 1);

        var matchingProduct = new Product
        {
            Id = 100,
            Name = "Test",
            Price = 600m,
            CategoryId = 1,
            TenantId = 1
        };

        // Act
        var result = query.IsSatisfiedBy(matchingProduct);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Query_IsSatisfiedBy_ReturnsFalseForNonMatchingEntity()
    {
        // Arrange
        var query = new Query<Product>();
        query.Where(p => p.Price > 500m);

        var nonMatchingProduct = new Product
        {
            Id = 100,
            Name = "Test",
            Price = 100m,
            CategoryId = 1,
            TenantId = 1
        };

        // Act
        var result = query.IsSatisfiedBy(nonMatchingProduct);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Complete Query Loop Test

    [Fact]
    public async Task FullQueryLoop_BuildExecuteVerify_WorksCorrectly()
    {
        // Step 1: Create a dynamic query based on "user input"
        var searchTerm = "phone";
        var minPrice = 100m;
        var maxPrice = 700m;
        var categoryId = 1;

        var query = new Query<Product>();
        query.Where(p => p.CategoryId == categoryId);
        query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
        query.Where(p => p.Name.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));
        query.OrderBy(p => p.Name);

        // Step 2: Execute the query
        var results = await _dbContext.Products
            .WithQuery(query)
            .ToListAsync();

        // Step 3: Verify results match all criteria
        Assert.NotEmpty(results);
        Assert.All(results, p =>
        {
            Assert.Equal(categoryId, p.CategoryId);
            Assert.True(p.Price >= minPrice);
            Assert.True(p.Price <= maxPrice);
            Assert.Contains(searchTerm, p.Name.ToLower());
        });

        // Step 4: Verify count matches
        var count = await _dbContext.Products
            .WithQuery(query)
            .CountAsync();
        Assert.Equal(results.Count, count);
    }

    #endregion
}
