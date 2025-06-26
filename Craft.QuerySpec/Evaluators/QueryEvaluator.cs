using Craft.QuerySpec.Contracts;

namespace Craft.QuerySpec.Evaluators;

public class QueryEvaluator : IEvaluator, ISelectEvaluator
{
    protected List<IEvaluator> Evaluators { get; } = [];

    public static QueryEvaluator Instance { get; } = new QueryEvaluator();

    public QueryEvaluator()
        => Evaluators.AddRange([
            WhereEvaluator.Instance,
            OrderEvaluator.Instance,
            SearchEvaluator.Instance,
            PaginationEvaluator.Instance,
            AsNoTrackingEvaluator.Instance,
            AsSplitQueryEvaluator.Instance,
            IgnoreAutoIncludeEvaluator.Instance,
            IgnoreQueryFiltersEvaluator.Instance,
        ]);

    public QueryEvaluator(IEnumerable<IEvaluator> evaluators)
        => Evaluators.AddRange(evaluators);

    public virtual IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T> query)
        where T : class
    {
        if (query is null)
            return queryable;

        foreach (var evaluator in Evaluators)
            queryable = evaluator.GetQuery(queryable, query);

        return queryable;
    }

    public IQueryable<TResult> GetQuery<T, TResult>(IQueryable<T> queryable, IQuery<T, TResult> query)
        where T : class
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.QuerySelectBuilder.Count == 0 && query.SelectorMany is null)
            throw new InvalidOperationException("No Selection defined in query");

        if (query.QuerySelectBuilder.Count > 0 && query.SelectorMany is not null)
            throw new InvalidOperationException("Cannot define both Select and SelectMany in query");

        queryable = GetQuery(queryable, (IQuery<T>)query);

        return query.SelectorMany is null
            ? queryable.Select(query.QuerySelectBuilder.Build())
            : queryable.SelectMany(query.SelectorMany);
    }
}
