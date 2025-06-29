using Microsoft.EntityFrameworkCore;

namespace Craft.QuerySpec;

public sealed class IgnoreQueryFiltersEvaluator : IEvaluator
{
    public static IgnoreQueryFiltersEvaluator Instance { get; } = new IgnoreQueryFiltersEvaluator();

    private IgnoreQueryFiltersEvaluator()
    { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);

        if (query?.IgnoreQueryFilters == true)
            queryable = queryable.IgnoreQueryFilters();

        return queryable;
    }
}
