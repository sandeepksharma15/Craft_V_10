using Craft.Repositories;
using Craft.TestDataStore.Fixtures;
using Craft.TestDataStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.QuerySpec.Tests.Services;

public class RepositoryTests
{
    private static DbContextOptions<TestDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static Repository<Country, KeyType> CreateRepository(TestDbContext context)
    {
        var logger = new Logger<Repository<Country, KeyType>>(new LoggerFactory());
        return new Repository<Country, KeyType>(context, logger);
    }

    [Fact]
    public async Task DeleteAsync_DeletesEntities_WhenNotSoftDelete()
    {
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesEntities_WhenImplementsISoftDelete()
    {
        // Arrange
        IQuery<Country> countryQuery = new Query<Country>();
        var options = CreateOptions();
        await using var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);
        var countriesToAdd = new List<Country>
            {
                new() { Name = "Country A", IsDeleted = false },
                new() { Name = "Country B", IsDeleted = false }
            };
        context.Countries?.AddRange(countriesToAdd);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        countryQuery.Where(c => c.Name!.Contains('A'));
        await repo.DeleteAsync(countryQuery);

        // Assert
        Assert.Equal(1, await context.Countries!.CountAsync(c => c.IsDeleted));
        Assert.Equal(1, await context.Countries!.CountAsync(c => c.Name == "Country B"));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
    }

    [Fact]
    public async Task DeleteAsync_RespectsCancellationToken()
    {
    }

    [Fact]
    public async Task DeleteAsync_DoesNotSave_WhenAutoSaveIsFalse()
    {
    }

    [Fact]
    public async Task GetAsync_ReturnsEntity_WhenMatchExists()
    {
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenNoMatch()
    {
    }

    [Fact]
    public async Task GetAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
    }

    [Fact]
    public async Task GetAsync_RespectsCancellationToken()
    {
    }

    [Fact]
    public async Task GetAsyncTResult_ReturnsProjection_WhenMatchExists()
    {
    }

    [Fact]
    public async Task GetAsyncTResult_ReturnsNull_WhenNoMatch()
    {
    }

    [Fact]
    public async Task GetAsyncTResult_ThrowsArgumentNullException_WhenQueryIsNull()
    {
    }

    [Fact]
    public async Task GetAsyncTResult_RespectsCancellationToken()
    {
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
    }

    [Fact]
    public async Task GetCountAsync_ReturnsZero_WhenNoMatch()
    {
    }

    [Fact]
    public async Task GetCountAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
    }

    [Fact]
    public async Task GetCountAsync_RespectsCancellationToken()
    {
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsPagedEntities()
    {
    }

    [Fact]
    public async Task GetPagedListAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
    }

    [Fact]
    public async Task GetPagedListAsync_ThrowsArgumentOutOfRangeException_WhenTakeOrSkipInvalid()
    {
    }

    [Fact]
    public async Task GetPagedListAsync_RespectsCancellationToken()
    {
    }

    [Fact]
    public async Task GetPagedListAsyncTResult_ReturnsPagedProjection()
    {
    }

    [Fact]
    public async Task GetPagedListAsyncTResult_ThrowsArgumentNullException_WhenQueryIsNull()
    {
    }

    [Fact]
    public async Task GetPagedListAsyncTResult_ThrowsArgumentOutOfRangeException_WhenTakeOrSkipInvalid()
    {
    }

    [Fact]
    public async Task GetPagedListAsyncTResult_RespectsCancellationToken()
    {
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities_WhenMatchExists()
    {
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoMatch()
    {
    }

    [Fact]
    public async Task GetAllAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
    }

    [Fact]
    public async Task GetAllAsync_RespectsCancellationToken()
    {
    }

    [Fact]
    public async Task GetAllAsyncTResult_ReturnsAllProjections_WhenMatchExists()
    {
    }

    [Fact]
    public async Task GetAllAsyncTResult_ReturnsEmptyList_WhenNoMatch()
    {
    }

    [Fact]
    public async Task GetAllAsyncTResult_ThrowsArgumentNullException_WhenQueryIsNull()
    {
    }

    [Fact]
    public async Task GetAllAsyncTResult_RespectsCancellationToken()
    {
    }
}
