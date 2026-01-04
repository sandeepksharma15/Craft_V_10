using Craft.Domain;
using Craft.Repositories;
using Craft.Testing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Testing.TestClasses;

/// <summary>
/// Base class for repository integration tests that include both read and write operations.
/// Inherits all read operation tests from BaseReadRepositoryTests and adds write operation tests.
/// Provides standard test methods for Add, Update, and Delete operations on repositories.
/// </summary>
/// <typeparam name="TEntity">The entity type being tested</typeparam>
/// <typeparam name="TKey">The primary key type of the entity</typeparam>
/// <typeparam name="TFixture">The database fixture type that implements IRepositoryTestFixture</typeparam>
/// <remarks>
/// Example usage:
/// <code>
/// [Collection(nameof(DatabaseTestCollection))]
/// public class ProductRepositoryTests : BaseChangeRepositoryTests&lt;Product, int, DatabaseFixture&gt;
/// {
///     public ProductRepositoryTests(DatabaseFixture fixture) : base(fixture) { }
///     
///     protected override Product CreateValidEntity()
///     {
///         return new Product { Name = "Test Product", Price = 99.99m };
///     }
/// }
/// </code>
/// This provides 15 read tests + 13 write tests = 28 comprehensive tests automatically!
/// </remarks>
public abstract class BaseChangeRepositoryTests<TEntity, TKey, TFixture> 
    : BaseReadRepositoryTests<TEntity, TKey, TFixture>
    where TEntity : class, IEntity<TKey>, new()
    where TFixture : class, IRepositoryTestFixture
{
    /// <summary>
    /// Initializes a new instance of the BaseChangeRepositoryTests class.
    /// </summary>
    /// <param name="fixture">The database fixture</param>
    protected BaseChangeRepositoryTests(TFixture fixture) : base(fixture)
    {
    }

    /// <summary>
    /// Creates an instance of the change repository to be tested.
    /// Override this to provide a custom repository implementation.
    /// Default implementation creates a ChangeRepository.
    /// </summary>
    protected override IReadRepository<TEntity, TKey> CreateRepository()
    {
        var logger = Fixture.ServiceProvider
            .GetRequiredService<ILogger<ChangeRepository<TEntity, TKey>>>();
        return new ChangeRepository<TEntity, TKey>(Fixture.DbContext, logger);
    }

    /// <summary>
    /// Helper method to get the repository as IChangeRepository.
    /// </summary>
    protected IChangeRepository<TEntity, TKey> GetChangeRepository() => (IChangeRepository<TEntity, TKey>)CreateRepository();

    #region AddAsync Tests

    [Fact]
    public virtual async Task AddAsync_ValidEntity_AddsToDatabase()
    {
        // Arrange
        var repository = GetChangeRepository();
        var entity = CreateValidEntity();

        // Act
        var addedEntity = await repository.AddAsync(entity, autoSave: true);

        // Assert
        Assert.NotNull(addedEntity);
        Assert.NotNull(addedEntity.Id);

        Fixture.DbContext.ChangeTracker.Clear();
        var retrieved = await repository.GetAsync(addedEntity.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(addedEntity.Id, retrieved.Id);
    }

    [Fact]
    public virtual async Task AddAsync_MultipleEntities_AddsAllToDatabase()
    {
        // Arrange
        var repository = GetChangeRepository();
        var entities = CreateValidEntities(3);

        // Act
        var addedEntities = new List<TEntity>();
        foreach (var entity in entities)
        {
            var added = await repository.AddAsync(entity, autoSave: false);
            addedEntities.Add(added);
        }
        await Fixture.DbContext.SaveChangesAsync();

        // Assert
        Fixture.DbContext.ChangeTracker.Clear();
        var count = await repository.GetCountAsync();
        Assert.Equal(3, count);
    }

    [Fact]
    public virtual async Task AddAsync_WithAutoSaveFalse_NotPersistedUntilSaveChanges()
    {
        // Arrange
        var repository = GetChangeRepository();
        var entity = CreateValidEntity();
        var countBefore = await repository.GetCountAsync();

        // Act
        _ = await repository.AddAsync(entity, autoSave: false);

        // Assert - entity count should be the same (not saved yet)
        var countAfterAdd = await repository.GetCountAsync();
        Assert.Equal(countBefore, countAfterAdd);

        // Now save changes
        await Fixture.DbContext.SaveChangesAsync();

        // Verify count increased
        Fixture.DbContext.ChangeTracker.Clear();
        var countAfterSave = await repository.GetCountAsync();
        Assert.Equal(countBefore + 1, countAfterSave);
    }

    [Fact]
    public virtual async Task AddRangeAsync_MultipleEntities_AddsAllToDatabase()
    {
        // Arrange
        var repository = GetChangeRepository();
        var entities = CreateValidEntities(5);

        // Act
        var addedEntities = await repository.AddRangeAsync(entities, autoSave: true);

        // Assert
        Assert.Equal(5, addedEntities.Count);
        Fixture.DbContext.ChangeTracker.Clear();
        var count = await repository.GetCountAsync();
        Assert.Equal(5, count);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public virtual async Task UpdateAsync_ExistingEntity_UpdatesInDatabase()
    {
        // Arrange
        var repository = GetChangeRepository();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        // Modify the entity
        SetEntityName(entity, "Updated Name");

        // Act
        var updatedEntity = await repository.UpdateAsync(entity, autoSave: true);

        // Assert
        Assert.NotNull(updatedEntity);
        Fixture.DbContext.ChangeTracker.Clear();
        var retrieved = await repository.GetAsync(entity.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Updated Name", GetEntityName(retrieved));
    }

    [Fact]
    public virtual async Task UpdateAsync_WithAutoSaveFalse_NotPersistedUntilSaveChanges()
    {
        // Arrange
        var repository = GetChangeRepository();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);
        var entityId = entity.Id;

        var originalName = GetEntityName(entity);
        SetEntityName(entity, "Updated Name");

        // Act
        await repository.UpdateAsync(entity, autoSave: false);

        // Assert - verify not persisted yet (query database directly)
        var retrievedBeforeSave = await Fixture.DbContext.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id!.Equals(entityId));
        Assert.NotNull(retrievedBeforeSave);
        Assert.Equal(originalName, GetEntityName(retrievedBeforeSave));

        // Now save and verify
        await Fixture.DbContext.SaveChangesAsync();
        Fixture.DbContext.ChangeTracker.Clear();
        var retrievedAfterSave = await repository.GetAsync(entityId);
        Assert.Equal("Updated Name", GetEntityName(retrievedAfterSave!));
    }

    [Fact]
    public virtual async Task UpdateRangeAsync_MultipleEntities_UpdatesAllInDatabase()
    {
        // Arrange
        var repository = GetChangeRepository();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync([.. entities]);

        // Modify all entities
        foreach (var entity in entities)
            SetEntityName(entity, $"Updated {GetEntityName(entity)}");

        // Act
        var updatedEntities = await repository.UpdateRangeAsync(entities, autoSave: true);

        // Assert
        Assert.Equal(3, updatedEntities.Count);
        Fixture.DbContext.ChangeTracker.Clear();
        var allEntities = await repository.GetAllAsync();
        Assert.All(allEntities, e => Assert.StartsWith("Updated", GetEntityName(e)));
    }

    #endregion

    #region DeleteAsync Tests (Soft Delete)

    [Fact]
    public virtual async Task DeleteAsync_ExistingEntity_SoftDeletesInDatabase()
    {
        // Arrange
        var repository = GetChangeRepository();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        // Act
        var deletedEntity = await repository.DeleteAsync(entity, autoSave: true);

        // Assert
        Assert.NotNull(deletedEntity);

        // Should not be found by default queries (soft deleted)
        Fixture.DbContext.ChangeTracker.Clear();
        var retrieved = await repository.GetAsync(entity.Id);
        Assert.Null(retrieved);

        // But should exist in database with IsDeleted = true (if ISoftDelete)
        if (entity is ISoftDelete)
        {
            var deletedFromDb = await Fixture.DbContext.Set<TEntity>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id!.Equals(entity.Id));

            Assert.NotNull(deletedFromDb);
            Assert.True(((ISoftDelete)deletedFromDb).IsDeleted);
        }
    }

    [Fact]
    public virtual async Task DeleteAsync_WithAutoSaveFalse_NotPersistedUntilSaveChanges()
    {
        // Arrange
        var repository = GetChangeRepository();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);
        var entityId = entity.Id;

        // Act
        await repository.DeleteAsync(entity, autoSave: false);

        // Assert - verify not deleted yet (query database directly)
        var retrievedBeforeSave = await Fixture.DbContext.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id!.Equals(entityId));
        Assert.NotNull(retrievedBeforeSave);

        // Now save and verify deletion
        await Fixture.DbContext.SaveChangesAsync();
        Fixture.DbContext.ChangeTracker.Clear();
        var retrievedAfterSave = await repository.GetAsync(entityId);
        Assert.Null(retrievedAfterSave);
    }

    [Fact]
    public virtual async Task DeleteRangeAsync_MultipleEntities_SoftDeletesAllInDatabase()
    {
        // Arrange
        var repository = GetChangeRepository();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync([.. entities]);

        // Act
        var deletedEntities = await repository.DeleteRangeAsync(entities, autoSave: true);

        // Assert
        Assert.Equal(3, deletedEntities.Count);
        Fixture.DbContext.ChangeTracker.Clear();
        var count = await repository.GetCountAsync();
        Assert.Equal(0, count); // All should be soft deleted and excluded
    }

    #endregion

    #region SaveChangesAsync Tests

    [Fact]
    public virtual async Task SaveChangesAsync_AfterMultipleAdds_ReturnsCorrectChangeCount()
    {
        // Arrange
        var repository = GetChangeRepository();
        var entities = CreateValidEntities(5);

        // Act
        foreach (var entity in entities)
            await repository.AddAsync(entity, autoSave: false);

        var changeCount = await Fixture.DbContext.SaveChangesAsync();

        // Assert
        Assert.Equal(5, changeCount);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the Name property value from an entity if it exists.
    /// Used for update tests to verify changes.
    /// </summary>
    protected virtual string? GetEntityName(TEntity entity)
    {
        var nameProperty = typeof(TEntity).GetProperty("Name");
        return nameProperty?.GetValue(entity)?.ToString();
    }

    /// <summary>
    /// Sets the Name property value on an entity if it exists.
    /// Used for update tests to modify entities.
    /// </summary>
    protected virtual void SetEntityName(TEntity entity, string name)
    {
        var nameProperty = typeof(TEntity).GetProperty("Name");

        if (nameProperty != null && nameProperty.CanWrite)
            nameProperty.SetValue(entity, name);
    }

    #endregion
}
