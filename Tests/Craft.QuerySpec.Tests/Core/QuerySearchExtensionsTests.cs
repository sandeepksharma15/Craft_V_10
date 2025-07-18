using System.Linq.Expressions;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Core;

public class QuerySearchExtensionsTests
{
    [Fact]
    public void Search_WithValidArguments_ShouldAddSearchCriteria()
    {
        // Arrange
        var query = new Query<Company>();
        Expression<Func<Company, object>> member = x => x.Name!;
        const string searchTerm = "A%C";
        const int searchGroup = 1;

        // Act
        var result = query.Search(member, searchTerm, searchGroup);
        var searchCriteria = query.SqlLikeSearchCriteriaBuilder?.SqlLikeSearchCriteriaList[0];

        // Assert
        Assert.Same(query, result);
        Assert.Equal(1, query.SqlLikeSearchCriteriaBuilder?.Count);
        Assert.Equal(member.ToString(), searchCriteria?.SearchItem?.ToString());
        Assert.Equal(searchTerm, searchCriteria?.SearchString);
        Assert.Equal(searchGroup, searchCriteria?.SearchGroup);
    }

    [Fact]
    public void Search_ByMemberName_WithValidArguments_ShouldAddSearchCriteria()
    {
        // Arrange
        var query = new Query<Company>();
        const string member = "Name";
        const string searchTerm = "A%C";
        const int searchGroup = 1;

        // Act
        var result = query.Search(member, searchTerm, searchGroup);
        var searchCriteria = query.SqlLikeSearchCriteriaBuilder?.SqlLikeSearchCriteriaList[0];

        // Assert
        Assert.Same(query, result);
        Assert.Equal(1, query.SqlLikeSearchCriteriaBuilder?.Count);
        Assert.Contains(member, searchCriteria?.SearchItem?.Body.ToString());
        Assert.Equal(searchTerm, searchCriteria?.SearchString);
        Assert.Equal(searchGroup, searchCriteria?.SearchGroup);
    }

    [Fact]
    public void Search_WithNullQuery_ShouldReturnNull()
    {
        // Arrange
        IQuery<Company> query = null!;
        Expression<Func<Company, object>> member = x => x.Name!;
        const string searchTerm = "A%C";
        const int searchGroup = 1;

        // Act
        var result = query.Search(member, searchTerm, searchGroup);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Search_WithNullMember_ShouldReturnQueryAndNotAddCriteria()
    {
        // Arrange
        var query = new Query<Company>();
        Expression<Func<Company, object>> member = null!;
        const string searchTerm = "test";
        const int searchGroup = 1;

        // Act
        var result = query.Search(member, searchTerm, searchGroup);

        // Assert
        Assert.Same(query, result);
        Assert.Empty(query.SqlLikeSearchCriteriaBuilder?.SqlLikeSearchCriteriaList!);
    }

    [Fact]
    public void Search_WithNullSearchTerm_ShouldReturnQueryAndNotAddCriteria()
    {
        // Arrange
        var query = new Query<Company>();
        Expression<Func<Company, object>> member = x => x.Name!;
        string searchTerm = null!;
        const int searchGroup = 1;

        // Act
        var result = query.Search(member, searchTerm, searchGroup);

        // Assert
        Assert.Same(query, result);
        Assert.Empty(query.SqlLikeSearchCriteriaBuilder?.SqlLikeSearchCriteriaList!);
    }

    [Fact]
    public void Search_ByMemberName_WithNullQuery_ShouldReturnNull()
    {
        // Arrange
        IQuery<Company> query = null!;
        const string member = "Name";
        const string searchTerm = "A%C";
        const int searchGroup = 1;

        // Act
        var result = query.Search(member, searchTerm, searchGroup);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Search_ByMemberName_WithNullMemberName_ShouldReturnQueryAndNotAddCriteria()
    {
        // Arrange
        var query = new Query<Company>();
        string member = null!;
        const string searchTerm = "A%C";
        const int searchGroup = 1;

        // Act
        var result = query.Search(member, searchTerm, searchGroup);

        // Assert
        Assert.Same(query, result);
        Assert.Empty(query.SqlLikeSearchCriteriaBuilder?.SqlLikeSearchCriteriaList!);
    }

    [Fact]
    public void Search_ByMemberName_WithNullSearchTerm_ShouldReturnQueryAndNotAddCriteria()
    {
        // Arrange
        var query = new Query<Company>();
        const string member = "Name";
        string searchTerm = null!;
        const int searchGroup = 1;

        // Act
        var result = query.Search(member, searchTerm, searchGroup);

        // Assert
        Assert.Same(query, result);
        Assert.Empty(query.SqlLikeSearchCriteriaBuilder?.SqlLikeSearchCriteriaList!);
    }

    [Fact]
    public void Search_WithNullSqlLikeSearchCriteriaBuilder_DoesNotThrow()
    {
        // Arrange
        var query = new QueryWithNullSqlLikeSearchCriteriaBuilder();
        Expression<Func<Company, object>> member = x => x.Name!;
        const string searchTerm = "A%C";
        const int searchGroup = 1;

        // Act & Assert
        var result = query.Search(member, searchTerm, searchGroup);
        Assert.Same(query, result);
    }

    private class QueryWithNullSqlLikeSearchCriteriaBuilder : IQuery<Company>
    {
        public bool AsNoTracking { get; set; }
        public bool AsSplitQuery { get; set; }
        public bool IgnoreAutoIncludes { get; set; }
        public bool IgnoreQueryFilters { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public SortOrderBuilder<Company>? SortOrderBuilder { get; set; } = null;
        public Func<IEnumerable<Company>, IEnumerable<Company>>? PostProcessingAction { get; set; }
        public SqlLikeSearchCriteriaBuilder<Company>? SqlLikeSearchCriteriaBuilder { get; set; } = null;
        public EntityFilterBuilder<Company>? EntityFilterBuilder { get; set; } = null;
        public void Clear() { }
        public bool IsSatisfiedBy(Company entity) => true;
        public void SetPage(int page, int pageSize) { }
    }
}
