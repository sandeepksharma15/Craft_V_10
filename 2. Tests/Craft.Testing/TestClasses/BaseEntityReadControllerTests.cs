using Craft.Controllers;
using Craft.Core;
using Craft.Domain;
using Craft.Repositories;
using Craft.Repositories.Services;
using Craft.Testing.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Testing.TestClasses;

/// <summary>
/// Base class for EntityReadController integration tests.
/// Provides standard test methods for read operations on controllers.
/// Inherit from this class to test any EntityReadController implementation.
/// </summary>
/// <typeparam name="TEntity">The entity type being tested</typeparam>
/// <typeparam name="TDto">The DTO type for the entity</typeparam>
/// <typeparam name="TKey">The primary key type of the entity</typeparam>
/// <typeparam name="TFixture">The test fixture type that implements IRepositoryTestFixture</typeparam>
/// <remarks>
/// Example usage:
/// <code>
/// [Collection(nameof(DatabaseTestCollection))]
/// public class BorderOrgControllerTests : BaseEntityReadControllerTests&lt;BorderOrg, BorderOrgDto, KeyType, DatabaseFixture&gt;
/// {
///     public BorderOrgControllerTests(DatabaseFixture fixture) : base(fixture) { }
///     
///     protected override BorderOrg CreateValidEntity()
///     {
///         return new BorderOrg { Name = "Test", Code = "TEST" };
///     }
/// }
/// </code>
/// This provides 12 comprehensive controller tests automatically!
/// </remarks>
public abstract class BaseEntityReadControllerTests<TEntity, TDto, TKey, TFixture> : IAsyncLifetime
    where TEntity : class, IEntity<TKey>, new()
    where TDto : class, IModel<TKey>, new()
    where TFixture : class, IRepositoryTestFixture
{
    /// <summary>
    /// The test fixture providing DbContext and service provider.
    /// </summary>
    protected readonly TFixture Fixture;

    /// <summary>
    /// Initializes a new instance of the BaseEntityReadControllerTests class.
    /// </summary>
    /// <param name="fixture">The test fixture</param>
    protected BaseEntityReadControllerTests(TFixture fixture) => Fixture = fixture;

    /// <summary>
    /// Creates an instance of the controller to be tested.
    /// Default implementation creates an EntityReadController.
    /// Override this if you need custom controller initialization.
    /// </summary>
    protected virtual EntityReadController<TEntity, TDto, TKey> CreateController()
    {
        var repository = CreateRepository();
        var logger = Fixture.ServiceProvider
            .GetRequiredService<ILogger<EntityReadController<TEntity, TDto, TKey>>>();

        var controller = new EntityReadController<TEntity, TDto, TKey>(repository, logger)
        {
            // Set up HttpContext for the controller
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        return controller;
    }

    /// <summary>
    /// Creates an instance of the repository to be tested.
    /// Default implementation creates a ReadRepository.
    /// Override this if you need custom repository initialization.
    /// </summary>
    protected virtual IReadRepository<TEntity, TKey> CreateRepository()
    {
        var logger = Fixture.ServiceProvider
            .GetRequiredService<ILogger<ReadRepository<TEntity, TKey>>>();

        return new ReadRepository<TEntity, TKey>(Fixture.DbContext, logger);
    }

    /// <summary>
    /// Creates a valid entity instance for testing.
    /// Derived classes must implement this to provide entity-specific creation logic.
    /// </summary>
    protected abstract TEntity CreateValidEntity();

    /// <summary>
    /// Creates multiple valid entity instances for testing.
    /// Default implementation uses CreateValidEntity() multiple times.
    /// </summary>
    protected virtual List<TEntity> CreateValidEntities(int count)
    {
        var entities = new List<TEntity>();

        for (int i = 0; i < count; i++)
            entities.Add(CreateValidEntity());

        return entities;
    }

    /// <summary>
    /// Helper method to seed the database with test entities.
    /// Default implementation adds entities to DbContext and saves changes.
    /// </summary>
    protected virtual async Task SeedDatabaseAsync(params TEntity[] entities)
    {
        if (entities == null || entities.Length == 0)
            return;

        Fixture.DbContext.Set<TEntity>().AddRange(entities);
        await Fixture.DbContext.SaveChangesAsync();
        Fixture.DbContext.ChangeTracker.Clear();
    }

    /// <summary>
    /// Helper method to clear the database before each test.
    /// Default implementation calls the fixture's ResetDatabaseAsync method.
    /// </summary>
    protected virtual async Task ClearDatabaseAsync() => await Fixture.ResetDatabaseAsync();

    /// <summary>
    /// Called before each test - clears the database to ensure test isolation.
    /// </summary>
    public virtual async Task InitializeAsync() => await ClearDatabaseAsync();

    /// <summary>
    /// Called after each test - clears the database to clean up.
    /// </summary>
    public virtual async Task DisposeAsync() => await ClearDatabaseAsync();

    #region GetAsync Tests

    [Fact]
    public virtual async Task GetAsync_ExistingEntity_ReturnsOkWithEntity()
    {
        // Arrange
        var controller = CreateController();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        // Act
        var result = await controller.GetAsync(entity.Id, includeDetails: false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedEntity = Assert.IsType<TEntity>(okResult.Value);
        Assert.NotNull(returnedEntity);
        Assert.Equal(entity.Id, returnedEntity.Id);
    }

    [Fact]
    public virtual async Task GetAsync_NonExistingEntity_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        var nonExistentId = default(TKey)!;

        // For numeric types, use a large number that won't exist
        if (typeof(TKey) == typeof(long) || typeof(TKey) == typeof(int))
            nonExistentId = (TKey)(object)999999L;
        else if (typeof(TKey) == typeof(Guid))
            nonExistentId = (TKey)(object)Guid.NewGuid();

        // Act
        var result = await controller.GetAsync(nonExistentId, includeDetails: false);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public virtual async Task GetAsync_WithIncludeDetails_ReturnsOkWithEntity()
    {
        // Arrange
        var controller = CreateController();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        // Act
        var result = await controller.GetAsync(entity.Id, includeDetails: true);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedEntity = Assert.IsType<TEntity>(okResult.Value);
        Assert.NotNull(returnedEntity);
        Assert.Equal(entity.Id, returnedEntity.Id);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public virtual async Task GetAllAsync_EmptyDatabase_ReturnsOkWithEmptyList()
    {
        // Arrange
        var controller = CreateController();
        await ClearDatabaseAsync();

        // Act
        var result = await controller.GetAllAsync(includeDetails: false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var entities = Assert.IsAssignableFrom<IEnumerable<TEntity>>(okResult.Value);
        Assert.Empty(entities);
    }

    [Fact]
    public virtual async Task GetAllAsync_MultipleEntities_ReturnsOkWithAllEntities()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(5);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await controller.GetAllAsync(includeDetails: false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedEntities = Assert.IsAssignableFrom<IEnumerable<TEntity>>(okResult.Value);
        Assert.Equal(5, returnedEntities.Count());
    }

    [Fact]
    public virtual async Task GetAllAsync_WithIncludeDetails_ReturnsOkWithAllEntities()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await controller.GetAllAsync(includeDetails: true);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedEntities = Assert.IsAssignableFrom<IEnumerable<TEntity>>(okResult.Value);
        Assert.Equal(3, returnedEntities.Count());
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    public virtual async Task GetCountAsync_EmptyDatabase_ReturnsOkWithZero()
    {
        // Arrange
        var controller = CreateController();
        await ClearDatabaseAsync();

        // Act
        var result = await controller.GetCountAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var count = Assert.IsType<long>(okResult.Value);
        Assert.Equal(0, count);
    }

    [Fact]
    public virtual async Task GetCountAsync_MultipleEntities_ReturnsOkWithCorrectCount()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(7);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await controller.GetCountAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var count = Assert.IsType<long>(okResult.Value);
        Assert.Equal(7, count);
    }

    #endregion

    #region GetPagedListAsync Tests

    [Fact]
    public virtual async Task GetPagedListAsync_FirstPage_ReturnsOkWithCorrectEntities()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(10);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await controller.GetPagedListAsync(page: 1, pageSize: 5, includeDetails: false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pageResponse = Assert.IsType<PageResponse<TEntity>>(okResult.Value);
        Assert.Equal(1, pageResponse.CurrentPage);
        Assert.Equal(5, pageResponse.PageSize);
        Assert.Equal(10, pageResponse.TotalCount);
        Assert.Equal(5, pageResponse.Items.Count());
    }

    [Fact]
    public virtual async Task GetPagedListAsync_LastPage_ReturnsOkWithRemainingEntities()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(12);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await controller.GetPagedListAsync(page: 3, pageSize: 5, includeDetails: false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pageResponse = Assert.IsType<PageResponse<TEntity>>(okResult.Value);
        Assert.Equal(3, pageResponse.CurrentPage);
        Assert.Equal(5, pageResponse.PageSize);
        Assert.Equal(12, pageResponse.TotalCount);
        Assert.Equal(2, pageResponse.Items.Count()); // Only 2 items on last page
    }

    [Fact]
    public virtual async Task GetPagedListAsync_EmptyDatabase_ReturnsOkWithEmptyPage()
    {
        // Arrange
        var controller = CreateController();
        await ClearDatabaseAsync();

        // Act
        var result = await controller.GetPagedListAsync(page: 1, pageSize: 10, includeDetails: false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pageResponse = Assert.IsType<PageResponse<TEntity>>(okResult.Value);
        Assert.Equal(1, pageResponse.CurrentPage);
        Assert.Equal(10, pageResponse.PageSize);
        Assert.Equal(0, pageResponse.TotalCount);
        Assert.Empty(pageResponse.Items);
    }

    [Fact]
    public virtual async Task GetPagedListAsync_WithIncludeDetails_ReturnsOkWithPageData()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(8);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await controller.GetPagedListAsync(page: 1, pageSize: 5, includeDetails: true);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pageResponse = Assert.IsType<PageResponse<TEntity>>(okResult.Value);
        Assert.Equal(1, pageResponse.CurrentPage);
        Assert.Equal(5, pageResponse.PageSize);
        Assert.Equal(8, pageResponse.TotalCount);
        Assert.Equal(5, pageResponse.Items.Count());
    }

    #endregion
}
