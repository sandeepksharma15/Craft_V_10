using Craft.QuerySpec.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Craft.QuerySpec.Evaluators;

public sealed class IgnoreAutoIncludeEvaluator : IEvaluator
{
    public static IgnoreAutoIncludeEvaluator Instance { get; } = new IgnoreAutoIncludeEvaluator();

    private IgnoreAutoIncludeEvaluator() { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T> query) where T : class
    {
        if (query.IgnoreAutoIncludes)
            queryable = queryable.IgnoreAutoIncludes();

        return queryable;
    }
}
