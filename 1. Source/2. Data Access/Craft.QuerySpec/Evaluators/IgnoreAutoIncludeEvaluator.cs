using Craft.Core;
using Microsoft.EntityFrameworkCore;

namespace Craft.QuerySpec;

public sealed class IgnoreAutoIncludeEvaluator : IEvaluator
{
    public static IgnoreAutoIncludeEvaluator Instance { get; } = new IgnoreAutoIncludeEvaluator();

    private IgnoreAutoIncludeEvaluator() { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);

        if (query?.IgnoreAutoIncludes == true)
            queryable = queryable.IgnoreAutoIncludes();

        return queryable;
    }
}

