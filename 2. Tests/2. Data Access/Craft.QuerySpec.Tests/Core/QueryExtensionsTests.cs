using Craft.Testing.Fixtures;

namespace Craft.QuerySpec.Tests.Core;

public class QueryExtensionsTests
{
    [Fact]
    public void IgnoreQueryFilters_WhenQueryIsNull_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.IgnoreQueryFilters();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void IgnoreQueryFilters_WhenCalled_SetsIgnoreQueryFiltersTrue()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        var result = query.IgnoreQueryFilters();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IgnoreQueryFilters);
    }

    [Fact]
    public void IgnoreAutoIncludes_WhenQueryIsNull_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.IgnoreAutoIncludes();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void IgnoreAutoIncludes_WhenCalled_SetsIgnoreAutoIncludesTrue()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        var result = query.IgnoreAutoIncludes();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IgnoreAutoIncludes);
    }

    [Fact]
    public void AsSplitQuery_WhenQueryIsNull_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.AsSplitQuery();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void AsSplitQuery_WhenCalled_SetsAsSplitQueryTrue()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        var result = query.AsSplitQuery();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.AsSplitQuery);
    }

    [Fact]
    public void AsNoTracking_WhenQueryIsNull_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.AsNoTracking();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void AsNoTracking_WhenCalled_SetsAsNoTrackingTrue()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        var result = query.AsNoTracking();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.AsNoTracking);
    }

    [Fact]
    public void Skip_WhenQueryIsNull_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.Skip(5);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Skip_WhenCalled_SetsSkip()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        var result = query.Skip(5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Skip);
    }

    [Fact]
    public void Skip_WhenSetToNull_SetsSkipToNull()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        var result = query.Skip(null);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Skip);
    }

    [Fact]
    public void Take_WhenQueryIsNull_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.Take(5);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Take_WhenCalled_SetsTake()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        var result = query.Take(5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Take);
    }

    [Fact]
    public void Take_WhenSetToNull_SetsTakeToNull()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        var result = query.Take(null);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Take);
    }

    [Fact]
    public void SetsNothing_GivenNoPostProcessingAction()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        Assert.Null(query.PostProcessingAction);
    }

    [Fact]
    public void SetPostProcessingAction_SetsAction_NotNullQuery()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        var result = query.SetPostProcessingAction(x => x);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.PostProcessingAction);
    }

    [Fact]
    public void SetPostProcessingAction_NullQuery_ReturnsNull()
    {
        // Arrange
        IQuery<Company> query = null!;

        // Act
        var result = query.SetPostProcessingAction(x => x);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SetsNothing_TResult_GivenNoPostProcessingAction()
    {
        // Arrange
        IQuery<Company, Company> query = new Query<Company, Company>();

        // Act
        Assert.Null(query.PostProcessingAction);
    }

    [Fact]
    public void SetPostProcessingAction_TResult_SetsAction_NotNullQuery()
    {
        // Arrange
        IQuery<Company, Company> query = new Query<Company, Company>();

        // Act
        var result = query.SetPostProcessingAction(x => x);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.PostProcessingAction);
    }

    [Fact]
    public void SetPostProcessingAction_TResult_NullQuery_ReturnsNull()
    {
        // Arrange
        IQuery<Company, Company> query = null!;

        // Act
        var result = query.SetPostProcessingAction(x => x);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void IsWithoutOrder_ReturnsTrue_NoSortOrderBuilder()
    {
        // Arrange
        // Simulate null SortOrderBuilder by creating a stub implementation
        var stub = new QueryStubWithoutSortOrderBuilder();

        var result = QueryExtensions.IsWithoutOrder(stub);

        Assert.True(result);
    }

    [Fact]
    public void IsWithoutOrder_ReturnsTrue_EmptyOrderDescriptorList()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        query.SortOrderBuilder?.OrderDescriptorList.Clear();
        var result = query.IsWithoutOrder();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsWithoutOrder_ReturnsFalse_NonEmptyOrderDescriptorList()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        query.OrderBy(x => x.Id);
        var result = query.IsWithoutOrder();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsWithoutOrder_ReturnsTrue_NullQuery()
    {
        // Arrange
        var result = ((IQuery<string>)null!).IsWithoutOrder();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Chaining_MultipleExtensions_SetsAllProperties()
    {
        // Arrange
        IQuery<Company> query = new Query<Company>();

        // Act
        var result = query.AsNoTracking()?
            .AsSplitQuery()?
            .IgnoreAutoIncludes()?
            .IgnoreQueryFilters()?
            .Skip(2)?
            .Take(3);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.AsNoTracking);
        Assert.True(result.AsSplitQuery);
        Assert.True(result.IgnoreAutoIncludes);
        Assert.True(result.IgnoreQueryFilters);
        Assert.Equal(2, result.Skip);
        Assert.Equal(3, result.Take);
    }

    private class QueryStubWithoutSortOrderBuilder : IQuery<Company>
    {
        public bool AsNoTracking { get; set; }
        public bool AsSplitQuery { get; set; }
        public bool IgnoreAutoIncludes { get; set; }
        public bool IgnoreQueryFilters { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public SortOrderBuilder<Company>? SortOrderBuilder { get; set; } = null;
        public Func<IEnumerable<Company>, IEnumerable<Company>>? PostProcessingAction { get; set; }
        public SqlLikeSearchCriteriaBuilder<Company>? SqlLikeSearchCriteriaBuilder { get; set; } = new SqlLikeSearchCriteriaBuilder<Company>();
        public EntityFilterBuilder<Company>? EntityFilterBuilder { get; set; } = new EntityFilterBuilder<Company>();
        public List<IncludeExpression>? IncludeExpressions { get; set; }
        public void Clear() { }
        public bool IsSatisfiedBy(Company entity) => true;
        public void SetPage(int page, int pageSize) { }
    }
}
