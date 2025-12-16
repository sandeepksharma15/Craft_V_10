using Craft.Cache;
using Craft.IntegrationTests.Entities;
using Craft.IntegrationTests.Fixtures;
using Craft.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.IntegrationTests.CrossModule;

/// <summary>
/// Integration tests for Cache + Repository cross-module functionality.
/// Tests caching patterns with repository operations.
/// </summary>
[Collection(nameof(DatabaseTestCollection))]
public class CacheRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IntegrationTestDbContext _dbContext = null!;
    private IChangeRepository<Product> _repository = null!;
    private ICacheService _cacheService = null!;

    public CacheRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
        _dbContext = _fixture.DbContext;

        var logger = _fixture.ServiceProvider.GetRequiredService<ILogger<ChangeRepository<Product, KeyType>>>();
        _repository = new ChangeRepository<Product>(_dbContext, logger);

        // Setup cache service
        var cacheOptions = Options.Create(new CacheOptions
        {
            DefaultExpiration = TimeSpan.FromMinutes(30),
            KeyPrefix = "integration-test",
            EnableStatistics = true
        });

        var loggerFactory = new LoggerFactory();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var memoryProvider = new MemoryCacheProvider(
            memoryCache,
            cacheOptions,
            loggerFactory.CreateLogger<MemoryCacheProvider>());

        var providerFactoryMock = new Mock<ICacheProviderFactory>();
        providerFactoryMock.Setup(f => f.GetDefaultProvider()).Returns(memoryProvider);
        providerFactoryMock.Setup(f => f.GetProvider("memory")).Returns(memoryProvider);

        _cacheService = new CacheService(
            providerFactoryMock.Object,
            loggerFactory.CreateLogger<CacheService>());
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region Cache-Aside Pattern Tests

    [Fact]
    public async Task CacheAside_GetById_CachesEntityOnFirstAccess()
    {
        // Arrange
        const KeyType productId = 1;
        var cacheKey = $"product:{productId}";

        // Verify cache is empty
        var cacheCheck = await _cacheService.GetAsync<Product>(cacheKey);
        Assert.False(cacheCheck.HasValue);

        // Act - Get from database and cache
        var product = await GetProductWithCacheAsync(productId);

        // Assert
        Assert.NotNull(product);
        Assert.Equal("Laptop", product.Name);

        // Verify cached
        var cached = await _cacheService.GetAsync<Product>(cacheKey);
        Assert.True(cached.HasValue);
        Assert.Equal(product.Id, cached.Value?.Id);
    }

    [Fact]
    public async Task CacheAside_GetById_ReturnsCachedValueOnSubsequentCalls()
    {
        // Arrange
        const KeyType productId = 2;
        var cacheKey = $"product:{productId}";

        // First call - populates cache
        var firstCall = await GetProductWithCacheAsync(productId);
        Assert.NotNull(firstCall);

        // Modify the cached value to prove second call returns cache
        var modifiedProduct = new Product
        {
            Id = productId,
            Name = "Modified (from cache)",
            Price = 999.99m,
            TenantId = 1
        };
        await _cacheService.SetAsync(cacheKey, modifiedProduct);

        // Act - Second call should return cached value
        var secondCall = await GetProductWithCacheAsync(productId);

        // Assert
        Assert.NotNull(secondCall);
        Assert.Equal("Modified (from cache)", secondCall.Name);
    }

    #endregion

    #region Cache Invalidation Tests

    [Fact]
    public async Task CacheInvalidation_OnUpdate_InvalidatesCache()
    {
        // Arrange
        const KeyType productId = 1;
        var cacheKey = $"product:{productId}";

        // Populate cache
        var product = await GetProductWithCacheAsync(productId);
        Assert.NotNull(product);

        // Verify cached
        var beforeUpdate = await _cacheService.GetAsync<Product>(cacheKey);
        Assert.True(beforeUpdate.HasValue);

        // Act - Update product and invalidate cache
        product.Name = "Updated Product Name";
        await _repository.UpdateAsync(product);
        await _cacheService.RemoveAsync(cacheKey);

        // Assert - Cache should be empty
        var afterUpdate = await _cacheService.GetAsync<Product>(cacheKey);
        Assert.False(afterUpdate.HasValue);

        // Fresh fetch should get updated value
        var freshFetch = await GetProductWithCacheAsync(productId);
        Assert.NotNull(freshFetch);
        Assert.Equal("Updated Product Name", freshFetch.Name);
    }

    [Fact]
    public async Task CacheInvalidation_OnDelete_InvalidatesCache()
    {
        // Arrange - Create and cache a product
        var newProduct = new Product
        {
            Name = "Cache Delete Test",
            Price = 99.99m,
            CategoryId = 1,
            TenantId = 1
        };
        var created = await _repository.AddAsync(newProduct);
        var cacheKey = $"product:{created.Id}";

        // Cache the product
        await _cacheService.SetAsync(cacheKey, created);

        // Verify cached
        var beforeDelete = await _cacheService.GetAsync<Product>(cacheKey);
        Assert.True(beforeDelete.HasValue);

        // Act - Delete and invalidate cache
        await _repository.DeleteAsync(created);
        await _cacheService.RemoveAsync(cacheKey);

        // Assert - Cache should be empty
        var afterDelete = await _cacheService.GetAsync<Product>(cacheKey);
        Assert.False(afterDelete.HasValue);
    }

    #endregion

    #region List Caching Tests

    [Fact]
    public async Task CacheList_GetAll_CachesEntireList()
    {
        // Arrange
        const string cacheKey = "products:all";

        // Act - Get all and cache
        var products = await GetAllProductsWithCacheAsync();

        // Assert
        Assert.NotEmpty(products);

        // Verify cached
        var cached = await _cacheService.GetAsync<List<Product>>(cacheKey);
        Assert.True(cached.HasValue);
        Assert.Equal(products.Count, cached.Value?.Count);
    }

    [Fact]
    public async Task CacheList_InvalidationOnCreate_InvalidatesListCache()
    {
        // Arrange - Cache the list
        const string listCacheKey = "products:all";
        var products = await GetAllProductsWithCacheAsync();
        var originalCount = products.Count;

        // Act - Create new product and invalidate list cache
        var newProduct = new Product
        {
            Name = "New Product for List Cache Test",
            Price = 75.00m,
            CategoryId = 1,
            TenantId = 1
        };
        await _repository.AddAsync(newProduct);
        await _cacheService.RemoveAsync(listCacheKey);

        // Assert - Cache should be invalidated
        var afterCreate = await _cacheService.GetAsync<List<Product>>(listCacheKey);
        Assert.False(afterCreate.HasValue);

        // Fresh fetch should include new product
        var freshList = await GetAllProductsWithCacheAsync();
        Assert.Equal(originalCount + 1, freshList.Count);
    }

    #endregion

    #region GetOrSet Pattern Tests

    [Fact]
    public async Task GetOrSet_WithFactory_LoadsFromDatabaseOnCacheMiss()
    {
        // Arrange
        const KeyType productId = 1;
        var cacheKey = $"product:getorset:{productId}";
        var factoryCallCount = 0;

        // Act
        var result = await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            factoryCallCount++;
            return await _repository.GetAsync(productId);
        });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, factoryCallCount);
        Assert.Equal("Laptop", result.Name);

        // Second call should use cache
        var secondResult = await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            factoryCallCount++;
            return await _repository.GetAsync(productId);
        });

        Assert.NotNull(secondResult);
        Assert.Equal(1, factoryCallCount); // Factory not called again
    }

    #endregion

    #region Complete Cache + Repository Loop Test

    [Fact]
    public async Task CompleteCacheRepositoryLoop_CreateCacheUpdateInvalidateDelete_WorksCorrectly()
    {
        // Step 1: Create product
        var product = new Product
        {
            Name = "Cache Loop Test Product",
            Price = 199.99m,
            CategoryId = 1,
            TenantId = 1
        };
        var created = await _repository.AddAsync(product);
        var cacheKey = $"product:{created.Id}";

        // Step 2: Cache the product
        await _cacheService.SetAsync(cacheKey, created);
        var cached = await _cacheService.GetAsync<Product>(cacheKey);
        Assert.True(cached.HasValue);
        Assert.Equal("Cache Loop Test Product", cached.Value?.Name);

        // Step 3: Update product and invalidate cache
        created.Name = "Updated Cache Loop Product";
        created.Price = 249.99m;
        await _repository.UpdateAsync(created);
        await _cacheService.RemoveAsync(cacheKey);

        // Verify cache invalidated
        var afterUpdate = await _cacheService.GetAsync<Product>(cacheKey);
        Assert.False(afterUpdate.HasValue);

        // Step 4: Re-cache updated product
        var freshProduct = await _repository.GetAsync(created.Id);
        Assert.NotNull(freshProduct);
        await _cacheService.SetAsync(cacheKey, freshProduct);

        var reCached = await _cacheService.GetAsync<Product>(cacheKey);
        Assert.True(reCached.HasValue);
        Assert.Equal("Updated Cache Loop Product", reCached.Value?.Name);

        // Step 5: Delete and final cache invalidation
        await _repository.DeleteAsync(freshProduct);
        await _cacheService.RemoveAsync(cacheKey);

        var afterDelete = await _cacheService.GetAsync<Product>(cacheKey);
        Assert.False(afterDelete.HasValue);

        // Verify product is soft-deleted
        var deletedProduct = await _repository.GetAsync(created.Id);
        Assert.Null(deletedProduct);
    }

    #endregion

    #region Helper Methods

    private async Task<Product?> GetProductWithCacheAsync(KeyType id)
    {
        var cacheKey = $"product:{id}";

        var cached = await _cacheService.GetAsync<Product>(cacheKey);
        if (cached.HasValue)
            return cached.Value;

        var product = await _repository.GetAsync(id);

        if (product is not null)
            await _cacheService.SetAsync(cacheKey, product);

        return product;
    }

    private async Task<List<Product>> GetAllProductsWithCacheAsync()
    {
        const string cacheKey = "products:all";

        var cached = await _cacheService.GetAsync<List<Product>>(cacheKey);
        if (cached.HasValue && cached.Value != null)
            return cached.Value;

        var products = await _repository.GetAllAsync();
        await _cacheService.SetAsync(cacheKey, products.ToList());

        return [.. products];
    }

    #endregion
}
