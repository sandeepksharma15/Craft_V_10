using System.Text.Json;
using Craft.QuerySpec.Core;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Core;

public class QueryTests
{
    private readonly IQueryable<Company> queryable;

    public QueryTests()
    {
        queryable = new List<Company>
        {
            new() { Id = 1, Name = "Company 1" },
            new() { Id = 2, Name = "Company 2" }
        }.AsQueryable();
    }

    [Fact]
    public void SetPage_WithValidPageSizeAndPage_ReturnsCorrectSkipAndTake()
    {
        // Arrange
        var query = new Query<Company>();
        const int pageSize = 10;
        const int page = 2;

        // Act
        query.SetPage(page, pageSize);

        // Assert
        Assert.Equal(pageSize, query.Take);
        Assert.Equal((page - 1) * pageSize, query.Skip);
    }

    [Theory]
    [InlineData(-5, 10)] // Negative page
    [InlineData(0, 10)] // Zero page
    [InlineData(1, -10)] // Negative pageSize
    [InlineData(1, 0)] // Zero pageSize
    public void SetPage_WithInvalidParameters_SetsToDefaults(int page, int pageSize)
    {
        // Arrange
        var query = new Query<Company>();

        // Act
        query.SetPage(page, pageSize);

        // Assert
        Assert.Equal(10, query.Take);
        Assert.Equal(10 * (1 - 1), query.Skip);
    }

    [Fact]
    public void IsSatisfiedBy_ReturnsTrue_ForMatchingEntity()
    {
        // Arrange
        var query = new Query<Company>().Where(e => e.Id == 1);

        // Act
        var result = query?.IsSatisfiedBy(queryable.First());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_ReturnsFalse_ForNonMatchingEntity()
    {
        // Arrange
        var query = new Query<Company>().Where(e => e.Id == 1);

        // Act
        var result = query?.IsSatisfiedBy(queryable.Last());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Clear_ResetsAllQuerySpecifications()
    {
        // Arrange
        var query = new Query<Company, object>
        {
            // Set some query specifications
            AsNoTracking = true,
            Skip = 10
        };
        query.Where(c => c.Name == "John");
        query.Select(c => c.Name!);

        // Act
        query.Clear();

        // Assert
        Assert.True(query.AsNoTracking);
        Assert.Equal(0, query.Skip);
        Assert.Equal(0, query.EntityFilterBuilder?.EntityFilterList.Count);
        Assert.Equal(0, query.QuerySelectBuilder?.Count);
        Assert.Null(query.SelectorMany);
    }

    [Fact]
    public void SerializeDeserialize_SimpleQuery_T_PreservesValues()
    {
        // Arrange
        var query = new Query<Company>
        {
            AsNoTracking = true,
            Skip = 10
        };
        query.Where(c => c.Name == "John");
        query.OrderBy(c => c.Id);

        // Act
        var serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new QueryJsonConverter<Company>());

        var serializedQuery = JsonSerializer.Serialize(query, serializeOptions);
        var deserializedQuery = JsonSerializer.Deserialize<Query<Company>>(serializedQuery, serializeOptions);

        // Assert
        Assert.NotNull(deserializedQuery);
        Assert.Equal(1, deserializedQuery.EntityFilterBuilder?.Count);
        Assert.Equal(1, deserializedQuery.SortOrderBuilder?.OrderDescriptorList.Count);
        Assert.True(deserializedQuery.AsNoTracking);
        Assert.Equal(10, deserializedQuery.Skip);
    }

    [Fact]
    public void SerializeDeserialize_SimpleQuery_T_TResult_PreservesValues()
    {
        // Arrange
        var query = new Query<Company, Company>
        {
            AsNoTracking = true,
            Skip = 10
        };
        query.Where(c => c.Name == "John");
        query.Select(c => c.Name!);
        query.OrderBy(c => c.Id);

        // Act
        var serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new QueryJsonConverter<Company, Company>());

        var serializedQuery = JsonSerializer.Serialize(query, serializeOptions);
        var deserializedQuery = JsonSerializer.Deserialize<Query<Company, Company>>(serializedQuery, serializeOptions);

        // Assert
        Assert.NotNull(deserializedQuery);
        Assert.Equal(1, deserializedQuery.EntityFilterBuilder?.Count);
        Assert.Equal(1, deserializedQuery.SortOrderBuilder?.OrderDescriptorList.Count);
        Assert.True(deserializedQuery.AsNoTracking);
        Assert.Equal(10, deserializedQuery.Skip);
        Assert.Equal(1, deserializedQuery.QuerySelectBuilder?.Count);
    }

