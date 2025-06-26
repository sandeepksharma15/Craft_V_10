using Microsoft.EntityFrameworkCore;

namespace Craft.QuerySpec;

public sealed class AsNoTrackingEvaluator : IEvaluator
{
    public static AsNoTrackingEvaluator Instance { get; } = new AsNoTrackingEvaluator();

    private AsNoTrackingEvaluator()
    { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class
    {
        if (query?.AsNoTracking == true)
            queryable = queryable.AsNoTracking();

        return queryable;
    }
}
