using Craft.Core;
using Craft.Extensions.Collections;
using Microsoft.EntityFrameworkCore;

namespace Craft.QuerySpec;

public static class DbSetExtensions
{
    public static async Task<IEnumerable<TSource>> ToEnumerableAsync<TSource>(this DbSet<TSource> source,
         IQuery<TSource>? query, CancellationToken cancellationToken = default) where TSource : class
    {
        return await ToListAsync(source, query, cancellationToken);
    }

    public static async Task<List<TSource>> ToListAsync<TSource>(this DbSet<TSource> source,
         IQuery<TSource>? query, CancellationToken cancellationToken = default) where TSource : class
    {
        var queryable = source.WithQuery(query);

        var result = await queryable.ToListSafeAsync(cancellationToken);

        return result.Count == 0
            ? []
            : query?.PostProcessingAction == null
            ? result
            : [.. query.PostProcessingAction(result)];
    }

    /// <summary>
    /// Applies a query specification to a DbSet and makes its EF Core entity metadata available
    /// to contextual evaluators such as <see cref="SearchEvaluator"/>.
    /// </summary>
    public static IQueryable<TSource> WithQuery<TSource>(this DbSet<TSource> source,
        IQuery<TSource>? query, IEvaluator? evaluator = null) where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);

        evaluator ??= QueryEvaluator.Instance;
        var context = new QueryEvaluationContext(source.EntityType);

        return evaluator switch
        {
            QueryEvaluator queryEvaluator => queryEvaluator.GetQuery(source, query, context),
            IContextualEvaluator contextualEvaluator => contextualEvaluator.GetQuery(source, query, context),
            _ => evaluator.GetQuery(source, query)
        };
    }

    public static IQueryable<TSource> WithQuery<TSource>(this IQueryable<TSource> source,
          IQuery<TSource>? query, IEvaluator? evaluator = null) where TSource : class
    {
        evaluator ??= QueryEvaluator.Instance;

        return evaluator.GetQuery(source, query);
    }

    /// <summary>
    /// Applies a projected query specification to a DbSet while retaining EF Core entity metadata.
    /// </summary>
    public static IQueryable<TResult> WithQuery<TSource, TResult>(this DbSet<TSource> source,
        IQuery<TSource, TResult>? query, ISelectEvaluator? evaluator = null)
        where TSource : class
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(source);

        evaluator ??= QueryEvaluator.Instance;

        return evaluator is QueryEvaluator queryEvaluator
            ? queryEvaluator.GetQuery(source, query, new QueryEvaluationContext(source.EntityType))
            : evaluator.GetQuery(source, query) ?? Enumerable.Empty<TResult>().AsQueryable();
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
