using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.QuerySpec.Tests.Extensions;

public class StringValueObjectSearchTests
{
    [Fact]
    public async Task Search_StringBackedValueObject_ExecutesOnDatabase()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddQuerySpecStringValueObjectSearch();
        services.AddDbContext<TestDbContext>(options => options.UseSqlite(connection));

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();

        context.Entities.AddRange(
            new TestEntity { Code = new TestCode("890123456789") },
            new TestEntity { Code = new TestCode("999999999999") });

        await context.SaveChangesAsync();

        var query = new Query<TestEntity>();
        query.Search(x => x.Code, "%123%");

        var result = await SearchEvaluator.Instance
            .GetQuery(context.Entities, query)
            .ToListAsync();

        var entity = Assert.Single(result);
        Assert.Equal("890123456789", entity.Code.Value);
    }

    [Fact]
    public async Task Search_DifferentGroups_AreCombinedWithAnd()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddQuerySpecStringValueObjectSearch();
        services.AddDbContext<TestDbContext>(options => options.UseSqlite(connection));

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();

        context.Entities.AddRange(
            new TestEntity { Code = new TestCode("123456"), Name = "Alpha" },
            new TestEntity { Code = new TestCode("123999"), Name = "Beta" });

        await context.SaveChangesAsync();

        var query = new Query<TestEntity>();
        query.Search(x => x.Code, "%123%", searchGroup: 1);
        query.Search(x => x.Name, "%Alpha%", searchGroup: 2);

        var result = await SearchEvaluator.Instance
            .GetQuery(context.Entities, query)
            .ToListAsync();

        var entity = Assert.Single(result);
        Assert.Equal("Alpha", entity.Name);
    }

    [Fact]
    public async Task Search_StringProperty_RemainsSupported()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddQuerySpecStringValueObjectSearch();
        services.AddDbContext<TestDbContext>(options => options.UseSqlite(connection));

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();

        context.Entities.AddRange(
            new TestEntity { Code = new TestCode("111"), Name = "Alpha 123" },
            new TestEntity { Code = new TestCode("222"), Name = "Beta" });

        await context.SaveChangesAsync();

        var query = new Query<TestEntity>();
        query.Search(x => x.Name, "%123%");

        var result = await SearchEvaluator.Instance
            .GetQuery(context.Entities, query)
            .ToListAsync();

        Assert.Single(result);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>()
                .Property(x => x.Code)
                .HasConversion(
                    value => value.Value,
                    value => new TestCode(value));

            StringValueObjectSearch.EnableStringSearch<TestCode>();
        }
    }

    private sealed class TestEntity
    {
        public int Id { get; set; }
        public TestCode Code { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private readonly record struct TestCode(string Value);
}
