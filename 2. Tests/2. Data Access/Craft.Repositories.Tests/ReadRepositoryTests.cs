using Craft.Repositories.Services;
using Craft.Testing.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Repositories.Tests;

[Collection(nameof(SystemTestCollectionDefinition))]
public class ReadRepositoryTests
{
    private static DbContextOptions<TestDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static ReadRepository<Country, KeyType> CreateRepository(TestDbContext context)
    {
        var logger = new Logger<ReadRepository<Country, KeyType>>(new LoggerFactory());
        return new ReadRepository<Country, KeyType>(context, logger);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities_WhenEntitiesExist()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == CountrySeed.COUNTRY_ID_1);
        Assert.Contains(result, c => c.Id == CountrySeed.COUNTRY_ID_2);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoEntitiesExist()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        context.Countries!.RemoveRange(context.Countries);
        context.SaveChanges();
        var repo = CreateRepository(context);

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_ReturnsEntity_WhenIdExists()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act
        var entity = await repo.GetAsync(CountrySeed.COUNTRY_ID_1);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(CountrySeed.COUNTRY_ID_1, entity.Id);
        Assert.Equal(CountrySeed.COUNTRY_NAME_1, entity.Name);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenIdDoesNotExist()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act
        var entity = await repo.GetAsync(9999);

        // Assert
        Assert.Null(entity);
    }

    [Fact]
    public async Task GetAsync_WithIncludeDetailsFalse_CompaniesIsNull()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act
        var entity = await repo.GetAsync(CountrySeed.COUNTRY_ID_1, includeDetails: false);

        // Assert
        Assert.NotNull(entity);
        Assert.Null(entity.Companies);
    }

    [Fact]
    public async Task GetAsync_WithIncludeDetailsTrue_CompaniesIsNotNull()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act
        var entity = await repo.GetAsync(CountrySeed.COUNTRY_ID_1, includeDetails: true);

        // Assert
        Assert.NotNull(entity);
        Assert.NotNull(entity.Companies);
        Assert.True(entity.Companies.Count >= 1);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act
        var count = await repo.GetCountAsync();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsZero_WhenNoEntitiesExist()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        context.Countries!.RemoveRange(context.Countries);
        context.SaveChanges();
        var repo = CreateRepository(context);

        // Act
        var count = await repo.GetCountAsync();

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsPagedEntities()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act
        var page = await repo.GetPagedListAsync(1, 1);

        // Assert
        Assert.NotNull(page);
        Assert.Equal(1, page.Items?.Count());
        Assert.Equal(2, page.TotalCount);
        Assert.Equal(1, page.CurrentPage);
        Assert.Equal(1, page.PageSize);
    }

    [Fact]
    public async Task GetPagedListAsync_ThrowsArgumentOutOfRangeException_WhenPageOrSizeInvalid()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repo.GetPagedListAsync(0, 1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repo.GetPagedListAsync(1, 0));
    }

    [Fact]
    public async Task GetAllAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => repo.GetAllAsync(cancellationToken: cts.Token));
    }

    [Fact]
    public async Task GetAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => repo.GetAsync(CountrySeed.COUNTRY_ID_1, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task GetCountAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => repo.GetCountAsync(cts.Token));
    }

    [Fact]
    public async Task GetPagedListAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => repo.GetPagedListAsync(1, 1, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenEntityExists()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act
        var exists = await repo.ExistsAsync(CountrySeed.COUNTRY_ID_1);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenEntityDoesNotExist()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act
        var exists = await repo.ExistsAsync(9999);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => repo.ExistsAsync(CountrySeed.COUNTRY_ID_1, cts.Token));
    }

    [Fact]
    public async Task AnyAsync_ReturnsTrue_WhenEntitiesExist()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act
        var any = await repo.AnyAsync();

        // Assert
        Assert.True(any);
    }

    [Fact]
    public async Task AnyAsync_ReturnsFalse_WhenNoEntitiesExist()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        context.Countries!.RemoveRange(context.Countries);
        await context.SaveChangesAsync();
        var repo = CreateRepository(context);

        // Act
        var any = await repo.AnyAsync();

        // Assert
        Assert.False(any);
    }

    [Fact]
    public async Task AnyAsync_RespectsCancellationToken()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => repo.AnyAsync(cts.Token));
    }
}

