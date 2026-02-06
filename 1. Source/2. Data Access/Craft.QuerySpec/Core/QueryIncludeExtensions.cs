using System.Linq.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// Extension methods for adding Include and ThenInclude functionality to queries.
/// </summary>
public static class QueryIncludeExtensions
{
    /// <summary>
    /// Specifies a related entity to include in the query results.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TProperty">The type of the navigation property.</typeparam>
    /// <param name="query">The query to add the include to.</param>
    /// <param name="navigationPropertyPath">The lambda expression representing the navigation property.</param>
    /// <returns>The query with the include added.</returns>
    public static IQuery<T> Include<T, TProperty>(
        this IQuery<T> query,
        Expression<Func<T, TProperty>> navigationPropertyPath)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(navigationPropertyPath, nameof(navigationPropertyPath));

        query.IncludeExpressions ??= [];

        var includeExpression = new IncludeExpression(
            navigationPropertyPath,
            typeof(T),
            typeof(TProperty),
            isThenInclude: false,
            previousInclude: null);

        query.IncludeExpressions.Add(includeExpression);

        return query;
    }

    /// <summary>
    /// Specifies additional related entities to include in the query results after a previous Include.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TPreviousProperty">The type of the previously included navigation property.</typeparam>
    /// <typeparam name="TProperty">The type of the navigation property to include.</typeparam>
    /// <param name="query">The query to add the include to.</param>
    /// <param name="navigationPropertyPath">The lambda expression representing the navigation property.</param>
    /// <returns>The query with the include added.</returns>
    public static IQuery<T> ThenInclude<T, TPreviousProperty, TProperty>(
        this IQuery<T> query,
        Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(navigationPropertyPath, nameof(navigationPropertyPath));

        if (query.IncludeExpressions is null || query.IncludeExpressions.Count == 0)
            throw new InvalidOperationException("ThenInclude can only be called after Include.");

        // Get the last include expression as the previous one
        var previousInclude = query.IncludeExpressions[^1];

        var includeExpression = new IncludeExpression(
            navigationPropertyPath,
            typeof(TPreviousProperty),
            typeof(TProperty),
            isThenInclude: true,
            previousInclude: previousInclude);

        query.IncludeExpressions.Add(includeExpression);

        return query;
    }

    /// <summary>
    /// Specifies additional related entities (collection) to include in the query results after a previous Include.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TPreviousProperty">The type of the previously included navigation property.</typeparam>
    /// <typeparam name="TProperty">The type of the navigation property to include.</typeparam>
    /// <param name="query">The query to add the include to.</param>
    /// <param name="navigationPropertyPath">The lambda expression representing the navigation property.</param>
    /// <returns>The query with the include added.</returns>
    public static IQuery<T> ThenInclude<T, TPreviousProperty, TProperty>(
        this IQuery<T> query,
        Expression<Func<TPreviousProperty, IEnumerable<TProperty>>> navigationPropertyPath)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(navigationPropertyPath, nameof(navigationPropertyPath));

        if (query.IncludeExpressions is null || query.IncludeExpressions.Count == 0)
            throw new InvalidOperationException("ThenInclude can only be called after Include.");

        // Get the last include expression as the previous one
        var previousInclude = query.IncludeExpressions[^1];

        var includeExpression = new IncludeExpression(
            navigationPropertyPath,
            typeof(TPreviousProperty),
            typeof(IEnumerable<TProperty>),
            isThenInclude: true,
            previousInclude: previousInclude);

        query.IncludeExpressions.Add(includeExpression);

        return query;
    }
}
