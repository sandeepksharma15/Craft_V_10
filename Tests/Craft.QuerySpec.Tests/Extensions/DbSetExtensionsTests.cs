using Craft.QuerySpec;
using Craft.TestDataStore.Fixtures;
using Craft.TestDataStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Craft.TestDataStore;

namespace Craft.QuerySpec.Tests.Extensions;

[Collection(nameof(SystemTestCollectionDefinition))]
public class DbSetExtensionsTests : IDisposable
{
    private readonly TestDbContext dbContext;
    private readonly IServiceScope scope;

    public DbSetExtensionsTests()
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

        // Ensure seed data is present for live testing
        if (dbContext.Companies != null && !dbContext.Companies.Any())
        {
            dbContext.Companies.AddRange(CompanySeed.Get());
            dbContext.SaveChanges();
        }
    }

    public void Dispose()
    {
        dbContext?.Dispose();
        scope?.Dispose();
    }

    [Fact]
    public async Task ToEnumerableAsync_Returns_Results()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(u => u.Name == CompanySeed.COMPANY_NAME_1);

        // Act
        var result = (await dbContext?.Companies?.ToEnumerableAsync(query)! ?? []).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(CompanySeed.COMPANY_NAME_1, result[0].Name);
    }

    [Fact]
    public async Task ToEnumerableAsync_Returns_Empty_For_NoMatch()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(u => u.Name == "NonExistent");

        // Act
        var result = (await dbContext?.Companies?.ToEnumerableAsync(query)! ?? []).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ToEnumerableAsync_Returns_All_For_NullQuery()
    {
        // Arrange & Act
        var result = (await dbContext?.Companies?.ToEnumerableAsync(null)! ?? []).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task ToEnumerableAsync_Applies_PostProcessing()
    {
        // Arrange
        var query = new Query<Company>
        {
            PostProcessingAction = items => items.Take(1)
        };

        // Act
        var result = (await dbContext?.Companies?.ToEnumerableAsync(query)! ?? []).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task ToListAsync_Returns_Results()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(u => u.Name == CompanySeed.COMPANY_NAME_1);

        // Act
        var result = await dbContext?.Companies?.ToListAsync(query)! ?? [];

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(CompanySeed.COMPANY_NAME_1, result[0].Name);
    }

    [Fact]
    public async Task ToListAsync_Returns_Empty_For_NoMatch()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(u => u.Name == "NonExistent");

        // Act
        var result = await dbContext?.Companies?.ToListAsync(query)! ?? [];

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ToListAsync_Applies_PostProcessing()
    {
        // Arrange
        var query = new Query<Company>
        {
            PostProcessingAction = items => items.Take(2)
        };

        // Act
        var result = await dbContext?.Companies?.ToListAsync(query)! ?? [];

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void WithQuery_Returns_Filtered_IQueryable()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(u => u.Name == CompanySeed.COMPANY_NAME_1);

        // Act
        var result = dbContext?.Companies?.WithQuery(query).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(CompanySeed.COMPANY_NAME_1, result[0].Name);
    }

    [Fact]
    public void WithQuery_Returns_All_For_NullQuery()
    {
        // Arrange & Act
        var result = dbContext?.Companies?.WithQuery(null).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void WithQuery_Returns_Empty_For_NoMatch()
    {
        // Arrange
        var query = new Query<Company>();
        query.Where(u => u.Name == "NonExistent");

        // Act
        var result = dbContext?.Companies?.WithQuery(query).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void WithQueryWithTResult_Returns_Projected_IQueryable()
    {
        // Arrange
        var query = new Query<Company, CompanyName>();
        query.Where(u => u.Name == CompanySeed.COMPANY_NAME_1);
        query.Select(u => u.Name!);

        // Act
        var result = dbContext?.Companies?.WithQuery(query).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(CompanySeed.COMPANY_NAME_1, result[0].Name);
    }

    [Fact]
    public void WithQueryWithTResult_Throws_If_No_Select()
    {
        // Arrange
        var query = new Query<Company, CompanyName>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => dbContext?.Companies?.WithQuery(query).ToList());
    }

    [Fact]
    public void WithQueryWithTResult_Throws_If_Both_Select_And_SelectMany()
    {
        // Arrange
        var query = new Query<Company, CompanyName>();
        query.Select(u => u.Name!);
        query.SelectorMany = c => new List<CompanyName> { new() { Name = c.Name } };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => dbContext?.Companies?.WithQuery(query).ToList());
    }

    [Fact]
    public void WithQueryWithTResult_Returns_Empty_For_NoMatch()
    {
        // Arrange
        var query = new Query<Company, CompanyName>();
        query.Where(u => u.Name == "NonExistent");
        query.Select(u => u.Name!);

        // Act
        var result = dbContext?.Companies?.WithQuery(query).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
