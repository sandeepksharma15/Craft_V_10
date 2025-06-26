namespace Craft.QuerySpec;

public sealed class PaginationEvaluator : IEvaluator
{
    public static PaginationEvaluator Instance { get; } = new PaginationEvaluator();

    private PaginationEvaluator() { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class
    {
        if (query?.Skip is not null and not 0)
            queryable = queryable.Skip(query.Skip.Value);

        if (query?.Take is not null)
            queryable = queryable.Take(query.Take.Value);

        return queryable;
    }
}
