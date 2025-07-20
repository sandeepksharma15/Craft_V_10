using Craft.QuerySpec;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Core;

/// <summary>
/// Unit tests for Query and Query{T, TResult} covering all edge cases and behaviors.
/// </summary>
public class QueryTests
{
    private readonly IQueryable<Company> queryable;

    public QueryTests()
    {
        // Arrange
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
    [InlineData(-5, 10)]
    [InlineData(0, 10)]
    [InlineData(1, -10)]
    [InlineData(1, 0)]
    public void SetPage_WithInvalidParameters_SetsToDefaults(int page, int pageSize)
    {
        // Arrange
        var query = new Query<Company>();

        // Act
        query.SetPage(page, pageSize);

        // Assert
        Assert.Equal(10, query.Take);
        Assert.Equal(0, query.Skip);
    }

    [Fact]
    public void SetPage_NullOrNegativeValues_UsesDefaults()
    {
        // Arrange
        var query = new Query<Company>();

        // Act
        query.SetPage(-1, -1);

        // Assert
        Assert.Equal(10, query.Take);
        Assert.Equal(0, query.Skip);

        // Act
        query.SetPage(0, 0);

        // Assert
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
    public void Skip_SetNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var query = new Query<Company>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => query.Skip = -1);
    }

    [Fact]
    public void Take_SetNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var query = new Query<Company>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => query.Take = -1);
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
    public void IsSatisfiedBy_NullEntity_ReturnsFalse()
    {
        // Arrange
        var query = new Query<Company>().Where(e => e.Id == 1);

        // Act
        var result = query?.IsSatisfiedBy(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_EmptyQuery_ReturnsTrue()
    {
        // Arrange
        var query = new Query<Company>();
        var company = new Company { Id = 1, Name = "Test" };

        // Act
        var result = query.IsSatisfiedBy(company);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Clear_ResetsAllQuerySpecifications()
    {
        // Arrange
        var query = new Query<Company, object>
        {
            AsNoTracking = false,
            AsSplitQuery = true,
            IgnoreAutoIncludes = false,
            IgnoreQueryFilters = true,
            Skip = 10,
            Take = 5
        };
        query.Where(c => c.Name == "John");
        query.Select(c => c.Name!);

        // Act
        query.Clear();

        // Assert
        Assert.True(query.AsNoTracking);
        Assert.False(query.AsSplitQuery);
        Assert.True(query.IgnoreAutoIncludes);
        Assert.False(query.IgnoreQueryFilters);
        Assert.Equal(0, query.Skip);
        Assert.Equal(10, query.Take);
        Assert.Equal(0, query.EntityFilterBuilder?.EntityFilterList.Count);
        Assert.Equal(0, query.QuerySelectBuilder?.Count);
        Assert.Null(query.SelectorMany);
    }

    [Fact]
    public void Clear_OnDefaultQuery_ResetsAll()
    {
        // Arrange
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
    public void DefaultConstructor_InitializesDefaults()
    {
        // Arrange & Act
        var query = new Query<Company>();

        // Assert
        Assert.True(query.AsNoTracking);
        Assert.True(query.IgnoreAutoIncludes);
        Assert.False(query.AsSplitQuery);
        Assert.False(query.IgnoreQueryFilters);
        Assert.Null(query.Skip);
        Assert.Null(query.Take);
        Assert.NotNull(query.SortOrderBuilder);
        Assert.NotNull(query.SqlLikeSearchCriteriaBuilder);
        Assert.NotNull(query.EntityFilterBuilder);
    }

    [Fact]
    public void PostProcessingAction_Works()
    {
        // Arrange
        var query = new Query<Company>
        {
            PostProcessingAction = items => items.Where(x => x.Id == 1)
        };
        var companies = new List<Company> { new() { Id = 1 }, new() { Id = 2 } };

        // Act
        var result = query.PostProcessingAction(companies);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public void QueryTResult_SelectorManyAndPostProcessingAction_Works()
    {
        // Arrange
        var query = new Query<Company, Company>
        {
            SelectorMany = c => new List<Company> { c },
            PostProcessingAction = items => items.Take(1)
        };
        var companies = new List<Company> { new() { Id = 1 }, new() { Id = 2 } };

        // Act
        var result = query.PostProcessingAction(companies);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void QueryTResult_Clear_ResetsAll()
    {
        // Arrange
        var query = new Query<Company, Company>();
        query.QuerySelectBuilder?.Add(c => c.Name!);
        query.SelectorMany = c => new List<Company> { c };

        // Act
        query.Clear();

        // Assert
        Assert.Equal(0, query.QuerySelectBuilder?.Count);
        Assert.Null(query.SelectorMany);
    }

    [Fact]
    public void QueryTResult_DefaultConstructor_InitializesDefaults()
    {
        // Arrange & Act
        var query = new Query<Company, Company>();

        // Assert
        Assert.NotNull(query.QuerySelectBuilder);
        Assert.Null(query.SelectorMany);
        Assert.Null(query.PostProcessingAction);
    }

    [Fact]
    public void SortOrderBuilder_AddAndClear_Works()
    {
        // Arrange
        var query = new Query<Company>();
        var builder = query.SortOrderBuilder;

        // Act
        builder?.Add(c => c.Name!);
        builder?.Clear();

        // Assert
        Assert.Empty(builder!.OrderDescriptorList);
    }

    [Fact]
    public void SqlLikeSearchCriteriaBuilder_AddAndClear_Works()
    {
        // Arrange
        var query = new Query<Company>();
        var builder = query.SqlLikeSearchCriteriaBuilder;

        // Act
        builder?.Add(c => c.Name!, "Test");
        builder?.Clear();

        // Assert
        Assert.Empty(builder!.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void EntityFilterBuilder_AddAndClear_Works()
    {
        // Arrange
        var query = new Query<Company>();
        var builder = query.EntityFilterBuilder;

        // Act
        builder?.Add(c => c.Name == "Test");
        builder?.Clear();

        // Assert
        Assert.Empty(builder!.EntityFilterList);
    }
}
