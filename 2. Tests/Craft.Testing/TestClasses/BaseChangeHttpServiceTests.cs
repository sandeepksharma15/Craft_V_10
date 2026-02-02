using Craft.Core.Common;
using Craft.Domain;
using Craft.HttpServices;
using Craft.HttpServices.Services;
using Craft.Testing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Testing.TestClasses;

/// <summary>
/// Base class for HTTP change service integration tests.
/// Inherits all read operation tests from BaseReadHttpServiceTests and adds write operation tests.
/// Provides standard test methods for Add, Update, and Delete operations via HTTP services.
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
/// public class BorderOrgHttpServiceTests : BaseChangeHttpServiceTests&lt;BorderOrg, BorderOrgVM, BorderOrgDTO, KeyType, IntegratedFixture&gt;
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
/// This provides 15 read tests + 13 write tests = 28 comprehensive tests automatically!
/// </remarks>
public abstract class BaseChangeHttpServiceTests<TEntity, TViewModel, TDto, TKey, TFixture> 
    : BaseReadHttpServiceTests<TEntity, TKey, TFixture>
    where TEntity : class, IEntity<TKey>, IModel<TKey>, new()
    where TViewModel : class, IModel<TKey>, new()
    where TDto : class, IModel<TKey>, new()
    where TFixture : class, IHttpServiceTestFixture
{
    /// <summary>
    /// Initializes a new instance of the BaseChangeHttpServiceTests class.
    /// </summary>
    /// <param name="fixture">The HTTP service test fixture</param>
    protected BaseChangeHttpServiceTests(TFixture fixture) : base(fixture) { }

    /// <summary>
    /// Creates an instance of the HTTP change service to be tested.
    /// Default implementation creates an HttpChangeService using the fixture's HttpClient.
    /// Override this method if you need custom service initialization.
    /// </summary>
    protected virtual IHttpChangeService<TEntity, TViewModel, TDto, TKey> CreateHttpChangeService()
    {
        var logger = Fixture.ServiceProvider
            .GetRequiredService<ILogger<HttpChangeService<TEntity, TViewModel, TDto, TKey>>>();

        var apiUrl = new Uri(Fixture.ApiBaseUri, GetApiEndpoint());

        return new HttpChangeService<TEntity, TViewModel, TDto, TKey>(apiUrl, Fixture.HttpClient, logger);
    }

    /// <summary>
    /// Creates a valid view model instance for testing.
    /// Override this method to customize view model creation with specific values or constraints.
    /// </summary>
    protected virtual TViewModel CreateValidViewModel()
    {
        var entity = CreateValidEntity();
        return MapEntityToViewModel(entity);
    }

    /// <summary>
    /// Creates multiple valid view model instances for testing.
    /// Override this method to customize batch view model creation.
    /// </summary>
    protected virtual List<TViewModel> CreateValidViewModels(int count)
    {
        var viewModels = new List<TViewModel>();

        for (int i = 0; i < count; i++)
            viewModels.Add(CreateValidViewModel());

        return viewModels;
    }

    /// <summary>
    /// Maps an entity to a view model. Default implementation uses reflection to copy properties.
    /// Override this if you need custom mapping logic.
    /// </summary>
    protected virtual TViewModel MapEntityToViewModel(TEntity entity)
    {
        var viewModel = new TViewModel();
        var entityType = typeof(TEntity);
        var viewModelType = typeof(TViewModel);

        // Copy all matching properties including ConcurrencyStamp for optimistic concurrency
        foreach (var prop in entityType.GetProperties())
        {
            var vmProp = viewModelType.GetProperty(prop.Name);
            if (vmProp != null && vmProp.CanWrite)
            {
                var value = prop.GetValue(entity);
                vmProp.SetValue(viewModel, value);
            }
        }

        return viewModel;
    }

    #region AddAsync Tests

    [Fact]
    public virtual async Task AddAsync_ValidViewModel_ReturnsSuccessWithEntity()
    {
        // Arrange
        var service = CreateHttpChangeService();
        var viewModel = CreateValidViewModel();

        // Act
        var result = await service.AddAsync(viewModel);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Id);
        Assert.Equal(201, result.StatusCode); // Created
        Assert.Null(result.Errors);

        // Verify entity was actually created in database
        Fixture.DbContext.ChangeTracker.Clear();
        var retrieved = Fixture.DbContext.Set<TEntity>().Find(result.Data.Id);
        Assert.NotNull(retrieved);
    }

    [Fact]
    public virtual async Task AddAsync_MultipleViewModels_AddsAllSuccessfully()
    {
        // Arrange
        var service = CreateHttpChangeService();
        var viewModels = CreateValidViewModels(3);

        // Act
        var results = new List<HttpServiceResult<TEntity?>>();
        foreach (var vm in viewModels)
        {
            var result = await service.AddAsync(vm);
            results.Add(result);
        }

        // Assert
        Assert.All(results, r =>
        {
            Assert.True(r.IsSuccess);
            Assert.NotNull(r.Data);
            Assert.Equal(201, r.StatusCode);
        });

        // Verify all entities were created
        Fixture.DbContext.ChangeTracker.Clear();
        var count = Fixture.DbContext.Set<TEntity>().Count();
        Assert.Equal(3, count);
    }

    [Fact]
    public virtual async Task AddAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var service = CreateHttpChangeService();
        var viewModel = CreateValidViewModel();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await service.AddAsync(viewModel, cancellationToken: cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public virtual async Task AddRangeAsync_MultipleViewModels_AddsAllSuccessfully()
    {
        // Arrange
        var service = CreateHttpChangeService();
        var viewModels = CreateValidViewModels(5);

        // Act
        var result = await service.AddRangeAsync(viewModels);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(5, result.Data.Count);
        Assert.Equal(200, result.StatusCode);

        // Verify all entities were created
        Fixture.DbContext.ChangeTracker.Clear();
        var count = Fixture.DbContext.Set<TEntity>().Count();
        Assert.Equal(5, count);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public virtual async Task UpdateAsync_ExistingEntity_ReturnsSuccessWithUpdatedEntity()
    {
        // Arrange
        var service = CreateHttpChangeService();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        var viewModel = MapEntityToViewModel(entity);
        ModifyViewModelForUpdate(viewModel);

        // Act
        var result = await service.UpdateAsync(viewModel);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.Errors);

        // Verify entity was actually updated in database
        Fixture.DbContext.ChangeTracker.Clear();
        var retrieved = Fixture.DbContext.Set<TEntity>().Find(entity.Id);
        Assert.NotNull(retrieved);
        VerifyEntityWasModified(entity, retrieved!);
    }

    [Fact]
    public virtual async Task UpdateAsync_NonExistingEntity_ReturnsNotFoundResult()
    {
        // Arrange
        var service = CreateHttpChangeService();
        await ClearDatabaseAsync();
        
        var viewModel = CreateValidViewModel();
        viewModel.Id = GetNonExistingId();

        // Act
        var result = await service.UpdateAsync(viewModel);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public virtual async Task UpdateAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var service = CreateHttpChangeService();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        var viewModel = MapEntityToViewModel(entity);
        ModifyViewModelForUpdate(viewModel);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await service.UpdateAsync(viewModel, cancellationToken: cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public virtual async Task UpdateRangeAsync_MultipleEntities_UpdatesAllSuccessfully()
    {
        // Arrange
        var service = CreateHttpChangeService();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync([.. entities]);

        var viewModels = entities.Select(MapEntityToViewModel).ToList();
        foreach (var viewModel in viewModels)
            ModifyViewModelForUpdate(viewModel);

        // Act
        var result = await service.UpdateRangeAsync(viewModels);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count);
        Assert.Equal(200, result.StatusCode);

        // Verify all entities were updated
        Fixture.DbContext.ChangeTracker.Clear();
        var allEntities = Fixture.DbContext.Set<TEntity>().ToList();
        for (int i = 0; i < entities.Count; i++)
            VerifyEntityWasModified(entities[i], allEntities.First(e => e.Id!.Equals(entities[i].Id)));
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public virtual async Task DeleteAsync_ExistingEntity_ReturnsSuccessResult()
    {
        // Arrange
        var service = CreateHttpChangeService();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);
        var entityId = entity.Id;

        // Act
        var result = await service.DeleteAsync(entityId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        Assert.Equal(200, result.StatusCode);

        // Verify entity was deleted (soft delete check)
        Fixture.DbContext.ChangeTracker.Clear();

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            // Soft delete - entity still exists but marked as deleted (must use IgnoreQueryFilters)
            var retrieved = Fixture.DbContext.Set<TEntity>()
                .IgnoreQueryFilters()
                .FirstOrDefault(e => e.Id!.Equals(entityId));
            Assert.NotNull(retrieved);
            Assert.True(((ISoftDelete)retrieved!).IsDeleted);
        }
        else
        {
            // Hard delete - entity should not exist
            var retrieved = Fixture.DbContext.Set<TEntity>().Find(entityId);
            Assert.Null(retrieved);
        }
    }

    [Fact]
    public virtual async Task DeleteAsync_NonExistingEntity_ReturnsNotFoundResult()
    {
        // Arrange
        var service = CreateHttpChangeService();
        await ClearDatabaseAsync();
        var nonExistingId = GetNonExistingId();

        // Act
        var result = await service.DeleteAsync(nonExistingId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public virtual async Task DeleteAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var service = CreateHttpChangeService();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await service.DeleteAsync(entity.Id, cancellationToken: cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public virtual async Task DeleteRangeAsync_MultipleEntities_DeletesAllSuccessfully()
    {
        // Arrange
        var service = CreateHttpChangeService();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync([.. entities]);

        var viewModels = entities.Select(MapEntityToViewModel).ToList();

        // Act
        var result = await service.DeleteRangeAsync(viewModels);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        Assert.Equal(200, result.StatusCode);

        // Verify all entities were deleted
        Fixture.DbContext.ChangeTracker.Clear();

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            // Soft delete - entities still exist but marked as deleted (must use IgnoreQueryFilters)
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

    #endregion

    #region Helper Methods

    /// <summary>
    /// Modifies a view model to prepare it for an update operation.
    /// Override this method to customize how view models are modified in update tests.
    /// Default implementation attempts to update the Name property if it exists,
    /// or the Description property as a fallback.
    /// </summary>
    /// <param name="viewModel">The view model to modify</param>
    protected virtual void ModifyViewModelForUpdate(TViewModel viewModel)
    {
        var viewModelType = typeof(TViewModel);

        // Try to update Name property
        var nameProperty = viewModelType.GetProperty("Name");
        if (nameProperty != null && nameProperty.CanWrite && nameProperty.PropertyType == typeof(string))
        {
            nameProperty.SetValue(viewModel, $"Updated {Guid.NewGuid().ToString()[..8]}");
            return;
        }

        // Try to update Description property
        var descProperty = viewModelType.GetProperty("Description");
        if (descProperty != null && descProperty.CanWrite && descProperty.PropertyType == typeof(string))
        {
            descProperty.SetValue(viewModel, $"Updated {Guid.NewGuid().ToString()[..8]}");
            return;
        }

        // If no suitable property found, derived classes must override this method
    }

    /// <summary>
    /// Verifies that an entity was modified during an update operation.
    /// Override this method to customize verification logic based on your entity properties.
    /// Default implementation compares Name or Description properties if they exist.
    /// </summary>
    /// <param name="originalEntity">The original entity before the update</param>
    /// <param name="updatedEntity">The entity retrieved from the database after the update</param>
    protected virtual void VerifyEntityWasModified(TEntity originalEntity, TEntity updatedEntity)
    {
        var entityType = typeof(TEntity);

        // Try to verify using Name property
        var nameProperty = entityType.GetProperty("Name");
        if (nameProperty != null && nameProperty.PropertyType == typeof(string))
        {
            var originalName = nameProperty.GetValue(originalEntity)?.ToString();
            var updatedName = nameProperty.GetValue(updatedEntity)?.ToString();
            Assert.NotEqual(originalName, updatedName);
            return;
        }

        // Try to verify using Description property
        var descProperty = entityType.GetProperty("Description");
        if (descProperty != null && descProperty.PropertyType == typeof(string))
        {
            var originalDesc = descProperty.GetValue(originalEntity)?.ToString();
            var updatedDesc = descProperty.GetValue(updatedEntity)?.ToString();
            Assert.NotEqual(originalDesc, updatedDesc);
            return;
        }

        // If no suitable property found, just verify the entity was retrieved successfully
        // Derived classes should override this method for proper verification
        Assert.NotNull(updatedEntity);
    }

    /// <summary>
    /// Gets the Name property value from a view model if it exists.
    /// Used for update tests to verify changes.
    /// </summary>
    [Obsolete("Use ModifyViewModelForUpdate instead for better flexibility.")]
    protected virtual string? GetViewModelName(TViewModel viewModel)
    {
        var nameProperty = typeof(TViewModel).GetProperty("Name");
        return nameProperty?.GetValue(viewModel)?.ToString();
    }

    /// <summary>
    /// Sets the Name property value on a view model if it exists.
    /// Used for update tests to modify view models.
    /// </summary>
    [Obsolete("Use ModifyViewModelForUpdate instead for better flexibility.")]
    protected virtual void SetViewModelName(TViewModel viewModel, string name)
    {
        var nameProperty = typeof(TViewModel).GetProperty("Name");

        if (nameProperty != null && nameProperty.CanWrite)
            nameProperty.SetValue(viewModel, name);
    }

    /// <summary>
    /// Gets the Name property value from an entity if it exists.
    /// Used for verification in tests.
    /// </summary>
    [Obsolete("Use VerifyEntityWasModified instead for better flexibility.")]
    protected virtual string? GetEntityName(TEntity entity)
    {
        var nameProperty = typeof(TEntity).GetProperty("Name");
        return nameProperty?.GetValue(entity)?.ToString();
    }

    #endregion
}

/// <summary>
/// Base class for HTTP change service integration tests with default KeyType.
/// Provides standard test methods for Add, Update, and Delete operations via HTTP services.
/// </summary>
/// <typeparam name="TEntity">The entity type being tested</typeparam>
/// <typeparam name="TViewModel">The view model type for the entity</typeparam>
/// <typeparam name="TDto">The data transfer object type for the entity</typeparam>
/// <typeparam name="TFixture">The HTTP service fixture type that implements IHttpServiceTestFixture</typeparam>
public abstract class BaseChangeHttpServiceTests<TEntity, TViewModel, TDto, TFixture> 
    : BaseChangeHttpServiceTests<TEntity, TViewModel, TDto, KeyType, TFixture>
    where TEntity : class, IEntity, IModel, new()
    where TViewModel : class, IModel, new()
    where TDto : class, IModel, new()
    where TFixture : class, IHttpServiceTestFixture
{
    protected BaseChangeHttpServiceTests(TFixture fixture) : base(fixture) { }
}
