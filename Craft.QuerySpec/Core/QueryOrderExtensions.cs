using System.Linq.Expressions;
using Craft.QuerySpec.Contracts;
using Craft.QuerySpec.Enums;

namespace Craft.QuerySpec.Core;

public static class QueryOrderExtensions
{
    public static IQuery<T> OrderBy<T>(this IQuery<T> query, Expression<Func<T, object>> propExpr) where T : class
    {
        if (query is null || propExpr is null) return query;

        query.SortOrderBuilder.Add(propExpr);

        return query;
    }

    public static IQuery<T> OrderByDescending<T>(this IQuery<T> query, Expression<Func<T, object>> propExpr) where T : class
    {
        if (query is null || propExpr is null) return query;

        query.SortOrderBuilder.Add(propExpr, OrderTypeEnum.OrderByDescending);

        return query;
    }

    public static IQuery<T> ThenBy<T>(this IQuery<T> query, Expression<Func<T, object>> propExpr) where T : class
    {
        if (query is null || propExpr is null) return query;

        query.SortOrderBuilder.Add(propExpr, OrderTypeEnum.ThenBy);

        return query;
    }

    public static IQuery<T> ThenByDescending<T>(this IQuery<T> query, Expression<Func<T, object>> propExpr) where T : class
    {
        if (query is null || propExpr is null) return query;

        query.SortOrderBuilder.Add(propExpr, OrderTypeEnum.ThenByDescending);

        return query;
    }

    public static IQuery<T> OrderBy<T>(this IQuery<T> query, string propName) where T : class
    {
        if (query is null || propName is null) return query;

        query.SortOrderBuilder.Add(propName);

        return query;
    }

    public static IQuery<T> OrderByDescending<T>(this IQuery<T> query, string propName) where T : class
    {
        if (query is null || propName is null) return query;

        query.SortOrderBuilder.Add(propName, OrderTypeEnum.OrderByDescending);

        return query;
    }

    public static IQuery<T> ThenBy<T>(this IQuery<T> query, string propName) where T : class
    {
        if (query is null || propName is null) return query;

        query.SortOrderBuilder.Add(propName, OrderTypeEnum.ThenBy);

        return query;
    }

    public static IQuery<T> ThenByDescending<T>(this IQuery<T> query, string propName) where T : class
    {
        if (query is null || propName is null) return query;

        query.SortOrderBuilder.Add(propName, OrderTypeEnum.ThenByDescending);

        return query;
    }
}
