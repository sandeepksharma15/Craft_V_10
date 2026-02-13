using Craft.Domain;
using Craft.QuerySpec;
using Craft.Repositories;
using Craft.Testing.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Testing.TestClasses;

/// <summary>
/// Base class for complete repository integration tests including read, write, and query operations.
/// Inherits all tests from BaseReadRepositoryTests (15 tests) and BaseChangeRepositoryTests (13 tests),
/// and adds query-based operation tests (8 tests) for a total of 36 comprehensive tests automatically!
/// </summary>
/// <typeparam name="TEntity">The entity type being tested</typeparam>
/// <typeparam name="TKey">The primary key type of the entity</typeparam>
/// <typeparam name="TFixture">The database fixture type that implements IRepositoryTestFixture</typeparam>
/// <remarks>
/// Example usage:
/// <code>
/// [Collection(nameof(DatabaseTestCollection))]
/// public class ProductRepositoryTests : BaseRepositoryTests&lt;Product, int, DatabaseFixture&gt;
/// {
///     public ProductRepositoryTests(DatabaseFixture fixture) : base(fixture) { }
///     
///     protected override Product CreateValidEntity()
///     {
///         return new Product { Name = "Test Product", Price = 99.99m };
///     }
/// }
/// </code>
/// This provides 15 read + 13 write + 8 query tests = 36 comprehensive tests automatically!
/// </remarks>
public abstract class BaseRepositoryTests<TEntity, TKey, TFixture> : BaseChangeRepositoryTests<TEntity, TKey, TFixture>
    where TEntity : class, IEntity<TKey>, new()
    where TFixture : class, IRepositoryTestFixture
{
    /// <summary>
    /// Initializes a new instance of the BaseRepositoryTests class.
    /// </summary>
    /// <param name="fixture">The database fixture</param>
    protected BaseRepositoryTests(TFixture fixture) : base(fixture)
    {
    }

    /// <summary>
    /// Creates an instance of the full repository to be tested.
    /// Override this to provide IRepository (QuerySpec) implementation.
    /// Default implementation creates a Repository from Craft.QuerySpec.
    /// </summary>
    protected override IReadRepository<TEntity, TKey> CreateRepository()
    {
        // Create Repository from Craft.QuerySpec
        var logger = Fixture.ServiceProvider
            .GetRequiredService<ILogger<Repository<TEntity, TKey>>>();

        var queryOptions = Fixture.ServiceProvider
            .GetRequiredService<IOptions<QueryOptions>>();

        return new Repository<TEntity, TKey>(Fixture.DbContext, logger, queryOptions);
    }

    /// <summary>
    /// Helper method to get the repository as IRepository (QuerySpec).
    /// </summary>
    protected IRepository<TEntity, TKey> GetFullRepository() => (IRepository<TEntity, TKey>)CreateRepository();

    /// <summary>
    /// Creates a simple query for testing.
    /// Override this to provide entity-specific query logic.
    /// </summary>
    protected virtual Query<TEntity> CreateSimpleQuery() => new();

    /// <summary>
    /// Creates a query with a filter.
    /// Override this for custom filtering logic.
    /// </summary>
    protected virtual Query<TEntity> CreateFilteredQuery(System.Linq.Expressions.Expression<Func<TEntity, bool>> filter)
    {
        var query = new Query<TEntity>();
        query.Where(filter);
        return query;
    }

    #region GetAsync with Query Tests

    [Fact]
    public virtual async Task GetAsync_WithQuery_ReturnsMatchingEntity()
    {
        // Arrange
        var repository = GetFullRepository();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        var query = CreateSimpleQuery();

        // Act
        var result = await repository.GetAsync(query);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public virtual async Task GetAsync_WithQueryNoMatch_ReturnsNull()
    {
        // Arrange
        var repository = GetFullRepository();
        await ClearDatabaseAsync();

        // Create query that filters by impossible condition
        var query = new Query<TEntity>();

        // Add a filter that will match nothing
        var nameProperty = typeof(TEntity).GetProperty("Name");
        if (nameProperty != null)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TEntity), "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, nameProperty);
            var constant = System.Linq.Expressions.Expression.Constant("NonExistent_" + Guid.NewGuid());
            var equals = System.Linq.Expressions.Expression.Equal(property, constant);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<TEntity, bool>>(equals, parameter);
            query.Where(lambda);
        }

        // Act
        var result = await repository.GetAsync(query);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAllAsync with Query Tests

    [Fact]
    public virtual async Task GetAllAsync_WithQuery_ReturnsMatchingEntities()
    {
        // Arrange
        var repository = GetFullRepository();
        var entities = CreateValidEntities(5);
        await SeedDatabaseAsync([.. entities]);

        var query = CreateSimpleQuery();

        // Act
        var result = await repository.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }

    [Fact]
    public virtual async Task GetAllAsync_WithQueryNoMatch_ReturnsEmptyList()
    {
        // Arrange
        var repository = GetFullRepository();
        await ClearDatabaseAsync();

        var query = CreateSimpleQuery();

        // Act
        var result = await repository.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetCountAsync with Query Tests

    [Fact]
    public virtual async Task GetCountAsync_WithQuery_ReturnsCorrectCount()
    {
        // Arrange
        var repository = GetFullRepository();
        var entities = CreateValidEntities(7);
        await SeedDatabaseAsync([.. entities]);

        var query = CreateSimpleQuery();

        // Act
        var count = await repository.GetCountAsync(query);

        // Assert
        Assert.Equal(7, count);
    }

    [Fact]
    public virtual async Task GetCountAsync_WithQueryNoMatch_ReturnsZero()
    {
        // Arrange
        var repository = GetFullRepository();
        await ClearDatabaseAsync();

        var query = CreateSimpleQuery();

        // Act
        var count = await repository.GetCountAsync(query);

        // Assert
        Assert.Equal(0, count);
    }

    #endregion

    #region GetPagedListAsync with Query Tests

    [Fact]
    public virtual async Task GetPagedListAsync_WithQuery_ReturnsPaginatedResults()
    {
        // Arrange
        var repository = GetFullRepository();
        var entities = CreateValidEntities(10);
        await SeedDatabaseAsync([.. entities]);

        var query = CreateSimpleQuery();
        query.SetPage(1, 5); // First page, 5 items

        // Act
        var result = await repository.GetPagedListAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.TotalCount);
        Assert.True(result.Items.Count() <= 5);
    }

    [Fact]
    public virtual async Task GetPagedListAsync_WithQueryNoMatch_ReturnsEmptyPage()
    {
        // Arrange
        var repository = GetFullRepository();
        await ClearDatabaseAsync();

        var query = CreateSimpleQuery();
        query.SetPage(1, 10);

        // Act
        var result = await repository.GetPagedListAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    #endregion
}
