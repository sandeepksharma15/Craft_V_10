using Craft.Core;
using Craft.IntegrationTests.Entities;
using Craft.IntegrationTests.Fixtures;
using Craft.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for ChangeRepository with complete CRUD loop.
/// Tests Create ? Read ? Update ? Delete ? Verify operations.
/// </summary>
[Collection(nameof(DatabaseTestCollection))]
public class ChangeRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IChangeRepository<Product> _repository = null!;
    private IntegrationTestDbContext _dbContext = null!;

    public ChangeRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
        _dbContext = _fixture.DbContext;

        var logger = _fixture.ServiceProvider.GetRequiredService<ILogger<ChangeRepository<Product, KeyType>>>();
        _repository = new ChangeRepository<Product>(_dbContext, logger);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region Complete CRUD Loop Tests

    [Fact]
    public async Task FullCrudLoop_CreateReadUpdateDelete_WorksCorrectly()
    {
        // Arrange - Create
        var newProduct = new Product
        {
            Name = "Integration Test Product",
            Price = 199.99m,
            Description = "Created for CRUD test",
            CategoryId = 1,
            TenantId = 1
        };

        // Act - Create
        var createdProduct = await _repository.AddAsync(newProduct);

        // Assert - Create
        Assert.NotEqual(default, createdProduct.Id);
        Assert.Equal("Integration Test Product", createdProduct.Name);

        // Act - Read
        var retrievedProduct = await _repository.GetAsync(createdProduct.Id);

        // Assert - Read
        Assert.NotNull(retrievedProduct);
        Assert.Equal(createdProduct.Id, retrievedProduct.Id);
        Assert.Equal("Integration Test Product", retrievedProduct.Name);

        // Act - Update
        retrievedProduct.Name = "Updated Product Name";
        retrievedProduct.Price = 299.99m;
        var updatedProduct = await _repository.UpdateAsync(retrievedProduct);

        // Assert - Update
        Assert.Equal("Updated Product Name", updatedProduct.Name);
        Assert.Equal(299.99m, updatedProduct.Price);

        // Verify update persisted
        var verifyUpdate = await _repository.GetAsync(createdProduct.Id);
        Assert.NotNull(verifyUpdate);
        Assert.Equal("Updated Product Name", verifyUpdate.Name);

        // Act - Delete (Soft Delete since Product implements ISoftDelete via BaseEntity)
        await _repository.DeleteAsync(verifyUpdate);

        // Assert - Delete (verify soft delete)
        var afterDelete = await _dbContext.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == createdProduct.Id);

        Assert.NotNull(afterDelete);
        Assert.True(afterDelete.IsDeleted);

        // Verify entity is filtered out by default query
        var filteredResult = await _repository.GetAsync(createdProduct.Id);
        Assert.Null(filteredResult);
    }

    #endregion

    #region Add Tests

    [Fact]
    public async Task AddAsync_WithValidEntity_ReturnsEntityWithGeneratedId()
    {
        // Arrange
        var product = new Product
        {
            Name = "New Product",
            Price = 49.99m,
            CategoryId = 1,
            TenantId = 1
        };

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        Assert.NotEqual(default, result.Id);
        Assert.Equal("New Product", result.Name);
        Assert.Equal(49.99m, result.Price);
    }

    [Fact]
    public async Task AddAsync_WithAutoSaveFalse_DoesNotPersistUntilSaveChanges()
    {
        // Arrange
        var product = new Product
        {
            Name = "Delayed Save Product",
            Price = 99.99m,
            CategoryId = 1,
            TenantId = 1
        };

        // Act
        var result = await _repository.AddAsync(product, autoSave: false);

        // Verify not yet persisted (using a new context would show it's not saved)
        var count = await _repository.GetCountAsync();
        var initialCount = count;

        // Now save
        await _repository.SaveChangesAsync();

        // Assert - count should have increased
        var newCount = await _repository.GetCountAsync();
        Assert.Equal(initialCount + 1, newCount);
    }

    [Fact]
    public async Task AddRangeAsync_WithMultipleEntities_AddsAllEntities()
    {
        // Arrange
        var initialCount = await _repository.GetCountAsync();
        var products = new[]
        {
            new Product { Name = "Batch Product 1", Price = 10m, CategoryId = 1, TenantId = 1 },
            new Product { Name = "Batch Product 2", Price = 20m, CategoryId = 1, TenantId = 1 },
            new Product { Name = "Batch Product 3", Price = 30m, CategoryId = 1, TenantId = 1 }
        };

        // Act
        var result = await _repository.AddRangeAsync(products);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.NotEqual(default, p.Id));

        var newCount = await _repository.GetCountAsync();
        Assert.Equal(initialCount + 3, newCount);
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentNullException_WhenEntityIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null!));
    }

    #endregion

    #region Read Tests

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsEntity()
    {
        // Act
        var result = await _repository.GetAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Laptop", result.Name);
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetAsync(99999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllNonDeletedEntities()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, p => Assert.False(p.IsDeleted));
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.True(count > 0);
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsPaginatedResults()
    {
        // Arrange
        const int pageSize = 2;
        const int currentPage = 1;

        // Act
        var result = await _repository.GetPagedListAsync(currentPage, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Count() <= pageSize);
        Assert.Equal(currentPage, result.CurrentPage);
        Assert.Equal(pageSize, result.PageSize);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task GetPagedListAsync_ThrowsForInvalidPage()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _repository.GetPagedListAsync(0, 10));
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_WithValidEntity_UpdatesEntity()
    {
        // Arrange
        var product = await _repository.GetAsync(1);
        Assert.NotNull(product);
        var originalName = product.Name;
        product.Name = "Updated Name " + Guid.NewGuid();

        // Act
        var result = await _repository.UpdateAsync(product);

        // Assert
        Assert.NotEqual(originalName, result.Name);

        // Verify persistence
        var verifyProduct = await _repository.GetAsync(1);
        Assert.NotNull(verifyProduct);
        Assert.Equal(product.Name, verifyProduct.Name);
    }

    [Fact]
    public async Task UpdateRangeAsync_UpdatesMultipleEntities()
    {
        // Arrange
        var products = (await _repository.GetAllAsync()).Take(2).ToList();
        foreach (var product in products)
            product.Description = $"Updated at {DateTime.UtcNow}";

        // Act
        var result = await _repository.UpdateRangeAsync(products);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Contains("Updated at", p.Description));
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_SoftDeletesEntity()
    {
        // Arrange
        var product = new Product
        {
            Name = "To Be Deleted",
            Price = 5.99m,
            CategoryId = 1,
            TenantId = 1
        };
        var created = await _repository.AddAsync(product);

        // Act
        await _repository.DeleteAsync(created);

        // Assert - verify soft delete
        var softDeleted = await _dbContext.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == created.Id);

        Assert.NotNull(softDeleted);
        Assert.True(softDeleted.IsDeleted);

        // Verify not returned by normal query
        var normalQuery = await _repository.GetAsync(created.Id);
        Assert.Null(normalQuery);
    }

    [Fact]
    public async Task DeleteRangeAsync_SoftDeletesMultipleEntities()
    {
        // Arrange
        var products = new[]
        {
            new Product { Name = "Delete Me 1", Price = 1m, CategoryId = 1, TenantId = 1 },
            new Product { Name = "Delete Me 2", Price = 2m, CategoryId = 1, TenantId = 1 }
        };
        var created = await _repository.AddRangeAsync(products);
        var ids = created.Select(p => p.Id).ToList();

        // Act
        await _repository.DeleteRangeAsync(created);

        // Assert
        var softDeleted = await _dbContext.Products
            .IgnoreQueryFilters()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        Assert.Equal(2, softDeleted.Count);
        Assert.All(softDeleted, p => Assert.True(p.IsDeleted));
    }

    #endregion
}
