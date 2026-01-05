using Craft.Domain;
using Craft.QuerySpec;
using Craft.Testing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Testing.TestClasses;

/// <summary>
/// Base class for complete HTTP service integration tests including read, write, and query operations.
/// Inherits all tests from BaseReadHttpServiceTests (15 tests) and BaseChangeHttpServiceTests (13 tests),
/// and adds query-based operation tests (8 tests) for a total of 36 comprehensive tests automatically!
/// </summary>
/// <typeparam name="TEntity">The entity type being tested</typeparam>
/// <typeparam name="TViewModel">The view model type for the entity</typeparam>
/// <typeparam name="TDto">The data transfer object type for the entity</typeparam>
/// <typeparam name="TKey">The primary key type of the entity</typeparam>
/// <typeparam name="TFixture">The HTTP service fixture type that implements IHttpServiceTestFixture</typeparam>
/// <remarks>
/// Example usage:
/// <code>
/// [Collection(nameof(HttpServiceTestCollection))]
/// public class BorderOrgHttpServiceTests : BaseHttpServiceTests&lt;BorderOrg, BorderOrgVM, BorderOrgDTO, KeyType, IntegratedFixture&gt;
/// {
///     public BorderOrgHttpServiceTests(IntegratedFixture fixture) : base(fixture) { }
///     
///     protected override string GetApiEndpoint() => "api/borderorg";
///     
///     protected override BorderOrg CreateValidEntity()
///     {
///         return new BorderOrg { Name = "Test Organization" };
///     }
///     
///     protected override BorderOrgVM CreateValidViewModel()
///     {
///         return new BorderOrgVM { Name = "Test Organization" };
///     }
/// }
/// </code>
/// This provides 15 read + 13 write + 8 query tests = 36 comprehensive tests automatically!
/// </remarks>
public abstract class BaseHttpServiceTests<TEntity, TViewModel, TDto, TKey, TFixture> : BaseChangeHttpServiceTests<TEntity, TViewModel, TDto, TKey, TFixture>
    where TEntity : class, IEntity<TKey>, IModel<TKey>, new()
    where TViewModel : class, IModel<TKey>, new()
    where TDto : class, IModel<TKey>, new()
    where TFixture : class, IHttpServiceTestFixture
{
    /// <summary>
    /// Initializes a new instance of the BaseHttpServiceTests class.
    /// </summary>
    /// <param name="fixture">The HTTP service test fixture</param>
    protected BaseHttpServiceTests(TFixture fixture) : base(fixture) { }

    /// <summary>
    /// Creates an instance of the full HTTP service (with QuerySpec support) to be tested.
    /// Default implementation creates an HttpService using the fixture's HttpClient.
    /// Override this method if you need custom service initialization.
    /// </summary>
    protected virtual IHttpService<TEntity, TViewModel, TDto, TKey> CreateHttpService()
    {
        var logger = Fixture.ServiceProvider
            .GetRequiredService<ILogger<HttpService<TEntity, TViewModel, TDto, TKey>>>();

        var apiUrl = new Uri(Fixture.ApiBaseUri, GetApiEndpoint());

        return new HttpService<TEntity, TViewModel, TDto, TKey>(apiUrl, Fixture.HttpClient, logger);
    }

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
    public virtual async Task GetAsync_WithQuery_ReturnsSuccessWithMatchingEntity()
    {
        // Arrange
        var service = CreateHttpService();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        var query = CreateSimpleQuery();
        query.Take = 1; // Add Take to ensure we have a valid query

        // Act
        var result = await service.GetAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success, $"Expected Success=true but got Success=false. StatusCode={result.StatusCode}, Errors={string.Join(", ", result.Errors ?? [])}");
        Assert.NotNull(result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetAsync_WithQueryNoMatch_ReturnsNotFoundResult()
    {
        // Arrange
        var service = CreateHttpService();
        await ClearDatabaseAsync();

        // Create query that filters by impossible condition
        var query = new Query<TEntity>();
        query.Take = 1; // Add Take to ensure we have a valid query

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
        var result = await service.GetAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.NotNull(result.StatusCode);
        Assert.Equal(404, result.StatusCode.Value);
    }

    #endregion

    #region GetAllAsync with Query Tests

    [Fact]
    public virtual async Task GetAllAsync_WithQuery_ReturnsSuccessWithMatchingEntities()
    {
        // Arrange
        var service = CreateHttpService();
        var entities = CreateValidEntities(5);
        await SeedDatabaseAsync([.. entities]);

        var query = CreateSimpleQuery();
        query.Take = 10; // Add Take to ensure we have a valid query

        // Act
        var result = await service.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success, $"Expected Success=true but got Success=false. StatusCode={result.StatusCode}, Errors={string.Join(", ", result.Errors ?? [])}");
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count > 0);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetAllAsync_WithQueryNoMatch_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var service = CreateHttpService();
        await ClearDatabaseAsync();

        var query = CreateSimpleQuery();
        query.Take = 10; // Add Take to ensure we have a valid query

        // Act
        var result = await service.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success, $"Expected Success=true but got Success=false. StatusCode={result.StatusCode}, Errors={string.Join(", ", result.Errors ?? [])}");
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    #endregion

    #region GetCountAsync with Query Tests

    [Fact]
    public virtual async Task GetCountAsync_WithQuery_ReturnsSuccessWithCorrectCount()
    {
        // Arrange
        var service = CreateHttpService();
        var entities = CreateValidEntities(7);
        await SeedDatabaseAsync([.. entities]);

        var query = CreateSimpleQuery();
        query.Take = 100; // Add Take to ensure we have a valid query

        // Act
        var result = await service.GetCountAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success, $"Expected Success=true but got Success=false. StatusCode={result.StatusCode}, Errors={string.Join(", ", result.Errors ?? [])}");
        Assert.Equal(7, result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetCountAsync_WithQueryNoMatch_ReturnsSuccessWithZero()
    {
        // Arrange
        var service = CreateHttpService();
        await ClearDatabaseAsync();

        var query = CreateSimpleQuery();
        query.Take = 100; // Add Take to ensure we have a valid query

        // Act
        var result = await service.GetCountAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success, $"Expected Success=true but got Success=false. StatusCode={result.StatusCode}, Errors={string.Join(", ", result.Errors ?? [])}");
        Assert.Equal(0, result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    #endregion

    #region GetPagedListAsync with Query Tests

    [Fact]
    public virtual async Task GetPagedListAsync_WithQuery_ReturnsSuccessWithPaginatedResults()
    {
        // Arrange
        var service = CreateHttpService();
        var entities = CreateValidEntities(10);
        await SeedDatabaseAsync([.. entities]);

        var query = CreateSimpleQuery();
        query.SetPage(1, 5); // First page, 5 items

        // Act
        var result = await service.GetPagedListAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(10, result.Data.TotalCount);
        Assert.True(result.Data.Items.Count() <= 5);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetPagedListAsync_WithQueryNoMatch_ReturnsSuccessWithEmptyPage()
    {
        // Arrange
        var service = CreateHttpService();
        await ClearDatabaseAsync();

        var query = CreateSimpleQuery();
        query.SetPage(1, 10);

        // Act
        var result = await service.GetPagedListAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success, $"Expected Success=true but got Success=false. StatusCode={result.StatusCode}, Errors={string.Join(", ", result.Errors ?? [])}");
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data.TotalCount);
        Assert.Empty(result.Data.Items);
        Assert.Equal(200, result.StatusCode);
    }

    #endregion

    #region DeleteAsync with Query Tests

    [Fact]
    public virtual async Task DeleteAsync_WithQuery_ReturnsSuccessResult()
    {
        // Arrange
        var service = CreateHttpService();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync([.. entities]);

        // Create a query to match the entities
        var query = CreateSimpleQuery();

        // Act
        var result = await service.DeleteAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Equal(200, result.StatusCode);

        // Verify entities were deleted
        Fixture.DbContext.ChangeTracker.Clear();

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            // Soft delete - entities should still exist but marked as deleted
            var allEntities = Fixture.DbContext.Set<TEntity>()
                .IgnoreQueryFilters()
                .ToList();
            Assert.Equal(3, allEntities.Count);
            Assert.All(allEntities, e => Assert.True(((ISoftDelete)e).IsDeleted));
        }
        else
        {
            // Hard delete - entities should not exist
            var count = Fixture.DbContext.Set<TEntity>().Count();
            Assert.Equal(0, count);
        }
    }

    [Fact]
    public virtual async Task DeleteAsync_WithQueryNoMatch_ReturnsSuccessResult()
    {
        // Arrange
        var service = CreateHttpService();
        await ClearDatabaseAsync();

        // Create query that matches nothing
        var query = new Query<TEntity>();
        query.Take = 100; // Add Take to ensure we have a valid query

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
        var result = await service.DeleteAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success, $"Expected Success=true but got Success=false. StatusCode={result.StatusCode}, Errors={string.Join(", ", result.Errors ?? [])}");
        Assert.True(result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    #endregion
}

/// <summary>
/// Base class for complete HTTP service integration tests with default KeyType.
/// Provides comprehensive testing for read, write, and query operations via HTTP services.
/// </summary>
/// <typeparam name="TEntity">The entity type being tested</typeparam>
/// <typeparam name="TViewModel">The view model type for the entity</typeparam>
/// <typeparam name="TDto">The data transfer object type for the entity</typeparam>
/// <typeparam name="TFixture">The HTTP service fixture type that implements IHttpServiceTestFixture</typeparam>
public abstract class BaseHttpServiceTests<TEntity, TViewModel, TDto, TFixture> 
    : BaseHttpServiceTests<TEntity, TViewModel, TDto, KeyType, TFixture>
    where TEntity : class, IEntity, IModel, new()
    where TViewModel : class, IModel, new()
    where TDto : class, IModel, new()
    where TFixture : class, IHttpServiceTestFixture
{
    protected BaseHttpServiceTests(TFixture fixture) : base(fixture) { }
}
