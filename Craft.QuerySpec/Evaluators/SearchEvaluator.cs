namespace Craft.QuerySpec;

public sealed class SearchEvaluator : IEvaluator
{
    private SearchEvaluator() { }

    public static SearchEvaluator Instance { get; } = new SearchEvaluator();

    public IQueryable<T>? GetQuery<T>(IQueryable<T>? queryable, IQuery<T>? query) where T : class
    {
        return queryable?.Search(query?.SqlLikeSearchCriteriaBuilder?.SqlLikeSearchCriteriaList ?? []);
    }
}
