using Microsoft.EntityFrameworkCore;

namespace Craft.QuerySpec;

public static class DbSetExtensions
{
    public static async Task<IEnumerable<TSource>> ToEnumerableAsync<TSource>(this DbSet<TSource> source,
         IQuery<TSource>? query, CancellationToken cancellationToken = default) where TSource : class
    {
        var result = await QueryEvaluator.Instance
            .GetQuery(source, query)
            .ToListAsync(cancellationToken) ?? [];

        return query?.PostProcessingAction == null
            ? result
            : query.PostProcessingAction(result);
    }

    public static async Task<List<TSource>> ToListAsync<TSource>(this DbSet<TSource> source,
         IQuery<TSource> query, CancellationToken cancellationToken = default) where TSource : class
    {
        var result = await QueryEvaluator.Instance
            .GetQuery(source, query)
            .ToListAsync(cancellationToken) ?? [];

        return query.PostProcessingAction == null
            ? result
            : [.. query.PostProcessingAction(result)];
    }

    public static IQueryable<TSource> WithQuery<TSource>(this IQueryable<TSource> source,
          IQuery<TSource>? query, IEvaluator? evaluator = null) where TSource : class
    {
        evaluator ??= QueryEvaluator.Instance;

        return evaluator.GetQuery(source, query) ?? Enumerable.Empty<TSource>().AsQueryable();
    }

    public static IQueryable<TResult> WithQuery<TSource, TResult>(this IQueryable<TSource> source,
        IQuery<TSource, TResult>? query, ISelectEvaluator? evaluator = null)
        where TSource : class
        where TResult : class
    {
        evaluator ??= QueryEvaluator.Instance;

        return evaluator.GetQuery(source, query) ?? Enumerable.Empty<TResult>().AsQueryable();
    }
}
