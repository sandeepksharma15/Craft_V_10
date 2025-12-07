using System.Linq.Expressions;

namespace Craft.QuerySpec;

// Represents a query with result projection.
[Serializable]
public class Query<T, TResult> : Query<T>, IQuery<T, TResult>
    where T : class
    where TResult : class
{
    // QuerySelectBuilder for constructing select expressions.
    public QuerySelectBuilder<T, TResult>? QuerySelectBuilder { get; internal set; } = new();

    // Expression for selecting many results.
    public Expression<Func<T, IEnumerable<TResult>>>? SelectorMany { get; set; }

    // Clears the query specifications including select expressions and selector for many results.
    public new void Clear()
    {
        base.Clear();
        QuerySelectBuilder?.Clear();
        SelectorMany = null;
    }

    // Function for post-processing results.
    public new Func<IEnumerable<TResult>, IEnumerable<TResult>>? PostProcessingAction { get; set; }
}

[Serializable]
public class Query<T> : IQuery<T> where T : class
{
    // Common query specifications.
    public bool AsNoTracking { get; set; } = true;
    public bool AsSplitQuery { get; set; }
    public bool IgnoreAutoIncludes { get; set; } = true;
    public bool IgnoreQueryFilters { get; set; }

    // Pagination specifications.
    public int? Skip
    {
        get => field;
        set
        {
            if (value is < 0)
                throw new ArgumentOutOfRangeException(nameof(Skip), "Skip cannot be negative.");
            field = value;
        }
    }

    public int? Take
    {
        get => field;
        set
        {
            if (value is <= 0)
                throw new ArgumentOutOfRangeException(nameof(Take), "Take must be greater than zero.");
            field = value;
        }
    }

    // Builders for building where and order expressions.
    public SortOrderBuilder<T>? SortOrderBuilder { get; set; } = new();
    public SqlLikeSearchCriteriaBuilder<T>? SqlLikeSearchCriteriaBuilder { get; set; } = new();
    public EntityFilterBuilder<T>? EntityFilterBuilder { get; set; } = new();

    // Function for post-processing results.
    public Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; set; }

    // Checks if the entity satisfies the query specifications.
    public virtual bool IsSatisfiedBy(T entity)
    {
        if (entity is null) return false;

        // Create a queryable from the entity
        var queryable = new List<T> { entity }.AsQueryable();

        queryable = QueryEvaluator.Instance.GetQuery(queryable, this);

        return queryable?.Any() == true;
    }

    // Sets pagination specifications.
    public virtual void SetPage(int page = PaginationConstant.DefaultPage, int pageSize = PaginationConstant.DefaultPageSize)
    {
        pageSize = pageSize > 0 ? pageSize : PaginationConstant.DefaultPageSize;
        page = Math.Max(page, PaginationConstant.DefaultPage);
        Take = pageSize;
        Skip = (page - 1) * pageSize;
    }

    // Clears all query specifications.
    public void Clear()
    {
        // Reset pagination specifications.
        SetPage();

        // Reset common query specifications.
        AsNoTracking = true;
        AsSplitQuery = false;
        IgnoreAutoIncludes = true;
        IgnoreQueryFilters = false;

        // Clear Builders
        SortOrderBuilder?.Clear();
        SqlLikeSearchCriteriaBuilder?.Clear();
        EntityFilterBuilder?.Clear();
    }
}
