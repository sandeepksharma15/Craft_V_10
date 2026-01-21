using AutoFixture;
using Craft.Core.Common;
using Craft.Domain;
using Craft.HttpServices;
using Craft.HttpServices.Services;
using Craft.Testing.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Testing.TestClasses;

/// <summary>
/// Base class for HTTP read service integration tests.
/// Provides standard test methods for read operations via HTTP services.
/// Inherit from this class and provide a fixture that implements IHttpServiceTestFixture.
/// </summary>
/// <typeparam name="TEntity">The entity type being tested</typeparam>
/// <typeparam name="TKey">The primary key type of the entity</typeparam>
/// <typeparam name="TFixture">The HTTP service fixture type that implements IHttpServiceTestFixture</typeparam>
/// <remarks>
/// Example usage:
/// <code>
/// [Collection(nameof(HttpServiceTestCollection))]
/// public class BorderOrgHttpServiceTests : BaseReadHttpServiceTests&lt;BorderOrg, KeyType, IntegratedFixture&gt;
/// {
///     public BorderOrgHttpServiceTests(IntegratedFixture fixture) : base(fixture) { }
///     
///     protected override BorderOrg CreateValidEntity()
///     {
///         return new BorderOrg { Name = "Test Organization" };
///     }
///     
///     protected override string GetApiEndpoint() => "api/borderorg";
/// }
/// </code>
/// </remarks>
public abstract class BaseReadHttpServiceTests<TEntity, TKey, TFixture> : IAsyncLifetime 
    where TEntity : class, IEntity<TKey>, IModel<TKey>, new()
    where TFixture : class, IHttpServiceTestFixture
{
    /// <summary>
    /// The HTTP service test fixture providing DbContext, HttpClient, and service provider.
    /// </summary>
    protected readonly TFixture Fixture;

    /// <summary>
    /// Initializes a new instance of the BaseReadHttpServiceTests class.
    /// </summary>
    /// <param name="fixture">The HTTP service test fixture</param>
    protected BaseReadHttpServiceTests(TFixture fixture) => Fixture = fixture;

    /// <summary>
    /// Gets the API endpoint path for the entity (e.g., "api/borderorg").
    /// Must be overridden by derived classes to specify the correct API route.
    /// </summary>
    protected abstract string GetApiEndpoint();

    /// <summary>
    /// Creates an instance of the HTTP read service to be tested.
    /// Default implementation creates an HttpReadService using the fixture's HttpClient.
    /// Override this method if you need custom service initialization.
    /// </summary>
    protected virtual IHttpReadService<TEntity, TKey> CreateHttpReadService()
    {
        var logger = Fixture.ServiceProvider
            .GetRequiredService<ILogger<HttpReadService<TEntity, TKey>>>();

        var apiUrl = new Uri(Fixture.ApiBaseUri, GetApiEndpoint());

        return new HttpReadService<TEntity, TKey>(apiUrl, Fixture.HttpClient, logger);
    }

    /// <summary>
    /// Creates a valid entity instance for testing.
    /// Override this method to customize entity creation with specific values or constraints.
    /// </summary>
    protected virtual TEntity CreateValidEntity()
    {
        var fixture = new Fixture();

        // Configure AutoFixture to handle circular references
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        try
        {
            return fixture.Create<TEntity>();
        }
        catch (ObjectCreationException)
        {
            // If AutoFixture fails, create a basic instance
            // Derived classes should override this method to provide proper entity creation
            var entity = new TEntity();
            return entity;
        }
    }

    /// <summary>
    /// Creates multiple valid entity instances for testing.
    /// Override this method to customize batch entity creation.
    /// </summary>
    protected virtual List<TEntity> CreateValidEntities(int count)
    {
        var entities = new List<TEntity>();

        for (int i = 0; i < count; i++)
            entities.Add(CreateValidEntity());

        return entities;
    }

    /// <summary>
    /// Helper method to seed the database with test entities directly via DbContext.
    /// Default implementation adds entities to DbContext and saves changes.
    /// Override this method if you need custom seeding logic.
    /// </summary>
    protected virtual async Task SeedDatabaseAsync(params TEntity[] entities)
    {
        if (entities == null || entities.Length == 0)
            return;

        Fixture.DbContext.Set<TEntity>().AddRange(entities);
        await Fixture.DbContext.SaveChangesAsync();

        // Clear change tracker to avoid tracking issues
        Fixture.DbContext.ChangeTracker.Clear();
    }

    /// <summary>
    /// Helper method to clear the database before each test.
    /// Default implementation calls the fixture's ResetDatabaseAsync method.
    /// Override this method if you need custom cleanup logic.
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
    public virtual async Task GetAsync_ExistingEntity_ReturnsSuccessWithEntity()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        // Act
        var result = await service.GetAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(entity.Id, result.Data.Id);
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.Errors);
    }

    [Fact]
    public virtual async Task GetAsync_NonExistingEntity_ReturnsNotFoundResult()
    {
        // Arrange
        var service = CreateHttpReadService();
        await ClearDatabaseAsync();
        var nonExistingId = GetNonExistingId();

        // Act
        var result = await service.GetAsync(nonExistingId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetAsync_WithIncludeDetails_ReturnsSuccessWithEntityAndDetails()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        // Act
        var result = await service.GetAsync(entity.Id, includeDetails: true);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(entity.Id, result.Data.Id);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await service.GetAsync(entity.Id, cancellationToken: cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public virtual async Task GetAllAsync_EmptyDatabase_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var service = CreateHttpReadService();
        await ClearDatabaseAsync();

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetAllAsync_MultipleEntities_ReturnsSuccessWithAllEntities()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entities = CreateValidEntities(5);
        await SeedDatabaseAsync([.. entities]);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(entities.Count, result.Data.Count);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetAllAsync_WithIncludeDetails_ReturnsSuccessWithAllEntitiesAndDetails()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync([.. entities]);

        // Act
        var result = await service.GetAllAsync(includeDetails: true);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(entities.Count, result.Data.Count);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetAllAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entities = CreateValidEntities(2);
        await SeedDatabaseAsync([.. entities]);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await service.GetAllAsync(cancellationToken: cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    public virtual async Task GetCountAsync_EmptyDatabase_ReturnsSuccessWithZero()
    {
        // Arrange
        var service = CreateHttpReadService();
        await ClearDatabaseAsync();

        // Act
        var result = await service.GetCountAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetCountAsync_MultipleEntities_ReturnsSuccessWithCorrectCount()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entities = CreateValidEntities(7);
        await SeedDatabaseAsync([.. entities]);

        // Act
        var result = await service.GetCountAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(entities.Count, result.Data);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetCountAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync([.. entities]);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await service.GetCountAsync(cancellationToken: cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(entities.Count, result.Data);
    }

    #endregion

    #region GetPagedListAsync Tests

    [Fact]
    public virtual async Task GetPagedListAsync_FirstPage_ReturnsSuccessWithCorrectPage()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entities = CreateValidEntities(10);
        await SeedDatabaseAsync([.. entities]);

        // Act
        var result = await service.GetPagedListAsync(page: 1, pageSize: 5);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.CurrentPage);
        Assert.Equal(5, result.Data.PageSize);
        Assert.Equal(10, result.Data.TotalCount);
        Assert.Equal(2, result.Data.TotalPages);
        Assert.Equal(5, result.Data.Items.Count());
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetPagedListAsync_LastPage_ReturnsSuccessWithRemainingEntities()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entities = CreateValidEntities(8);
        await SeedDatabaseAsync([.. entities]);

        // Act
        var result = await service.GetPagedListAsync(page: 2, pageSize: 5);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.CurrentPage);
        Assert.Equal(5, result.Data.PageSize);
        Assert.Equal(8, result.Data.TotalCount);
        Assert.Equal(2, result.Data.TotalPages);
        Assert.Equal(3, result.Data.Items.Count()); // Only 3 items on last page
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetPagedListAsync_EmptyDatabase_ReturnsSuccessWithEmptyPage()
    {
        // Arrange
        var service = CreateHttpReadService();
        await ClearDatabaseAsync();

        // Act
        var result = await service.GetPagedListAsync(page: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.CurrentPage);
        Assert.Equal(10, result.Data.PageSize);
        Assert.Equal(0, result.Data.TotalCount);
        Assert.Equal(0, result.Data.TotalPages);
        Assert.Empty(result.Data.Items);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetPagedListAsync_WithIncludeDetails_ReturnsSuccessWithPageAndDetails()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entities = CreateValidEntities(15);
        await SeedDatabaseAsync([.. entities]);

        // Act
        var result = await service.GetPagedListAsync(page: 2, pageSize: 5, includeDetails: true);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.CurrentPage);
        Assert.Equal(5, result.Data.Items.Count());
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public virtual async Task GetPagedListAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var service = CreateHttpReadService();
        var entities = CreateValidEntities(10);
        await SeedDatabaseAsync([.. entities]);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await service.GetPagedListAsync(page: 1, pageSize: 5, cancellationToken: cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets a non-existing ID for testing scenarios where entity should not be found.
    /// Override this if you need specific non-existing ID logic for your TKey type.
    /// </summary>
    protected virtual TKey GetNonExistingId()
    {
        if (typeof(TKey) == typeof(int))
            return (TKey)(object)-999;
        if (typeof(TKey) == typeof(long))
            return (TKey)(object)-999L;
        if (typeof(TKey) == typeof(Guid))
            return (TKey)(object)Guid.NewGuid();
        if (typeof(TKey) == typeof(string))
            return (TKey)(object)"non-existing-id";

        throw new NotSupportedException($"GetNonExistingId not implemented for type {typeof(TKey).Name}. Please override this method in your test class.");
    }

    #endregion
}

/// <summary>
/// Base class for HTTP read service integration tests with default KeyType.
/// Provides standard test methods for read operations via HTTP services.
/// </summary>
/// <typeparam name="TEntity">The entity type being tested</typeparam>
/// <typeparam name="TFixture">The HTTP service fixture type that implements IHttpServiceTestFixture</typeparam>
public abstract class BaseReadHttpServiceTests<TEntity, TFixture> : BaseReadHttpServiceTests<TEntity, KeyType, TFixture>
    where TEntity : class, IEntity, IModel, new()
    where TFixture : class, IHttpServiceTestFixture
{
    protected BaseReadHttpServiceTests(TFixture fixture) : base(fixture) { }
}
