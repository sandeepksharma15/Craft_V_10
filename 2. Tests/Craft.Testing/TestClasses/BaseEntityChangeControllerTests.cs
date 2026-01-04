using AutoFixture;
using Craft.Controllers;
using Craft.Core;
using Craft.Domain;
using Craft.Repositories;
using Craft.Repositories.Services;
using Craft.Testing.Abstractions;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Testing.TestClasses;

/// <summary>
/// Base class for EntityChangeController integration tests.
/// Inherits all read operation tests from BaseEntityReadControllerTests and adds write operation tests.
/// Provides standard test methods for Create, Update, and Delete operations on controllers.
/// </summary>
/// <typeparam name="TEntity">The entity type being tested</typeparam>
/// <typeparam name="TDto">The DTO type for the entity</typeparam>
/// <typeparam name="TKey">The primary key type of the entity</typeparam>
/// <typeparam name="TFixture">The test fixture type that implements IRepositoryTestFixture</typeparam>
/// <remarks>
/// Example usage:
/// <code>
/// [Collection(nameof(DatabaseTestCollection))]
/// public class BorderOrgChangeControllerTests : BaseEntityChangeControllerTests&lt;BorderOrg, BorderOrgDto, KeyType, DatabaseFixture&gt;
/// {
///     public BorderOrgChangeControllerTests(DatabaseFixture fixture) : base(fixture) { }
///     
///     protected override BorderOrg CreateValidEntity()
///     {
///         return new BorderOrg { Name = "Test", Code = "TEST" };
///     }
/// }
/// </code>
/// This provides 12 read + 13 write tests = 25 comprehensive controller tests automatically!
/// </remarks>
public abstract class BaseEntityChangeControllerTests<TEntity, TDto, TKey, TFixture> 
    : BaseEntityReadControllerTests<TEntity, TDto, TKey, TFixture>
    where TEntity : class, IEntity<TKey>, new()
    where TDto : class, IModel<TKey>, new()
    where TFixture : class, IRepositoryTestFixture
{
    /// <summary>
    /// Initializes a new instance of the BaseEntityChangeControllerTests class.
    /// </summary>
    /// <param name="fixture">The test fixture</param>
    protected BaseEntityChangeControllerTests(TFixture fixture) : base(fixture)
    {
        // Configure Mapster for DTO <-> Entity mapping if not already configured
        ConfigureMapping();
    }

    /// <summary>
    /// Configures Mapster mapping between Entity and DTO.
    /// Override this to provide custom mapping configuration.
    /// </summary>
    protected virtual void ConfigureMapping()
    {
        // Default configuration - maps by convention
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
    }

    /// <summary>
    /// Creates an instance of the change controller to be tested.
    /// Override this if you need custom controller initialization.
    /// </summary>
    protected virtual new EntityChangeController<TEntity, TDto, TKey> CreateController()
    {
        var repository = CreateChangeRepository();
        var logger = Fixture.ServiceProvider
            .GetRequiredService<ILogger<EntityChangeController<TEntity, TDto, TKey>>>();
        
        var controller = new TestEntityChangeController<TEntity, TDto, TKey>(repository, logger);
        
        // Set up HttpContext for the controller
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    /// <summary>
    /// Creates an instance of the change repository to be tested.
    /// Override this if you need custom repository initialization.
    /// </summary>
    protected virtual IChangeRepository<TEntity, TKey> CreateChangeRepository()
    {
        var logger = Fixture.ServiceProvider
            .GetRequiredService<ILogger<ChangeRepository<TEntity, TKey>>>();
        return new ChangeRepository<TEntity, TKey>(Fixture.DbContext, logger);
    }

    /// <summary>
    /// Creates a valid DTO instance from an entity.
    /// Override this if you need custom DTO creation logic.
    /// </summary>
    protected virtual TDto CreateValidDto(TEntity entity)
    {
        return entity.Adapt<TDto>();
    }

    /// <summary>
    /// Creates a valid DTO instance for testing.
    /// Override this if you need custom DTO creation logic.
    /// </summary>
    protected virtual TDto CreateValidDto()
    {
        var entity = CreateValidEntity();
        return entity.Adapt<TDto>();
    }

    #region AddAsync Tests

    [Fact]
    public virtual async Task AddAsync_ValidDto_ReturnsCreatedWithEntity()
    {
        // Arrange
        var controller = CreateController();
        var dto = CreateValidDto();

        // Act
        var result = await controller.AddAsync(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        var returnedEntity = Assert.IsType<TEntity>(createdResult.Value);
        Assert.NotNull(returnedEntity);
        Assert.NotNull(returnedEntity.Id);
    }

    [Fact]
    public virtual async Task AddAsync_ValidDto_EntityIsPersisted()
    {
        // Arrange
        var controller = CreateController();
        var dto = CreateValidDto();

        // Act
        var result = await controller.AddAsync(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        var returnedEntity = Assert.IsType<TEntity>(createdResult.Value);
        
        Fixture.DbContext.ChangeTracker.Clear();
        var repository = CreateChangeRepository();
        var persistedEntity = await repository.GetAsync(returnedEntity.Id);
        Assert.NotNull(persistedEntity);
    }

    [Fact]
    public virtual async Task AddAsync_MultipleEntities_AllArePersisted()
    {
        // Arrange
        var controller = CreateController();
        
        // Act - Add 3 entities
        for (int i = 0; i < 3; i++)
        {
            var dto = CreateValidDto();
            var result = await controller.AddAsync(dto);
            Assert.IsType<CreatedResult>(result.Result);
        }

        // Assert
        Fixture.DbContext.ChangeTracker.Clear();
        var repository = CreateChangeRepository();
        var count = await repository.GetCountAsync();
        Assert.Equal(3, count);
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public virtual async Task AddRangeAsync_ValidDtos_ReturnsOk()
    {
        // Arrange
        var controller = CreateController();
        var dtos = new List<TDto>();
        for (int i = 0; i < 5; i++)
        {
            dtos.Add(CreateValidDto());
        }

        // Act
        var result = await controller.AddRangeAsync(dtos);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public virtual async Task AddRangeAsync_ValidDtos_AllEntitiesArePersisted()
    {
        // Arrange
        var controller = CreateController();
        var dtos = new List<TDto>();
        for (int i = 0; i < 5; i++)
        {
            dtos.Add(CreateValidDto());
        }

        // Act
        await controller.AddRangeAsync(dtos);

        // Assert
        Fixture.DbContext.ChangeTracker.Clear();
        var repository = CreateChangeRepository();
        var count = await repository.GetCountAsync();
        Assert.Equal(5, count);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public virtual async Task UpdateAsync_ExistingEntity_ReturnsOkWithUpdatedEntity()
    {
        // Arrange
        var controller = CreateController();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        // Clear tracker and reload entity to simulate real scenario
        Fixture.DbContext.ChangeTracker.Clear();
        var repository = CreateChangeRepository();
        var existingEntity = await repository.GetAsync(entity.Id);
        Assert.NotNull(existingEntity);

        var dto = CreateValidDto(existingEntity);
        // Modify DTO (assuming Name property exists)
        var nameProperty = typeof(TDto).GetProperty("Name");
        if (nameProperty != null && nameProperty.CanWrite)
        {
            nameProperty.SetValue(dto, "Updated Name");
        }

        // Act
        var result = await controller.UpdateAsync(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedEntity = Assert.IsType<TEntity>(okResult.Value);
        Assert.NotNull(returnedEntity);
    }

    [Fact]
    public virtual async Task UpdateAsync_ExistingEntity_ChangesArePersisted()
    {
        // Arrange
        var controller = CreateController();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        // Clear tracker and reload
        Fixture.DbContext.ChangeTracker.Clear();
        var repository = CreateChangeRepository();
        var existingEntity = await repository.GetAsync(entity.Id);
        Assert.NotNull(existingEntity);

        var dto = CreateValidDto(existingEntity);
        var nameProperty = typeof(TDto).GetProperty("Name");
        if (nameProperty != null && nameProperty.CanWrite)
        {
            nameProperty.SetValue(dto, "Updated Name");
        }

        // Act
        await controller.UpdateAsync(dto);

        // Assert
        Fixture.DbContext.ChangeTracker.Clear();
        var updatedEntity = await repository.GetAsync(existingEntity.Id);
        Assert.NotNull(updatedEntity);

        var entityNameProperty = typeof(TEntity).GetProperty("Name");
        if (entityNameProperty != null)
        {
            Assert.Equal("Updated Name", entityNameProperty.GetValue(updatedEntity));
        }
    }

    [Fact]
    public virtual async Task UpdateAsync_NonExistingEntity_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        var dto = CreateValidDto();

        // Set a non-existing ID
        if (typeof(TKey) == typeof(long) || typeof(TKey) == typeof(int))
        {
            dto.Id = (TKey)(object)999999L;
        }
        else if (typeof(TKey) == typeof(Guid))
        {
            dto.Id = (TKey)(object)Guid.NewGuid();
        }

        // Act
        var result = await controller.UpdateAsync(dto);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    #endregion

    #region UpdateRangeAsync Tests

    [Fact]
    public virtual async Task UpdateRangeAsync_ExistingEntities_ReturnsOk()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync(entities.ToArray());

        // Clear tracker and reload entities
        Fixture.DbContext.ChangeTracker.Clear();
        var repository = CreateChangeRepository();
        var existingEntities = await repository.GetAllAsync();

        var dtos = existingEntities.Select(e => CreateValidDto(e)).ToList();
        foreach (var dto in dtos)
        {
            var nameProperty = typeof(TDto).GetProperty("Name");
            if (nameProperty != null && nameProperty.CanWrite)
            {
                nameProperty.SetValue(dto, $"Updated {nameProperty.GetValue(dto)}");
            }
        }

        // Act
        var result = await controller.UpdateRangeAsync(dtos);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public virtual async Task UpdateRangeAsync_ExistingEntities_AllChangesArePersisted()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync(entities.ToArray());

        // Clear tracker and reload entities
        Fixture.DbContext.ChangeTracker.Clear();
        var repository = CreateChangeRepository();
        var existingEntities = await repository.GetAllAsync();

        var dtos = existingEntities.Select(e => CreateValidDto(e)).ToList();
        foreach (var dto in dtos)
        {
            var nameProperty = typeof(TDto).GetProperty("Name");
            if (nameProperty != null && nameProperty.CanWrite)
            {
                nameProperty.SetValue(dto, $"Updated {nameProperty.GetValue(dto)}");
            }
        }

        // Act
        await controller.UpdateRangeAsync(dtos);

        // Assert
        Fixture.DbContext.ChangeTracker.Clear();
        var allEntities = await repository.GetAllAsync();

        Assert.All(allEntities, e =>
        {
            var nameProperty = typeof(TEntity).GetProperty("Name");
            if (nameProperty != null)
            {
                var name = nameProperty.GetValue(e)?.ToString();
                Assert.StartsWith("Updated", name);
            }
        });
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public virtual async Task DeleteAsync_ExistingEntity_ReturnsOk()
    {
        // Arrange
        var controller = CreateController();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);

        // Act
        var result = await controller.DeleteAsync(entity.Id);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public virtual async Task DeleteAsync_ExistingEntity_EntityIsDeleted()
    {
        // Arrange
        var controller = CreateController();
        var entity = CreateValidEntity();
        await SeedDatabaseAsync(entity);
        var entityId = entity.Id;

        // Act
        await controller.DeleteAsync(entityId);

        // Assert
        Fixture.DbContext.ChangeTracker.Clear();
        var repository = CreateChangeRepository();
        var deletedEntity = await repository.GetAsync(entityId);
        Assert.Null(deletedEntity);
    }

    [Fact]
    public virtual async Task DeleteAsync_NonExistingEntity_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        var nonExistentId = default(TKey)!;

        if (typeof(TKey) == typeof(long) || typeof(TKey) == typeof(int))
        {
            nonExistentId = (TKey)(object)999999L;
        }
        else if (typeof(TKey) == typeof(Guid))
        {
            nonExistentId = (TKey)(object)Guid.NewGuid();
        }

        // Act
        var result = await controller.DeleteAsync(nonExistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region DeleteRangeAsync Tests

    [Fact]
    public virtual async Task DeleteRangeAsync_ExistingEntities_ReturnsOk()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync(entities.ToArray());

        // Clear tracker and reload entities to get proper DTOs
        Fixture.DbContext.ChangeTracker.Clear();
        var repository = CreateChangeRepository();
        var existingEntities = await repository.GetAllAsync();
        var dtos = existingEntities.Select(e => CreateValidDto(e)).ToList();

        // Act
        var result = await controller.DeleteRangeAsync(dtos);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public virtual async Task DeleteRangeAsync_ExistingEntities_AllEntitiesAreDeleted()
    {
        // Arrange
        var controller = CreateController();
        var entities = CreateValidEntities(3);
        await SeedDatabaseAsync(entities.ToArray());

        // Clear tracker and reload entities
        Fixture.DbContext.ChangeTracker.Clear();
        var repository = CreateChangeRepository();
        var existingEntities = await repository.GetAllAsync();
        var dtos = existingEntities.Select(e => CreateValidDto(e)).ToList();

        // Act
        await controller.DeleteRangeAsync(dtos);

        // Assert
        Fixture.DbContext.ChangeTracker.Clear();
        var count = await repository.GetCountAsync();
        Assert.Equal(0, count);
    }

    #endregion
}

/// <summary>
/// Concrete test implementation of EntityChangeController for testing purposes.
/// This allows us to instantiate the abstract EntityChangeController class.
/// </summary>
internal class TestEntityChangeController<TEntity, TDto, TKey> : EntityChangeController<TEntity, TDto, TKey>
    where TEntity : class, IEntity<TKey>, new()
    where TDto : class, IModel<TKey>, new()
{
    public TestEntityChangeController(IChangeRepository<TEntity, TKey> repository, ILogger<EntityChangeController<TEntity, TDto, TKey>> logger)
        : base(repository, logger)
    {
    }
}
