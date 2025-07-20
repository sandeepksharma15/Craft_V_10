namespace Craft.QuerySpec;

public sealed class PaginationEvaluator : IEvaluator
{
    public static PaginationEvaluator Instance { get; } = new PaginationEvaluator();

    private PaginationEvaluator() { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);

        if (query?.Skip is not null && (query.Skip.Value > 0))
            queryable = queryable.Skip(query.Skip.Value);

        if (query?.Take is not null && (query.Take.Value > 0))
            queryable = queryable.Take(query.Take.Value);

        return queryable;
    }
}
