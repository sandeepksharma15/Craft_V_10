using System.Linq.Expressions;

namespace Craft.QuerySpec;

public static class QueryWhereExtensions
{
    public static IQuery<T>? Where<T>(this IQuery<T> query, Expression<Func<T, bool>> expression) where T : class
    {
        if (query is null || expression is null) return query;

        query?.EntityFilterBuilder?.Add(expression);

        return query;
    }

    public static IQuery<T>?  Where<T>(this IQuery<T> query, Expression<Func<T, object>> propExpr,
        object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo) where T : class
    {
        if (query is null || propExpr is null) return query;

        query?.EntityFilterBuilder?.Add(propExpr, compareWith, comparisonType);

        return query;
    }

    public static IQuery<T>? Where<T>(this IQuery<T> query, string propName,
        object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo) where T : class
    {
        if (query is null || propName is null) return query;

        query?.EntityFilterBuilder?.Add(propName, compareWith, comparisonType);

        return query;
    }
}
