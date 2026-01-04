using System.Linq.Expressions;
using Craft.Controllers;
using Craft.Core;
using Craft.Domain;
using Craft.QuerySpec;
using Craft.QuerySpec.Services;
using Craft.Repositories;
using Craft.Testing.Abstractions;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Testing.TestClasses;

/// <summary>
/// Base class for complete EntityController integration tests.
/// Inherits all tests from BaseEntityReadControllerTests (12 tests) and BaseEntityChangeControllerTests (13 tests),
/// and adds query-based operation tests (8 tests) for a total of 33 comprehensive tests automatically!
/// </summary>
/// <typeparam name="TEntity">The entity type being tested</typeparam>
/// <typeparam name="TDto">The DTO type for the entity</typeparam>
/// <typeparam name="TKey">The primary key type of the entity</typeparam>
/// <typeparam name="TFixture">The test fixture type that implements IRepositoryTestFixture</typeparam>
/// <remarks>
/// Example usage:
/// <code>
/// [Collection(nameof(DatabaseTestCollection))]
/// public class ProductControllerTests : BaseEntityControllerTests&lt;Product, ProductDto, int, DatabaseFixture&gt;
/// {
///     public ProductControllerTests(DatabaseFixture fixture) : base(fixture) { }
///     
///     protected override Product CreateValidEntity()
///     {
///         return new Product { Name = "Test Product", Price = 99.99m };
///     }
/// }
/// </code>
/// This provides 12 read + 13 write + 8 query tests = 33 comprehensive tests automatically!
/// </remarks>
public abstract class BaseEntityControllerTests<TEntity, TDto, TKey, TFixture> : BaseEntityChangeControllerTests<TEntity, TDto, TKey, TFixture>
    where TEntity : class, IEntity<TKey>, new()
    where TDto : class, IModel<TKey>, new()
    where TFixture : class, IRepositoryTestFixture
{
    /// <summary>
    /// Initializes a new instance of the BaseEntityControllerTests class.
    /// </summary>
    /// <param name="fixture">The database fixture</param>
    protected BaseEntityControllerTests(TFixture fixture) : base(fixture) { }

    /// <summary>
    /// Creates an instance of the full controller to be tested.
    /// Override this to provide custom EntityController implementation.
    /// </summary>
    protected virtual new EntityController<TEntity, TDto, TKey> CreateController()
    {
        var repository = CreateFullRepository();

        var logger = Fixture.ServiceProvider
            .GetRequiredService<ILogger<EntityController<TEntity, TDto, TKey>>>();

        var controller = new TestEntityController<TEntity, TDto, TKey>(repository, logger)
        {
            // Set up HttpContext for the controller
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    /// <summary>
    /// Creates an instance of the full repository (with Query support) to be tested.
    /// </summary>
    protected virtual IRepository<TEntity, TKey> CreateFullRepository()
    {
        var logger = Fixture.ServiceProvider
            .GetRequiredService<ILogger<Repository<TEntity, TKey>>>();

        return new Repository<TEntity, TKey>(Fixture.DbContext, logger);
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
    protected virtual Query<TEntity> CreateFilteredQuery(Expression<Func<TEntity, bool>> filter)
    {
        var query = new Query<TEntity>();
        query.Where(filter);
        return query;
    }

    #region GetAsync with Query Tests

    [Fact]
    public virtual async Task GetAsync_WithQuery_ReturnsOkWithMatchingEntity()
    {
        // Arrange
        var controller = CreateController();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        var query = CreateSimpleQuery();

        // Act
        var result = await controller.GetAsync(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedEntity = Assert.IsType<TEntity>(okResult.Value);
        Assert.NotNull(returnedEntity);
    }

    [Fact]
    public virtual async Task GetAsync_WithQueryNoMatch_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        await ClearDatabaseAsync();

        // Create query that filters by impossible condition
        var query = new Query<TEntity>();
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
        var result = await controller.GetAsync(query);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    #endregion

    #region GetAllAsync with Query Tests

    [Fact]
    public virtual async Task GetAllAsync_WithQuery_ReturnsOkWithMatchingEntities()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(5);
        await SeedDatabaseAsync([.. entities]);

        var query = CreateSimpleQuery();

        // Act
        var result = await controller.GetAllAsync(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedEntities = Assert.IsType<List<TEntity>>(okResult.Value, exactMatch: false);
        Assert.NotEmpty(returnedEntities);
        Assert.Equal(5, returnedEntities.Count);
    }

    [Fact]
    public virtual async Task GetAllAsync_WithQueryNoMatch_ReturnsOkWithEmptyList()
    {
        // Arrange
        var controller = CreateController();
        await ClearDatabaseAsync();

        var query = CreateSimpleQuery();

        // Act
        var result = await controller.GetAllAsync(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedEntities = Assert.IsType<List<TEntity>>(okResult.Value, exactMatch: false);
        Assert.Empty(returnedEntities);
    }

    #endregion

    #region GetCountAsync with Query Tests

    [Fact]
    public virtual async Task GetCountAsync_WithQuery_ReturnsOkWithCorrectCount()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(7);
        await SeedDatabaseAsync([.. entities]);

        var query = CreateSimpleQuery();

        // Act
        var result = await controller.GetCountAsync(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var count = Assert.IsType<long>(okResult.Value);
        Assert.Equal(7, count);
    }

    [Fact]
    public virtual async Task GetCountAsync_WithQueryNoMatch_ReturnsOkWithZero()
    {
        // Arrange
        var controller = CreateController();
        await ClearDatabaseAsync();

        var query = CreateSimpleQuery();

        // Act
        var result = await controller.GetCountAsync(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var count = Assert.IsType<long>(okResult.Value);
        Assert.Equal(0, count);
    }

    #endregion

    #region GetPagedListAsync with Query Tests

    [Fact]
    public virtual async Task GetPagedListAsync_WithQuery_ReturnsOkWithPaginatedResults()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(10);
        await SeedDatabaseAsync([.. entities]);

        var query = CreateSimpleQuery();
        query.SetPage(1, 5); // First page (1-based), 5 items

        // Act
        var result = await controller.GetPagedListAsync(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pageResponse = Assert.IsType<PageResponse<TEntity>>(okResult.Value);
        Assert.NotNull(pageResponse);
        Assert.Equal(10, pageResponse.TotalCount);
        Assert.True(pageResponse.Items.Count() <= 5);
    }

    [Fact]
    public virtual async Task GetPagedListAsync_WithQueryNoMatch_ReturnsOkWithEmptyPage()
    {
        // Arrange
        var controller = CreateController();
        await ClearDatabaseAsync();

        var query = CreateSimpleQuery();
        query.SetPage(1, 10); // First page (1-based)

        // Act
        var result = await controller.GetPagedListAsync(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pageResponse = Assert.IsType<PageResponse<TEntity>>(okResult.Value);
        Assert.NotNull(pageResponse);
        Assert.Equal(0, pageResponse.TotalCount);
        Assert.Empty(pageResponse.Items);
    }

    #endregion

    #region DeleteAsync with Query Tests

    [Fact]
    public virtual async Task DeleteAsync_WithQuery_ReturnsOk()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync([.. entities]);

        // Create query to delete specific entities
        var query = CreateSimpleQuery();

        // Act
        var result = await controller.DeleteAsync(query);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public virtual async Task DeleteAsync_WithQuery_EntitiesAreDeleted()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync([.. entities]);

        var query = CreateSimpleQuery();

        // Act
        await controller.DeleteAsync(query);

        // Assert - Verify deletion
        Fixture.DbContext.ChangeTracker.Clear();
        var repository = CreateFullRepository();
        var count = await repository.GetCountAsync();
        Assert.Equal(0, count);
    }

    #endregion
}

/// <summary>
/// Concrete test implementation of EntityController for testing purposes.
/// This allows us to instantiate the abstract EntityController class.
/// </summary>
internal class TestEntityController<TEntity, TDto, TKey> : EntityController<TEntity, TDto, TKey>
    where TEntity : class, IEntity<TKey>, new()
    where TDto : class, IModel<TKey>, new()
{
    public TestEntityController(IRepository<TEntity, TKey> repository, ILogger<EntityController<TEntity, TDto, TKey>> logger)
        : base(repository, logger) { }
}
