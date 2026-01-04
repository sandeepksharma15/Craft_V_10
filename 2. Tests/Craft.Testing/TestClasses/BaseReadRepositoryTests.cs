using AutoFixture;
using Craft.Core;
using Craft.Domain;
using Craft.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Craft.Testing.TestClasses;

/// <summary>
/// Base class for read-only repository integration tests.
/// Provides standard test methods for read operations on repositories.
/// Inherit from this class and provide DbContext setup to test any entity repository.
/// </summary>
/// <typeparam name="TEntity">The entity type being tested</typeparam>
/// <typeparam name="TKey">The primary key type of the entity</typeparam>
/// <remarks>
/// Example usage:
/// <code>
/// public class ProductRepositoryTests : BaseReadRepositoryTests&lt;Product, int&gt;
/// {
///     protected override IReadRepository&lt;Product, int&gt; CreateRepository()
///     {
///         // Setup and return your repository with DbContext
///         return new ReadRepository&lt;Product, int&gt;(dbContext, logger);
///     }
///     
///     protected override Product CreateValidEntity()
///     {
///         // Customize entity creation if needed
///         return base.CreateValidEntity();
///     }
/// }
/// </code>
/// </remarks>
public abstract class BaseReadRepositoryTests<TEntity, TKey> : IAsyncLifetime 
    where TEntity : class, IEntity<TKey>, new()
{
    /// <summary>
    /// Creates an instance of the repository to be tested.
    /// Derived classes must implement this to provide the repository instance.
    /// </summary>
    protected abstract IReadRepository<TEntity, TKey> CreateRepository();

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
        {
            entities.Add(CreateValidEntity());  // Now uses the overridden method
        }
        return entities;
    }

    /// <summary>
    /// Helper method to seed the database with test entities.
    /// Derived classes should implement this to add entities to the DbContext.
    /// </summary>
    protected abstract Task SeedDatabaseAsync(params TEntity[] entities);

    /// <summary>
    /// Helper method to clear the database before each test.
    /// Derived classes should implement this to reset database state.
    /// </summary>
    protected abstract Task ClearDatabaseAsync();

    /// <summary>
    /// Called before each test - clears the database to ensure test isolation.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        await ClearDatabaseAsync();
    }

    /// <summary>
    /// Called after each test - clears the database to clean up.
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }

    #region GetAsync Tests

    [Fact]
    public virtual async Task GetAsync_ExistingEntity_ReturnsEntity()
    {
        // Arrange
        var repository = CreateRepository();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        // Act
        var result = await repository.GetAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public virtual async Task GetAsync_NonExistingEntity_ReturnsNull()
    {
        // Arrange
        var repository = CreateRepository();
        await ClearDatabaseAsync();
        var nonExistingId = GetNonExistingId();

        // Act
        var result = await repository.GetAsync(nonExistingId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public virtual async Task GetAsync_WithIncludeDetails_ReturnsEntityWithDetails()
    {
        // Arrange
        var repository = CreateRepository();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        // Act
        var result = await repository.GetAsync(entity.Id, includeDetails: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public virtual async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var repository = CreateRepository();
        await ClearDatabaseAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public virtual async Task GetAllAsync_MultipleEntities_ReturnsAllEntities()
    {
        // Arrange
        var repository = CreateRepository();
        var entities = CreateValidEntities(5);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entities.Count, result.Count);
    }

    [Fact]
    public virtual async Task GetAllAsync_WithIncludeDetails_ReturnsAllEntitiesWithDetails()
    {
        // Arrange
        var repository = CreateRepository();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await repository.GetAllAsync(includeDetails: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entities.Count, result.Count);
    }

    [Fact]
    public virtual async Task GetAllAsync_WithSoftDeletedEntities_ExcludesDeletedByDefault()
    {
        // Arrange
        var repository = CreateRepository();
        var entities = CreateValidEntities(3);
        
        // Mark one entity as deleted if it implements ISoftDelete
        if (entities[0] is ISoftDelete softDeleteEntity)
        {
            softDeleteEntity.IsDeleted = true;
        }
        
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        
        // If entity supports soft delete, should exclude the deleted one
        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            Assert.Equal(entities.Count - 1, result.Count);
        }
        else
        {
            Assert.Equal(entities.Count, result.Count);
        }
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    public virtual async Task GetCountAsync_EmptyDatabase_ReturnsZero()
    {
        // Arrange
        var repository = CreateRepository();
        await ClearDatabaseAsync();

        // Act
        var count = await repository.GetCountAsync();

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public virtual async Task GetCountAsync_MultipleEntities_ReturnsCorrectCount()
    {
        // Arrange
        var repository = CreateRepository();
        var entities = CreateValidEntities(7);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var count = await repository.GetCountAsync();

        // Assert
        Assert.Equal(entities.Count, count);
    }

    [Fact]
    public virtual async Task GetCountAsync_WithSoftDeletedEntities_ExcludesDeletedByDefault()
    {
        // Arrange
        var repository = CreateRepository();
        var entities = CreateValidEntities(5);
        
        // Mark two entities as deleted if they implement ISoftDelete
        if (entities[0] is ISoftDelete softDelete1 && entities[1] is ISoftDelete softDelete2)
        {
            softDelete1.IsDeleted = true;
            softDelete2.IsDeleted = true;
        }
        
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var count = await repository.GetCountAsync();

        // Assert
        // If entity supports soft delete, should exclude the deleted ones
        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            Assert.Equal(entities.Count - 2, count);
        }
        else
        {
            Assert.Equal(entities.Count, count);
        }
    }

    #endregion

    #region GetPagedListAsync Tests

    [Fact]
    public virtual async Task GetPagedListAsync_FirstPage_ReturnsCorrectEntities()
    {
        // Arrange
        var repository = CreateRepository();
        var entities = CreateValidEntities(10);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await repository.GetPagedListAsync(currentPage: 1, pageSize: 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(5, result.Items.Count());
    }

    [Fact]
    public virtual async Task GetPagedListAsync_LastPage_ReturnsRemainingEntities()
    {
        // Arrange
        var repository = CreateRepository();
        var entities = CreateValidEntities(8);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await repository.GetPagedListAsync(currentPage: 2, pageSize: 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(8, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(3, result.Items.Count()); // Only 3 items on last page
    }

    [Fact]
    public virtual async Task GetPagedListAsync_EmptyDatabase_ReturnsEmptyPage()
    {
        // Arrange
        var repository = CreateRepository();
        await ClearDatabaseAsync();

        // Act
        var result = await repository.GetPagedListAsync(currentPage: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Items);
    }

    [Fact]
    public virtual async Task GetPagedListAsync_WithIncludeDetails_ReturnsPageWithDetails()
    {
        // Arrange
        var repository = CreateRepository();
        var entities = CreateValidEntities(15);
        await SeedDatabaseAsync(entities.ToArray());

        // Act
        var result = await repository.GetPagedListAsync(currentPage: 2, pageSize: 5, includeDetails: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(5, result.Items.Count());
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
