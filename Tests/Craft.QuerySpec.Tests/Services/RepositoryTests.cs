using Craft.Testing.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.QuerySpec.Tests.Services;

[Collection(nameof(SystemTestCollectionDefinition))]
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
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var logger = new Logger<Repository<NoSoftDeleteEntity, KeyType>>(new LoggerFactory());
        var repo = new Repository<NoSoftDeleteEntity, KeyType>(context, logger);

        var entities = new List<NoSoftDeleteEntity>
        {
            new() { Desc = "Entity A" },
            new() { Desc = "Entity B" }
        };
        context.NoSoftDeleteEntities?.AddRange(entities);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        IQuery<NoSoftDeleteEntity> query = new Query<NoSoftDeleteEntity>();
        query.Where(e => e.Desc!.Contains('A'));
        await repo.DeleteAsync(query);

        // Assert
        Assert.Equal(1, await context.NoSoftDeleteEntities!.CountAsync());
        Assert.Equal("Entity B", (await context.NoSoftDeleteEntities!.FirstAsync()).Desc);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesEntities_WhenImplementsISoftDelete()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
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
        IQuery<Country> countryQuery = new Query<Country>();
        countryQuery.Where(c => c.Name!.Contains('A'));
        await repo.DeleteAsync(countryQuery);

        // Assert
        Assert.Equal(1, await context.Countries!.CountAsync(c => c.IsDeleted));
        Assert.Equal(1, await context.Countries!.CountAsync(c => c.Name == "Country B"));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.DeleteAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_RespectsCancellationToken()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "A" }, new() { Name = "B" } };
        context.Countries?.AddRange(countries);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Create a query to delete
        IQuery<Country> query = new Query<Country>();
        query.Where(c => c.Name == "A");
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repo.DeleteAsync(query, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotSave_WhenAutoSaveIsFalse()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "A" }, new() { Name = "B" } };
        context.Countries?.AddRange(countries);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Create a query to delete
        IQuery<Country> query = new Query<Country>();
        query.Where(c => c.Name == "A");
        await repo.DeleteAsync(query, autoSave: false);

        // Assert that the entities are not deleted yet
        Assert.Equal(4, await context.Countries!.CountAsync()); // Not yet saved
        await context.SaveChangesAsync();
        Assert.Equal(1, await context.Countries!.CountAsync(c => c.IsDeleted));
    }

    [Fact]
    public async Task GetAsync_ReturnsEntity_WhenMatchExists()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var country = new Country { Name = "TestCountry" };
        context.Countries?.Add(country);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        IQuery<Country> query = new Query<Country>();
        query.Where(c => c.Name == "TestCountry");
        var result = await repo.GetAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestCountry", result!.Name);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenNoMatch()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        IQuery<Country> query = new Query<Country>();
        query.Where(c => c.Name == "DoesNotExist");

        // Act
        var result = await repo.GetAsync(query);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetAsync(null!));
    }

    [Fact]
    public async Task GetAsync_RespectsCancellationToken()
    {

        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var country = new Country { Name = "TestCountry" };
        context.Countries?.Add(country);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Create a query to get the country
        IQuery<Country> query = new Query<Country>();
        query.Where(c => c.Name == "TestCountry");
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repo.GetAsync(query, cts.Token));
    }

    [Fact]
    public async Task GetAsyncTResult_ReturnsProjection_WhenMatchExists()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var country = new Country { Name = "TestCountry" };
        context.Countries?.Add(country);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var query = new Query<Country, CountryDto>
        {
            QuerySelectBuilder = new QuerySelectBuilder<Country, CountryDto>().Add(c => c.Name!, d => d.Name!)
        };
        query.Where(c => c.Name == "TestCountry");
        var result = await repo.GetAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestCountry", result!.Name);
    }

    [Fact]
    public async Task GetAsyncTResult_ReturnsNull_WhenNoMatch()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var query = new Query<Country, CountryDto>
        {
            QuerySelectBuilder = new QuerySelectBuilder<Country, CountryDto>()
                .Add(c => c.Name!, d => d.Name!)
        };
        query.Where(c => c.Name == "DoesNotExist");

        // Act
        var result = await repo.GetAsync(query);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsyncTResult_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetAsync<CountryDto>(null!));
    }

    [Fact]
    public async Task GetAsyncTResult_RespectsCancellationToken()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var country = new Country { Name = "TestCountry" };
        context.Countries?.Add(country);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Create a query to get the country
        var query = new Query<Country, CountryDto>
        {
            QuerySelectBuilder = new QuerySelectBuilder<Country, CountryDto>()
                .Add(c => c.Name!, d => d.Name!)
        };
        query.Where(c => c.Name == "TestCountry");
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repo.GetAsync(query, cts.Token));
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "A" }, new() { Name = "B" } };
        context.Countries?.AddRange(countries);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        IQuery<Country> query = new Query<Country>();
        query.Where(c => c.Name != null);
        var count = await repo.GetCountAsync(query);

        // Assert
        Assert.Equal(4, count);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsZero_WhenNoMatch()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act
        IQuery<Country> query = new Query<Country>();
        query.Where(c => c.Name == "DoesNotExist");
        var count = await repo.GetCountAsync(query);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetCountAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetCountAsync(null!));
    }

    [Fact]
    public async Task GetCountAsync_RespectsCancellationToken()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);
        var country = new Country { Name = "TestCountry" };
        context.Countries?.Add(country);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Create a query to count the country
        IQuery<Country> query = new Query<Country>();
        query.Where(c => c.Name == "TestCountry");
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repo.GetCountAsync(query, cts.Token));
    }

    [Fact]
    public async Task GetPagedListAsync_ReturnsPagedEntities()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var countries = new List<Country> {
            new() { Name = "A" },
            new() { Name = "B" },
            new() { Name = "C" }
        };
        context.Countries?.AddRange(countries);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        IQuery<Country> query = new Query<Country> { Skip = 1, Take = 1 };
        var page = await repo.GetPagedListAsync(query);

        // Assert
        Assert.Single(page.Items);
        Assert.Equal(5, page.TotalCount);
    }

    [Fact]
    public async Task GetPagedListAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetPagedListAsync(null!));
    }

    [Fact]
    public async Task GetPagedListAsync_ThrowsArgumentOutOfRangeException_WhenTakeOrSkipInvalid()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Query<Country> { Skip = 0, Take = 0 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Query<Country> { Skip = -1, Take = 1 });
    }

    [Fact]
    public async Task GetPagedListAsync_RespectsCancellationToken()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "A" }, new() { Name = "B" } };
        context.Countries?.AddRange(countries);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Create a query to get paged list
        IQuery<Country> query = new Query<Country> { Skip = 0, Take = 1 };
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repo.GetPagedListAsync(query, cts.Token));
    }

    [Fact]
    public async Task GetPagedListAsyncTResult_ReturnsPagedProjection()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var countries = new List<Country> {
            new() { Name = "A" },
            new() { Name = "B" },
            new() { Name = "C" }
        };
        context.Countries?.AddRange(countries);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var query = new Query<Country, CountryDto>
        {
            Skip = 1,
            Take = 1,
            QuerySelectBuilder = new QuerySelectBuilder<Country, CountryDto>().Add(c => c.Name!, d => d.Name!)
        };
        var page = await repo.GetPagedListAsync(query);

        // Assert
        Assert.Single(page.Items);
        Assert.Equal(5, page.TotalCount);
    }

    [Fact]
    public async Task GetPagedListAsyncTResult_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetPagedListAsync<CountryDto>(null!));
    }

    [Fact]
    public async Task GetPagedListAsyncTResult_ThrowsArgumentOutOfRangeException_WhenTakeOrSkipInvalid()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();
        var repo = CreateRepository(context);

        // Act & Assert
        var act = () => new Query<Country, CountryDto>
        {
            Skip = 0,
            Take = 0,
            QuerySelectBuilder = new QuerySelectBuilder<Country, CountryDto>().Add(c => c.Name!, d => d.Name!)
        };
        Assert.Throws<ArgumentOutOfRangeException>(act);

        // Act & Assert

        act = () => new Query<Country, CountryDto>
        {
            Skip = -1,
            Take = 1,
            QuerySelectBuilder = new QuerySelectBuilder<Country, CountryDto>().Add(c => c.Name!, d => d.Name!)
        };
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public async Task GetPagedListAsyncTResult_RespectsCancellationToken()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "A" }, new() { Name = "B" } };
        context.Countries?.AddRange(countries);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Create a query to get paged list
        var query = new Query<Country, CountryDto>
        {
            Skip = 0,
            Take = 1,
            QuerySelectBuilder = new QuerySelectBuilder<Country, CountryDto>().Add(c => c.Name!, d => d.Name!)
        };
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repo.GetPagedListAsync(query, cts.Token));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities_WhenMatchExists()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "A" }, new() { Name = "B" } };
        context.Countries?.AddRange(countries);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        IQuery<Country> query = new Query<Country>();
        query.Where(c => c.Name != null);
        var result = await repo.GetAllAsync(query);

        // Assert
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoMatch()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);

        // Act
        IQuery<Country> query = new Query<Country>();
        query.Where(c => c.Name == "DoesNotExist");
        var result = await repo.GetAllAsync(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetAllAsync(null!));
    }

    [Fact]
    public async Task GetAllAsync_RespectsCancellationToken()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var country = new Country { Name = "TestCountry" };
        context.Countries?.Add(country);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Create a query to get all countries
        IQuery<Country> query = new Query<Country>();
        query.Where(c => c.Name == "TestCountry");
        using var cts = new CancellationTokenSource();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repo.GetAllAsync(query, cts.Token));
    }

    [Fact]
    public async Task GetAllAsyncTResult_ReturnsAllProjections_WhenMatchExists()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var countries = new List<Country> { new() { Name = "A" }, new() { Name = "B" } };
        context.Countries?.AddRange(countries);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        var query = new Query<Country, CountryDto>
        {
            QuerySelectBuilder = new QuerySelectBuilder<Country, CountryDto>()
                .Add(c => c.Name!, d => d.Name!)
        };
        var result = await repo.GetAllAsync(query);

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Contains(result, r => r.Name == "A");
        Assert.Contains(result, r => r.Name == "B");
    }

    [Fact]
    public async Task GetAllAsyncTResult_ReturnsEmptyList_WhenNoMatch()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        context.Database.EnsureCreated();

        var repo = CreateRepository(context);
        var query = new Query<Country, CountryDto>
        {
            QuerySelectBuilder = new QuerySelectBuilder<Country, CountryDto>()
                .Add(c => c.Name!, d => d.Name!)
        };
        query.Where(c => c.Name == "DoesNotExist");

        // Act
        var result = await repo.GetAllAsync(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsyncTResult_ThrowsArgumentNullException_WhenQueryIsNull()
    {
        // Arrange
        await using var context = new TestDbContext(CreateOptions());
        var repo = CreateRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetAllAsync<CountryDto>(null!));
    }

    // DTO for projection tests
    private class CountryDto
    {
        public string? Name { get; set; }
    }
}
