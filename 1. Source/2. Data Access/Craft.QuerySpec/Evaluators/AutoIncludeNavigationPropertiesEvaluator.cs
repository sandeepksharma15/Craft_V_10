using Craft.Core;
namespace Craft.QuerySpec;

/// <summary>
/// Evaluator that automatically includes all navigation properties when AutoIncludeNavigationProperties is true.
/// Uses reflection to discover navigation properties and applies them to the query (1 level deep only).
/// </summary>
public sealed class AutoIncludeNavigationPropertiesEvaluator : IEvaluator
{
    public static AutoIncludeNavigationPropertiesEvaluator Instance { get; } = new AutoIncludeNavigationPropertiesEvaluator();

    private AutoIncludeNavigationPropertiesEvaluator() { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);

        // Only process if AutoIncludeNavigationProperties is enabled
        if (query?.AutoIncludeNavigationProperties != true)
            return queryable;

        // Discover all navigation properties for the entity type
        var navigationProperties = NavigationPropertyDiscovery.DiscoverNavigationProperties<T>();

        if (navigationProperties.Count == 0)
            return queryable;

        // Ensure IncludeExpressions list exists
        query.IncludeExpressions ??= [];

        // Create and add Include expressions for each discovered navigation property
        // Only add if not already explicitly included by the user
        foreach (var navProperty in navigationProperties)
        {
            // Check if this property is already included
            var alreadyIncluded = query.IncludeExpressions.Any(e =>
                !e.IsThenInclude &&
                e.EntityType == typeof(T) &&
                GetPropertyName(e.Expression) == navProperty.Name);

            if (!alreadyIncluded)
            {
                var includeExpression = NavigationPropertyDiscovery.CreateIncludeExpression<T>(navProperty);
                query.IncludeExpressions.Add(includeExpression);
            }
        }

        return queryable;
    }

    private static string GetPropertyName(System.Linq.Expressions.LambdaExpression expression)
    {
        if (expression.Body is System.Linq.Expressions.MemberExpression memberExpression)
            return memberExpression.Member.Name;

        return string.Empty;
    }
}