    [Fact]
    public void DefaultConstructor_InitializesDefaults()
    {
        // Arrange & Act
        var query = new Query<Company>();

        // Assert
        Assert.True(query.AsNoTracking);
        Assert.True(query.IgnoreAutoIncludes);
        Assert.False(query.AsSplitQuery);
        Assert.False(query.IgnoreQueryFilters);
        Assert.Equal(null!, query.Skip);
        Assert.Equal(null!, query.Take);
        Assert.NotNull(query.SortOrderBuilder);
        Assert.NotNull(query.SqlLikeSearchCriteriaBuilder);
        Assert.NotNull(query.EntityFilterBuilder);
    }

    [Fact]
    public void SetPage_NullOrNegativeValues_UsesDefaults()
    {
        // Arrange
        var query = new Query<Company>();

        // Act & Assert
        query.SetPage(-1, -1);
        Assert.Equal(10, query.Take);
        Assert.Equal(0, query.Skip);

        // Act & Assert
        query.SetPage(0, 0);
        Assert.Equal(10, query.Take);
        Assert.Equal(0, query.Skip);
    }

    [Fact]
    public void SetPage_LargePage_SetsSkipCorrectly()
    {
        // Arrange
        var query = new Query<Company>();

        // Act
        query.SetPage(5, 20);

        // Assert
        Assert.Equal(20, query.Take);
        Assert.Equal(80, query.Skip);
    }

    [Fact]
    public void Clear_OnDefaultQuery_ResetsAll()
    {
        // Arrange & Act
        var query = new Query<Company>();

        // Act
        query.Clear();

        // Assert
        Assert.True(query.AsNoTracking);
        Assert.True(query.IgnoreAutoIncludes);
        Assert.False(query.AsSplitQuery);
        Assert.False(query.IgnoreQueryFilters);
        Assert.Equal(0, query.Skip);
        Assert.Equal(10, query.Take);
        Assert.Empty(query.SortOrderBuilder?.OrderDescriptorList!);
        Assert.Empty(query.SqlLikeSearchCriteriaBuilder?.SqlLikeSearchCriteriaList!);
        Assert.Empty(query.EntityFilterBuilder?.EntityFilterList!);
    }

    [Fact]
    public void IsSatisfiedBy_NullEntity_ReturnsFalse()
    {
        // Arrange
        var query = new Query<Company>().Where(e => e.Id == 1);

        // Act & Assert
        Assert.False(query?.IsSatisfiedBy(null!));
    }

    [Fact]
    public void IsSatisfiedBy_EmptyQuery_ReturnsTrue()
    {
        var query = new Query<Company>();
        var company = new Company { Id = 1, Name = "Test" };
        Assert.True(query.IsSatisfiedBy(company));
    }

    [Fact]
    public void PostProcessingAction_Works()
    {
        var query = new Query<Company>
        {
            PostProcessingAction = items => items.Where(x => x.Id == 1)
        };
        var companies = new List<Company> { new() { Id = 1 }, new() { Id = 2 } };
        var result = query.PostProcessingAction(companies);
        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public void QueryTResult_SelectorManyAndPostProcessingAction_Works()
    {
        var query = new Query<Company, Company>
        {
            SelectorMany = c => new List<Company> { c },
            PostProcessingAction = items => items.Take(1)
        };
        var companies = new List<Company> { new() { Id = 1 }, new() { Id = 2 } };
        var result = query.PostProcessingAction(companies);
        Assert.Single(result);
    }

    [Fact]
    public void QueryTResult_Clear_ResetsAll()
    {
        var query = new Query<Company, Company>();
        query.QuerySelectBuilder?.Add(c => c.Name!);
        query.SelectorMany = c => new List<Company> { c };
        query.Clear();
        Assert.Equal(0, query.QuerySelectBuilder?.Count);
        Assert.Null(query.SelectorMany);
    }

    [Fact]
    public void QueryTResult_DefaultConstructor_InitializesDefaults()
    {
        var query = new Query<Company, Company>();
        Assert.NotNull(query.QuerySelectBuilder);
        Assert.Null(query.SelectorMany);
        Assert.Null(query.PostProcessingAction);
    }
}
