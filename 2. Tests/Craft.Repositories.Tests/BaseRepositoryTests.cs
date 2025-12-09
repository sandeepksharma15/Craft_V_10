using Craft.Testing.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Repositories.Tests;

[Collection(nameof(SystemTestCollectionDefinition))]
public class BaseRepositoryTests
{
    private readonly DbContextOptions<TestDbContext> options;

    public BaseRepositoryTests()
    {
        options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .Options;
    }

    [Fact]
    public async Task GetDbContextAsync_Returns_Context()
    {
        // Arrange
        using var context = new TestDbContext(options);
        var logger = new TestLogger<BaseRepository<Company, long>>();
        var repo = new BaseRepository<Company, long>(context, logger);

        // Act
        var dbContext = await repo.GetDbContextAsync();

        // Assert
        Assert.Equal(context, dbContext);
    }

    [Fact]
    public async Task GetDbSetAsync_Returns_DbSet()
    {
        // Arrange
        using var context = new TestDbContext(options);
        var logger = new TestLogger<BaseRepository<Company, long>>();
        var repo = new BaseRepository<Company, long>(context, logger);

        // Act
        var dbSet = await repo.GetDbSetAsync();

        // Assert
        Assert.Equal(context.Companies, dbSet);
    }

    [Fact]
    public void SaveChanges_Returns_Zero_When_No_Changes()
    {
        // Arrange
        using var context = new TestDbContext(options);
        var logger = new TestLogger<BaseRepository<Company, long>>();
        var repo = new BaseRepository<Company, long>(context, logger);

        // Act
        var result = repo.SaveChanges();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task SaveChangesAsync_Returns_Zero_When_No_Changes()
    {
        // Arrange
        using var context = new TestDbContext(options);
        var logger = new TestLogger<BaseRepository<Company, long>>();
        var repo = new BaseRepository<Company, long>(context, logger);

        // Act
        var result = await repo.SaveChangesAsync();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void SaveChanges_Persists_Entity()
    {
        // Arrange
        using var context = new TestDbContext(options);
        var logger = new TestLogger<BaseRepository<Company, long>>();
        var repo = new BaseRepository<Company, long>(context, logger);
        var company = new Company { Name = "Test Company", CountryId = 1L };

        // Act
        context.Companies!.Add(company);
        var result = repo.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.Contains(context.Companies!, c => c.Name == "Test Company");
    }

    [Fact]
    public async Task SaveChangesAsync_Persists_Entity()
    {
        // Arrange
        using var context = new TestDbContext(options);
        var logger = new TestLogger<BaseRepository<Company, long>>();
        var repo = new BaseRepository<Company, long>(context, logger);
        var company = new Company { Name = "Test Company Async", CountryId = 1L };

        // Act
        context.Companies!.Add(company);
        var result = await repo.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Contains(context.Companies!, c => c.Name == "Test Company Async");
    }

    [Fact]
    public async Task SaveChangesAsync_CancellationToken_Works()
    {
        // Arrange
        using var context = new TestDbContext(options);
        var logger = new TestLogger<BaseRepository<Company, long>>();
        var repo = new BaseRepository<Company, long>(context, logger);
        var company = new Company { Name = "Test Company Async CT", CountryId = 1L };

        // Act
        context.Companies!.Add(company);
        var result = await repo.SaveChangesAsync(CancellationToken.None);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Constructor_Initializes_Fields()
    {
        // Arrange & Act
        using var context = new TestDbContext(options);
        var logger = new TestLogger<BaseRepository<Company, long>>();
        var repo = new BaseRepository<Company, long>(context, logger);

        // Assert
        Assert.NotNull(repo);
    }

    [Fact]
    public void SaveChanges_Throws_When_Context_Disposed()
    {
        // Arrange
        var context = new TestDbContext(options);
        var logger = new TestLogger<BaseRepository<Company, long>>();
        var repo = new BaseRepository<Company, long>(context, logger);

        // Act
        context.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(() => repo.SaveChanges());
    }

    [Fact]
    public async Task SaveChangesAsync_Throws_When_Context_Disposed()
    {
        // Arrange
        var context = new TestDbContext(options);
        var logger = new TestLogger<BaseRepository<Company, long>>();
        var repo = new BaseRepository<Company, long>(context, logger);

        // Act
        context.Dispose();

        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => repo.SaveChangesAsync());
    }

    // Explicitly implement the interface method to resolve the nullability mismatch issue.
    private class TestLogger<T> : ILogger<T>
    {
        IDisposable ILogger.BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        { }
    }
}
