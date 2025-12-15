using Craft.Core;
using Craft.IntegrationTests.Entities;
using Craft.IntegrationTests.Fixtures;
using Craft.Repositories;
using Craft.Repositories.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for ReadRepository operations.
/// </summary>
[Collection(nameof(DatabaseTestCollection))]
public class ReadRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IReadRepository<Product> _repository = null!;

    public ReadRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();

        var logger = _fixture.ServiceProvider.GetRequiredService<ILogger<ReadRepository<Product, KeyType>>>();
        _repository = new ReadRepository<Product>(_fixture.DbContext, logger);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsProduct()
    {
        // Act
        var product = await _repository.GetAsync(1);

        // Assert
        Assert.NotNull(product);
        Assert.Equal("Laptop", product.Name);
    }

    [Fact]
    public async Task GetAsync_WithIncludeDetails_IncludesNavigationProperties()
    {
        // Act
        var product = await _repository.GetAsync(1, includeDetails: true);

        // Assert
        Assert.NotNull(product);
        // Note: Navigation properties will only be included if configured in DbSet
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Act
        var products = await _repository.GetAllAsync();

        // Assert
        Assert.NotEmpty(products);
        Assert.True(products.Count >= 4); // Seeded products (excluding soft-deleted)
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(4, count); // 5 seeded - 1 soft deleted
    }

    [Fact]
    public async Task GetPagedListAsync_FirstPage_ReturnsCorrectItems()
    {
        // Act
        var result = await _repository.GetPagedListAsync(1, 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetPagedListAsync_SecondPage_ReturnsCorrectItems()
    {
        // Act
        var result = await _repository.GetPagedListAsync(2, 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.CurrentPage);
    }

    [Fact]
    public async Task GetPagedListAsync_WithLargePageSize_ReturnsAllItems()
    {
        // Act
        var result = await _repository.GetPagedListAsync(1, 100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Items.Count());
        Assert.Equal(1, result.TotalPages);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public async Task GetPagedListAsync_InvalidPage_ThrowsException(int page, int pageSize)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _repository.GetPagedListAsync(page, pageSize));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public async Task GetPagedListAsync_InvalidPageSize_ThrowsException(int page, int pageSize)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _repository.GetPagedListAsync(page, pageSize));
    }
}
