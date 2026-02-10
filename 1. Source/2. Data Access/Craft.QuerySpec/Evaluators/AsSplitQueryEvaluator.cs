using Craft.Core;
using Microsoft.EntityFrameworkCore;

namespace Craft.QuerySpec;

public sealed class AsSplitQueryEvaluator : IEvaluator
{
    public static AsSplitQueryEvaluator Instance { get; } = new AsSplitQueryEvaluator();

    private AsSplitQueryEvaluator()
    { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);

        if (query?.AsSplitQuery == true)
            queryable = queryable.AsSplitQuery();

        return queryable;
    }
}

