namespace Craft.QuerySpec;

/// <summary>
/// Evaluates and applies query specifications such as filtering, ordering, searching, pagination, and selection.
/// </summary>
public class QueryEvaluator : IEvaluator, ISelectEvaluator
{
    /// <summary>
    /// List of evaluators to apply in sequence.
    /// </summary>
    protected List<IEvaluator> Evaluators { get; } = [];

    /// <summary>
    /// Singleton instance for convenience.
    /// </summary>
    public static QueryEvaluator Instance { get; } = new QueryEvaluator();

    /// <summary>
    /// Initializes a new instance with default evaluators.
    /// </summary>
    public QueryEvaluator()
        => Evaluators.AddRange([
            WhereEvaluator.Instance,
            OrderEvaluator.Instance,
            SearchEvaluator.Instance,
            PaginationEvaluator.Instance,
            AutoIncludeNavigationPropertiesEvaluator.Instance,
            IncludeEvaluator.Instance,
            AsNoTrackingEvaluator.Instance,
            AsSplitQueryEvaluator.Instance,
            IgnoreAutoIncludeEvaluator.Instance,
            IgnoreQueryFiltersEvaluator.Instance,
        ]);

    /// <summary>
    /// Initializes a new instance with custom evaluators.
    /// </summary>
    public QueryEvaluator(IEnumerable<IEvaluator> evaluators)
        => Evaluators.AddRange(evaluators);

    /// <summary>
    /// Applies all evaluators to the queryable based on the provided query specification.
    /// </summary>
    public virtual IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query)
        where T : class
    {
        if (query is null)
            return queryable;

        foreach (var evaluator in Evaluators)
            queryable = evaluator.GetQuery(queryable, query) ?? queryable;

        return queryable;
    }

    /// <summary>
    /// Applies all evaluators and selection logic to the queryable based on the provided query specification.
    /// </summary>
    public IQueryable<TResult> GetQuery<T, TResult>(IQueryable<T> queryable, IQuery<T, TResult>? query)
        where T : class
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        var hasSelect = query.QuerySelectBuilder?.Count > 0;
        var hasSelectMany = query.SelectorMany is not null;

        if (!hasSelect && !hasSelectMany)
            throw new InvalidOperationException("No Selection defined in query");

        if (hasSelect && hasSelectMany)
            throw new InvalidOperationException("Cannot define both Select and SelectMany in query");

        // Apply all evaluators except selection
        var filtered = GetQuery(queryable, (IQuery<T>)query) ?? queryable;

        if (hasSelect)
        {
            var selector = query.QuerySelectBuilder?.Build();

            return selector is null
                ? throw new InvalidOperationException("QuerySelectBuilder is not defined")
                : filtered.Select(selector);
        }
        
        if (hasSelectMany)
            return filtered.SelectMany(query.SelectorMany!);

        throw new InvalidOperationException(
            "Internal error: No selection strategy determined. This indicates a bug in QueryEvaluator.");
    }
}
