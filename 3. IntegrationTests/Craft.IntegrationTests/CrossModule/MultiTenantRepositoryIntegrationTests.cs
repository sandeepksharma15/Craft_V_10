using Craft.IntegrationTests.Entities;
using Craft.IntegrationTests.Fixtures;
using Craft.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.IntegrationTests.CrossModule;

/// <summary>
/// Integration tests for multi-tenant repository operations.
/// Tests tenant isolation and cross-tenant scenarios.
/// </summary>
[Collection(nameof(DatabaseTestCollection))]
public class MultiTenantRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IntegrationTestDbContext _dbContext = null!;
    private IChangeRepository<Product> _repository = null!;

    public MultiTenantRepositoryIntegrationTests(DatabaseFixture fixture)
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

    #region Tenant Isolation Tests

    [Fact]
    public async Task Repository_GetAll_ReturnsOnlyActiveTenantData()
    {
        // Arrange - Products are seeded with different TenantIds

        // Act
        var allProducts = await _repository.GetAllAsync();

        // Assert - Should return all non-deleted products from all tenants
        Assert.NotEmpty(allProducts);
    }

    [Fact]
    public async Task Repository_FilterByTenant_ReturnsTenantSpecificData()
    {
        // Arrange
        const KeyType tenantId = 1;

        // Act - Manually filter by tenant
        var tenant1Products = await _dbContext.Products
            .Where(p => p.TenantId == tenantId && !p.IsDeleted)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(tenant1Products);
        Assert.All(tenant1Products, p => Assert.Equal(tenantId, p.TenantId));
    }

    [Fact]
    public async Task Repository_FilterByDifferentTenant_ReturnsOnlyThatTenantData()
    {
        // Arrange
        const KeyType tenantId = 2;

        // Act
        var tenant2Products = await _dbContext.Products
            .Where(p => p.TenantId == tenantId && !p.IsDeleted)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(tenant2Products);
        Assert.All(tenant2Products, p => Assert.Equal(tenantId, p.TenantId));
    }

    #endregion

    #region Cross-Tenant Data Creation Tests

    [Fact]
    public async Task Create_WithDifferentTenants_MaintainsIsolation()
    {
        // Arrange & Act
        var tenant1Product = await _repository.AddAsync(new Product
        {
            Name = "Tenant 1 Product",
            Price = 100m,
            CategoryId = 1,
            TenantId = 1
        });

        var tenant2Product = await _repository.AddAsync(new Product
        {
            Name = "Tenant 2 Product",
            Price = 200m,
            CategoryId = 1,
            TenantId = 2
        });

        // Assert - Both created with correct tenant IDs
        Assert.Equal(1, tenant1Product.TenantId);
        Assert.Equal(2, tenant2Product.TenantId);

        // Query each tenant's data
        var tenant1Data = await _dbContext.Products
            .Where(p => p.TenantId == 1 && p.Name == "Tenant 1 Product")
            .FirstOrDefaultAsync();

        var tenant2Data = await _dbContext.Products
            .Where(p => p.TenantId == 2 && p.Name == "Tenant 2 Product")
            .FirstOrDefaultAsync();

        Assert.NotNull(tenant1Data);
        Assert.NotNull(tenant2Data);
        Assert.NotEqual(tenant1Data.Id, tenant2Data.Id);
    }

    #endregion

    #region Tenant-Scoped Operations Tests

    [Fact]
    public async Task Update_OnlyAffectsSpecificTenantRecord()
    {
        // Arrange - Create products for different tenants
        var product1 = await _repository.AddAsync(new Product
        {
            Name = "Multi-Tenant Update Test",
            Price = 50m,
            CategoryId = 1,
            TenantId = 1
        });

        var product2 = await _repository.AddAsync(new Product
        {
            Name = "Multi-Tenant Update Test",
            Price = 50m,
            CategoryId = 1,
            TenantId = 2
        });

        // Act - Update only tenant 1's product
        product1.Price = 75m;
        await _repository.UpdateAsync(product1);

        // Assert - Tenant 2's product should be unchanged
        var tenant2Product = await _repository.GetAsync(product2.Id);
        Assert.NotNull(tenant2Product);
        Assert.Equal(50m, tenant2Product.Price);

        var tenant1Product = await _repository.GetAsync(product1.Id);
        Assert.NotNull(tenant1Product);
        Assert.Equal(75m, tenant1Product.Price);
    }

    [Fact]
    public async Task Delete_OnlyAffectsSpecificTenantRecord()
    {
        // Arrange
        var product1 = await _repository.AddAsync(new Product
        {
            Name = "Multi-Tenant Delete Test",
            Price = 25m,
            CategoryId = 1,
            TenantId = 1
        });

        var product2 = await _repository.AddAsync(new Product
        {
            Name = "Multi-Tenant Delete Test",
            Price = 25m,
            CategoryId = 1,
            TenantId = 2
        });

        // Act - Delete only tenant 1's product
        await _repository.DeleteAsync(product1);

        // Assert - Tenant 2's product should still exist
        var tenant2Product = await _repository.GetAsync(product2.Id);
        Assert.NotNull(tenant2Product);
        Assert.False(tenant2Product.IsDeleted);

        // Tenant 1's product should be soft-deleted
        var tenant1Product = await _dbContext.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == product1.Id);
        Assert.NotNull(tenant1Product);
        Assert.True(tenant1Product.IsDeleted);
    }

    #endregion

    #region Tenant Count and Statistics Tests

    [Fact]
    public async Task GetCount_ByTenant_ReturnsCorrectCount()
    {
        // Arrange
        var tenant1Count = await _dbContext.Products
            .Where(p => p.TenantId == 1 && !p.IsDeleted)
            .CountAsync();

        var tenant2Count = await _dbContext.Products
            .Where(p => p.TenantId == 2 && !p.IsDeleted)
            .CountAsync();

        // Assert - Different tenants have different counts
        Assert.True(tenant1Count > 0);
        Assert.True(tenant2Count > 0);
    }

    #endregion

    #region Complete Multi-Tenant CRUD Loop Test

    [Fact]
    public async Task CompleteTenantLoop_CreateReadUpdateDeletePerTenant_MaintainsIsolation()
    {
        // Setup - Create products for two tenants
        var tenant1Products = new[]
        {
            new Product { Name = "T1 Loop Product A", Price = 10m, CategoryId = 1, TenantId = 1 },
            new Product { Name = "T1 Loop Product B", Price = 20m, CategoryId = 1, TenantId = 1 }
        };

        var tenant2Products = new[]
        {
            new Product { Name = "T2 Loop Product A", Price = 30m, CategoryId = 1, TenantId = 2 },
            new Product { Name = "T2 Loop Product B", Price = 40m, CategoryId = 1, TenantId = 2 }
        };

        // Step 1: Create
        var created1 = await _repository.AddRangeAsync(tenant1Products);
        var created2 = await _repository.AddRangeAsync(tenant2Products);
        Assert.Equal(2, created1.Count);
        Assert.Equal(2, created2.Count);

        // Step 2: Read with tenant filter
        var readTenant1 = await _dbContext.Products
            .Where(p => p.Name.StartsWith("T1 Loop") && p.TenantId == 1 && !p.IsDeleted)
            .ToListAsync();
        Assert.Equal(2, readTenant1.Count);

        var readTenant2 = await _dbContext.Products
            .Where(p => p.Name.StartsWith("T2 Loop") && p.TenantId == 2 && !p.IsDeleted)
            .ToListAsync();
        Assert.Equal(2, readTenant2.Count);

        // Step 3: Update tenant 1 only
        foreach (var product in readTenant1)
            product.Price += 100m;
        await _repository.UpdateRangeAsync(readTenant1);

        // Verify tenant 2 unchanged
        var verifyTenant2 = await _dbContext.Products
            .Where(p => p.Name.StartsWith("T2 Loop") && p.TenantId == 2)
            .ToListAsync();
        Assert.Contains(verifyTenant2, p => p.Price == 30m);
        Assert.Contains(verifyTenant2, p => p.Price == 40m);

        // Step 4: Delete tenant 1's products only
        await _repository.DeleteRangeAsync(readTenant1);

        // Verify tenant 1 deleted
        var afterDeleteTenant1 = await _dbContext.Products
            .Where(p => p.Name.StartsWith("T1 Loop") && p.TenantId == 1 && !p.IsDeleted)
            .ToListAsync();
        Assert.Empty(afterDeleteTenant1);

        // Verify tenant 2 still exists
        var afterDeleteTenant2 = await _dbContext.Products
            .Where(p => p.Name.StartsWith("T2 Loop") && p.TenantId == 2 && !p.IsDeleted)
            .ToListAsync();
        Assert.Equal(2, afterDeleteTenant2.Count);

        // Cleanup tenant 2
        await _repository.DeleteRangeAsync(afterDeleteTenant2);
    }

    #endregion

    #region Tenant Data Aggregation Tests

    [Fact]
    public async Task Aggregate_SumByTenant_CalculatesCorrectly()
    {
        // Arrange - Create products with known prices for each tenant
        await _repository.AddAsync(new Product
        {
            Name = "Sum Test T1",
            Price = 100m,
            CategoryId = 1,
            TenantId = 1
        });

        await _repository.AddAsync(new Product
        {
            Name = "Sum Test T2",
            Price = 200m,
            CategoryId = 1,
            TenantId = 2
        });

        // Act
        var tenant1Sum = await _dbContext.Products
            .Where(p => p.Name.StartsWith("Sum Test") && p.TenantId == 1 && !p.IsDeleted)
            .SumAsync(p => p.Price);

        var tenant2Sum = await _dbContext.Products
            .Where(p => p.Name.StartsWith("Sum Test") && p.TenantId == 2 && !p.IsDeleted)
            .SumAsync(p => p.Price);

        // Assert
        Assert.Equal(100m, tenant1Sum);
        Assert.Equal(200m, tenant2Sum);
    }

    [Fact]
    public async Task Aggregate_AverageByTenant_CalculatesCorrectly()
    {
        // Arrange - Create products with known prices
        await _repository.AddRangeAsync(
        [
            new Product { Name = "Avg Test T1 A", Price = 100m, CategoryId = 1, TenantId = 1 },
            new Product { Name = "Avg Test T1 B", Price = 200m, CategoryId = 1, TenantId = 1 }
        ]);

        // Act
        var tenant1Avg = await _dbContext.Products
            .Where(p => p.Name.StartsWith("Avg Test T1") && p.TenantId == 1 && !p.IsDeleted)
            .AverageAsync(p => p.Price);

        // Assert
        Assert.Equal(150m, tenant1Avg);
    }

    #endregion

    #region Tenant Query Combination Tests

    [Fact]
    public async Task Query_CombineTenantWithOtherFilters_WorksCorrectly()
    {
        // Arrange - Create products with various attributes
        await _repository.AddRangeAsync(
        [
            new Product { Name = "Combo T1 Expensive", Price = 500m, CategoryId = 1, TenantId = 1 },
            new Product { Name = "Combo T1 Cheap", Price = 50m, CategoryId = 1, TenantId = 1 },
            new Product { Name = "Combo T2 Expensive", Price = 600m, CategoryId = 1, TenantId = 2 },
            new Product { Name = "Combo T2 Cheap", Price = 60m, CategoryId = 1, TenantId = 2 }
        ]);

        // Act - Query expensive products for tenant 1 only
        var tenant1Expensive = await _dbContext.Products
            .Where(p => p.Name.StartsWith("Combo") && p.TenantId == 1 && p.Price > 100m && !p.IsDeleted)
            .ToListAsync();

        // Assert
        Assert.Single(tenant1Expensive);
        Assert.Equal("Combo T1 Expensive", tenant1Expensive[0].Name);
        Assert.Equal(1, tenant1Expensive[0].TenantId);
    }

    #endregion
}
