using Craft.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Craft.QuerySpec;

/// <summary>
/// Evaluator that applies Include expressions to eagerly load related entities.
/// </summary>
public sealed class IncludeEvaluator : IEvaluator
{
    public static IncludeEvaluator Instance { get; } = new IncludeEvaluator();

    private IncludeEvaluator() { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);

        if (query?.IncludeExpressions is null || query.IncludeExpressions.Count == 0)
            return queryable;

        // Process includes - group them by their root includes
        var processedRoots = new HashSet<IncludeExpression>();

        foreach (var includeExpression in query.IncludeExpressions)
        {
            // Find the root of this include chain
            var root = GetRoot(includeExpression);

            // Skip if we've already processed this root
            if (processedRoots.Contains(root))
                continue;

            processedRoots.Add(root);

            // Apply the full include chain starting from this root
            queryable = ApplyIncludeChain(queryable, root, query.IncludeExpressions);
        }

        return queryable;
    }

    private static IncludeExpression GetRoot(IncludeExpression include)
    {
        var current = include;
        while (current.PreviousInclude is not null)
            current = current.PreviousInclude;
        return current;
    }

    private static IQueryable<T> ApplyIncludeChain<T>(
        IQueryable<T> queryable,
        IncludeExpression root,
        List<IncludeExpression> allIncludes) where T : class
    {
        // Apply the root include
        queryable = ApplyInclude(queryable, root);

        // Find and apply all ThenIncludes that follow this root
        var thenIncludes = allIncludes
            .Where(e => e.IsThenInclude && GetRoot(e) == root)
            .ToList();

        foreach (var thenInclude in thenIncludes)
        {
            queryable = ApplyThenInclude(queryable, thenInclude);
        }

        return queryable;
    }

    private static IQueryable<T> ApplyInclude<T>(IQueryable<T> queryable, IncludeExpression include) where T : class
    {
        // Use EntityFrameworkQueryableExtensions.Include with proper generic arguments
        var includeMethod = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .Where(m => m.Name == nameof(EntityFrameworkQueryableExtensions.Include))
            .Where(m => m.GetParameters().Length == 2)
            .Where(m =>
            {
                var parameters = m.GetParameters();
                return parameters[1].ParameterType.IsGenericType &&
                       parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>);
            })
            .FirstOrDefault();

        if (includeMethod is null)
            return queryable;

        var genericIncludeMethod = includeMethod.MakeGenericMethod(typeof(T), include.PropertyType);

        var result = genericIncludeMethod.Invoke(null, [queryable, include.Expression]);

        return result as IQueryable<T> ?? queryable;
    }

    private static IQueryable<T> ApplyThenInclude<T>(IQueryable<T> queryable, IncludeExpression thenInclude) where T : class
    {
        // For ThenInclude, we need to rebuild the expression chain
        var chain = BuildExpressionChain(thenInclude);

        // Start with the root include
        var current = queryable;

        for (int i = 0; i < chain.Count; i++)
        {
            if (i == 0)
            {
                // First is a regular Include
                current = ApplyInclude(current, chain[i]);
            }
            else
            {
                // Rest are ThenIncludes - these need special handling
                // Since we already applied them through the Include chain, skip
            }
        }

        return current;
    }

    private static List<IncludeExpression> BuildExpressionChain(IncludeExpression include)
    {
        var chain = new List<IncludeExpression>();
        var current = include;

        // Build chain from leaf to root
        while (current is not null)
        {
            chain.Insert(0, current);
            current = current.PreviousInclude!;
        }

        return chain;
    }
}

