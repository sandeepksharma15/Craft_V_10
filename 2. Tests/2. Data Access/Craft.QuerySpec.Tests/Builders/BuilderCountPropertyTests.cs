using Craft.Testing.Fixtures;

namespace Craft.QuerySpec.Tests.Builders;

public class BuilderCountPropertyTests
{
    [Fact]
    public void SortOrderBuilder_Count_ReturnsIntNotLong()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act
        builder.Add(c => c.Name!);
        builder.Add(c => c.Id);

        // Assert
        Assert.IsType<int>(builder.Count);
        Assert.Equal(2, builder.Count);
    }

    [Fact]
    public void EntityFilterBuilder_Count_ReturnsIntNotLong()
    {
        // Arrange
        var builder = new EntityFilterBuilder<Company>();

        // Act
        builder.Add(c => c.Name == "Test");
        builder.Add(c => c.Id == 1);

        // Assert
        Assert.IsType<int>(builder.Count);
        Assert.Equal(2, builder.Count);
    }

    [Fact]
    public void QuerySelectBuilder_Count_ReturnsIntNotLong()
    {
        // Arrange
        var builder = new QuerySelectBuilder<Company, CompanyDto>();

        // Act
        builder.Add(c => c.Name!, d => d.Name!);
        builder.Add(c => c.Id, d => d.Id);

        // Assert
        Assert.IsType<int>(builder.Count);
        Assert.Equal(2, builder.Count);
    }

    [Fact]
    public void SqlLikeSearchCriteriaBuilder_Count_ReturnsIntNotLong()
    {
        // Arrange
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act
        builder.Add(c => c.Name!, "Test%");
        builder.Add(c => c.Name!, "%Test");

        // Assert
        Assert.IsType<int>(builder.Count);
        Assert.Equal(2, builder.Count);
    }

    [Fact]
    public void AllBuilders_Count_MatchesUnderlyingListCount()
    {
        // Arrange & Act & Assert
        var sortBuilder = new SortOrderBuilder<Company>();
        sortBuilder.Add(c => c.Name!);
        Assert.Equal(sortBuilder.OrderDescriptorList.Count, sortBuilder.Count);

        var filterBuilder = new EntityFilterBuilder<Company>();
        filterBuilder.Add(c => c.Name == "Test");
        Assert.Equal(filterBuilder.EntityFilterList.Count, filterBuilder.Count);

        var selectBuilder = new QuerySelectBuilder<Company, CompanyDto>();
        selectBuilder.Add(c => c.Name!, d => d.Name!);
        Assert.Equal(selectBuilder.SelectDescriptorList.Count, selectBuilder.Count);

        var searchBuilder = new SqlLikeSearchCriteriaBuilder<Company>();
        searchBuilder.Add(c => c.Name!, "Test");
        Assert.Equal(searchBuilder.SqlLikeSearchCriteriaList.Count, searchBuilder.Count);
    }

    [Fact]
    public void AllBuilders_Count_StartsAtZero()
    {
        // Assert
        Assert.Equal(0, new SortOrderBuilder<Company>().Count);
        Assert.Equal(0, new EntityFilterBuilder<Company>().Count);
        Assert.Equal(0, new QuerySelectBuilder<Company, CompanyDto>().Count);
        Assert.Equal(0, new SqlLikeSearchCriteriaBuilder<Company>().Count);
    }

    [Fact]
    public void AllBuilders_Count_UpdatesAfterClear()
    {
        // Arrange
        var sortBuilder = new SortOrderBuilder<Company>();
        sortBuilder.Add(c => c.Name!);
        sortBuilder.Clear();
        Assert.Equal(0, sortBuilder.Count);

        var filterBuilder = new EntityFilterBuilder<Company>();
        filterBuilder.Add(c => c.Name == "Test");
        filterBuilder.Clear();
        Assert.Equal(0, filterBuilder.Count);

        var selectBuilder = new QuerySelectBuilder<Company, CompanyDto>();
        selectBuilder.Add(c => c.Name!, d => d.Name!);
        selectBuilder.Clear();
        Assert.Equal(0, selectBuilder.Count);

        var searchBuilder = new SqlLikeSearchCriteriaBuilder<Company>();
        searchBuilder.Add(c => c.Name!, "Test");
        searchBuilder.Clear();
        Assert.Equal(0, searchBuilder.Count);
    }

    private class CompanyDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
