using Craft.Core;

namespace Craft.QuerySpec;

public sealed class SearchEvaluator : IContextualEvaluator
{
    private SearchEvaluator() { }

    public static SearchEvaluator Instance { get; } = new SearchEvaluator();

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);

        return queryable.Search(query?
            .SqlLikeSearchCriteriaBuilder?
            .SqlLikeSearchCriteriaList ?? [])!;
    }

    public IQueryable<T> GetQuery<T>(
        IQueryable<T> queryable,
        IQuery<T>? query,
        QueryEvaluationContext context)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);
        ArgumentNullException.ThrowIfNull(context);

        return queryable.Search(
            query?.SqlLikeSearchCriteriaBuilder?.SqlLikeSearchCriteriaList ?? [],
            context.RootEntityType)!;
    }
}
