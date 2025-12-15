using System.Linq.Expressions;
using System.Text;

namespace Craft.QuerySpec;

/// <summary>
/// Represents a query with result projection.
/// </summary>
/// <remarks>
/// This class does not require disposal. All resources are managed by the garbage collector.
/// </remarks>
[Serializable]
public class Query<T, TResult> : Query<T>, IQuery<T, TResult>
    where T : class
    where TResult : class
{
    public QuerySelectBuilder<T, TResult>? QuerySelectBuilder { get; internal set; } = new();

    public Expression<Func<T, IEnumerable<TResult>>>? SelectorMany { get; set; }

    public new void Clear()
    {
        base.Clear();
        QuerySelectBuilder?.Clear();
        SelectorMany = null;
    }

    /// <summary>
    /// Optional post-processing function applied to results after database retrieval.
    /// </summary>
    /// <remarks>
    /// WARNING: This function executes in-memory after database query completion.
    /// Be cautious with large result sets and avoid capturing large objects in closures.
    /// </remarks>
    public new Func<IEnumerable<TResult>, IEnumerable<TResult>>? PostProcessingAction { get; set; }
}

/// <summary>
/// Query specification for entity retrieval.
/// </summary>
/// <remarks>
/// This class does not require disposal. All resources are managed by the garbage collector.
/// This class is NOT thread-safe. Do not share instances across threads.
/// </remarks>
[Serializable]
public class Query<T> : IQuery<T> where T : class
{
    private int? _skip;
    private int? _take;

    public bool AsNoTracking { get; set; } = true;
    public bool AsSplitQuery { get; set; }
    public bool IgnoreAutoIncludes { get; set; } = true;
    public bool IgnoreQueryFilters { get; set; }

    public int? Skip
    {
        get => _skip;
        set
        {
            if (value is < 0)
                throw new ArgumentOutOfRangeException(nameof(Skip), value, "Skip cannot be negative.");
            _skip = value;
        }
    }

    public int? Take
    {
        get => _take;
        set
        {
            if (value is < 1 and not null)
                throw new ArgumentOutOfRangeException(nameof(Take), value, "Take must be greater than zero.");
            _take = value;
        }
    }

    public SortOrderBuilder<T>? SortOrderBuilder { get; set; } = new();
    public SqlLikeSearchCriteriaBuilder<T>? SqlLikeSearchCriteriaBuilder { get; set; } = new();
    public EntityFilterBuilder<T>? EntityFilterBuilder { get; set; } = new();

    /// <summary>
    /// Optional post-processing function applied to results after database retrieval.
    /// </summary>
    /// <remarks>
    /// WARNING: This function executes in-memory after database query completion.
    /// Be cautious with large result sets and avoid capturing large objects in closures.
    /// </remarks>
    public Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; set; }

    /// <summary>
    /// Checks if the entity satisfies the query's filter criteria.
    /// </summary>
    /// <remarks>
    /// This method only evaluates WHERE clauses for performance. 
    /// Ordering, pagination, and other query features are not considered.
    /// </remarks>
    public virtual bool IsSatisfiedBy(T entity)
    {
        if (entity is null) return false;

        var queryable = new List<T> { entity }.AsQueryable();
        queryable = WhereEvaluator.Instance.GetQuery(queryable, this) ?? queryable;

        return queryable.Any();
    }

    /// <summary>
    /// Sets pagination specifications.
    /// </summary>
    /// <param name="page">Page number (1-based). Must be 1 or greater.</param>
    /// <param name="pageSize">Number of items per page. Must be 1 or greater.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when page or pageSize is less than 1.</exception>
    public virtual void SetPage(int page = PaginationConstant.DefaultPage, int pageSize = PaginationConstant.DefaultPageSize)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), page, "Page number must be 1 or greater.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be 1 or greater.");

        Take = pageSize;
        Skip = (page - 1) * pageSize;
    }

    /// <summary>
    /// Clears all query specifications and resets to defaults.
    /// </summary>
    public void Clear()
    {
        _skip = null;
        _take = null;

        AsNoTracking = true;
        AsSplitQuery = false;
        IgnoreAutoIncludes = true;
        IgnoreQueryFilters = false;

        SortOrderBuilder?.Clear();
        SqlLikeSearchCriteriaBuilder?.Clear();
        EntityFilterBuilder?.Clear();
    }

    /// <summary>
    /// Returns a string representation of the query for debugging purposes.
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Query<{typeof(T).Name}>");
        sb.AppendLine($"  AsNoTracking: {AsNoTracking}");
        sb.AppendLine($"  AsSplitQuery: {AsSplitQuery}");
        sb.AppendLine($"  IgnoreAutoIncludes: {IgnoreAutoIncludes}");
        sb.AppendLine($"  IgnoreQueryFilters: {IgnoreQueryFilters}");
        sb.AppendLine($"  Skip: {Skip?.ToString() ?? "null"}, Take: {Take?.ToString() ?? "null"}");

        if (EntityFilterBuilder?.Count > 0)
            sb.AppendLine($"  Filters: {EntityFilterBuilder.Count}");

        if (SortOrderBuilder?.Count > 0)
            sb.AppendLine($"  Orders: {SortOrderBuilder}");

        if (SqlLikeSearchCriteriaBuilder?.Count > 0)
            sb.AppendLine($"  Search Criteria: {SqlLikeSearchCriteriaBuilder.Count}");

        return sb.ToString();
    }
}
