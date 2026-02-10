using System.Linq.Expressions;
using Craft.Testing.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.QuerySpec.Tests.Extensions;

[Collection(nameof(SystemTestCollectionDefinition))]
public class SearchExtensionTests : IDisposable
{
    private readonly TestDbContext dbContext;
    private readonly IServiceScope scope;

    public SearchExtensionTests()
    {
        var services = new ServiceCollection()
            .AddDbContext<TestDbContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            });
        var provider = services.BuildServiceProvider();
        scope = provider.CreateScope();
        dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        dbContext?.Dispose();
        scope?.Dispose();
    }

    [Fact]
    public void Search_WithEmptyCriterias_ShouldReturnSource()
    {
        // Arrange
        var criterias = new List<SqlLikeSearchInfo<Company>>();

        // Act
        var result = dbContext?.Companies?.Search(criterias);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dbContext?.Companies?.ToList(), [.. result]);
    }

    [Fact]
    public void Search_WithMatchingCriterias_ShouldReturnFilteredSource()
    {
        // Arrange
        Expression<Func<Company, string>> searchItem = x => x.Name!;
        var criterias = new List<SqlLikeSearchInfo<Company>>
        {
            new(searchItem, "%2")
        };

        // Act
        var result = dbContext?.Companies?.Search(criterias)?.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Company 2", result[0].Name);
    }

    [Fact]
    public void Search_WithNonMatchingCriterias_ShouldReturnEmptySource()
    {
        // Arrange
        Expression<Func<Company, string>> searchItem = x => x.Name!;
        var criterias = new List<SqlLikeSearchInfo<Company>>
        {
            new(searchItem, "NonExistent")
        };

        // Act
        var result = dbContext?.Companies?.Search(criterias);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Search_WithNullSearchString_CriteriaThrowsError()
    {
        // Arrange & Act
        Expression<Func<Company, string>> searchItem = x => x.Name!;
        var ex = Record.Exception(() => new List<SqlLikeSearchInfo<Company>>
        {
            new(searchItem, null!)
        });

        // Assert
        Assert.NotNull(ex);
    }

    [Fact]
    public void Search_WithMultipleCriterias_ShouldReturnUnionOfMatches()
    {
        // Arrange
        Expression<Func<Company, string>> searchItem = x => x.Name!;
        var criterias = new List<SqlLikeSearchInfo<Company>>
        {
            new(searchItem, "%1"),
            new(searchItem, "%2")
        };

        // Act
        var result = dbContext?.Companies?.Search(criterias)?.ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "Company 1");
        Assert.Contains(result, c => c.Name == "Company 2");
    }

    [Fact]
    public void Search_WithNullCriterias_ReturnsSource()
    {
        // Act
        var result = dbContext?.Companies?.Search(null!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dbContext?.Companies?.ToList(), [.. result]);
    }

    [Fact]
    public void Search_WithEmptySource_ReturnsEmpty()
    {
        // Arrange
        var emptyContext = CreateEmptyContext();
        var criterias = new List<SqlLikeSearchInfo<Company>>
        {
            new(x => x.Name!, "%1")
        };

        // Act
        var result = emptyContext?.Companies?.Search(criterias);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        emptyContext?.Dispose();
    }

    private static TestDbContext CreateEmptyContext()
    {
        var services = new ServiceCollection()
            .AddDbContext<TestDbContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            });
        var provider = services.BuildServiceProvider();
        var context = provider.GetRequiredService<TestDbContext>();
        context.Database.EnsureCreated();
        context?.Companies?.RemoveRange(context.Companies);
        context?.SaveChanges();

        return context!;
    }
}

