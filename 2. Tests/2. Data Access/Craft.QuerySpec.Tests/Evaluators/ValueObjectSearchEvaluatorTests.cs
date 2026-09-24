using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Craft.QuerySpec.Tests.Evaluators;

public class ValueObjectSearchEvaluatorTests
{
    [Fact]
    public void WithQuery_StringBackedValueObject_ExecutesLikeOnServer()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateContext(connection);
        Seed(context);

        var query = new Query<SearchEntity>();
        query.Search(x => x.Code, "%0123%");

        var translatedQuery = context.Entities.WithQuery(query);
        var sql = translatedQuery.ToQueryString();
        var results = translatedQuery.ToList();

        Assert.Contains("LIKE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Single(results);
        Assert.Equal(1, results[0].Id);
    }

    [Fact]
    public void WithQuery_NullableStringBackedValueObject_ExecutesLikeOnServer()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateContext(connection);
        Seed(context);

        var query = new Query<SearchEntity>();
        query.Search(x => x.OptionalCode!, "%999%");

        var translatedQuery = context.Entities.WithQuery(query);
        var sql = translatedQuery.ToQueryString();
        var results = translatedQuery.ToList();

        Assert.Contains("LIKE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Single(results);
        Assert.Equal(1, results[0].Id);
    }

    [Fact]
    public void WithQuery_MultipleSearchGroups_ComposeWithAnd()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateContext(connection);
        Seed(context);

        var query = new Query<SearchEntity>();
        query.Search(x => x.Code, "%0123%", searchGroup: 1);
        query.Search(x => x.OptionalCode!, "%999%", searchGroup: 2);

        var results = context.Entities.WithQuery(query).ToList();

        Assert.Single(results);
        Assert.Equal(1, results[0].Id);
    }

    private static SearchDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<SearchDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new SearchDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static void Seed(SearchDbContext context)
    {
        context.Entities.AddRange(
            new SearchEntity
            {
                Id = 1,
                Code = new TestCode("890123456789"),
                OptionalCode = new TestCode("919999")
            },
            new SearchEntity
            {
                Id = 2,
                Code = new TestCode("894567890123"),
                OptionalCode = null
            });

        context.SaveChanges();
    }

    private sealed class SearchDbContext(DbContextOptions<SearchDbContext> options) : DbContext(options)
    {
        public DbSet<SearchEntity> Entities => Set<SearchEntity>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<TestCode>()
                .HaveConversion<TestCodeConverter>();
        }
    }

    private sealed class SearchEntity
    {
        public int Id { get; set; }
        public TestCode Code { get; set; }
        public TestCode? OptionalCode { get; set; }
    }

    private readonly record struct TestCode(string Value)
    {
        public static explicit operator string(TestCode value) => value.Value;
        public static explicit operator TestCode(string value) => new(value);
    }

    private sealed class TestCodeConverter : ValueConverter<TestCode, string>
    {
        public TestCodeConverter()
            : base(
                value => value.Value,
                value => new TestCode(value))
        {
        }
    }
}
