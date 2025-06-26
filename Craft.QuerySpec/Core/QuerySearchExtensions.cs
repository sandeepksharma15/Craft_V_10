using System.Linq.Expressions;
using Craft.QuerySpec.Contracts;

namespace Craft.QuerySpec.Core;

public static class QuerySearchExtensions
{
    public static IQuery<T> Search<T>(this IQuery<T> query, Expression<Func<T, object>> member,
        string searchTerm, int searchGroup = 1) where T : class
    {
        if (query is null || member is null || searchTerm is null) return query;

        query.SqlLikeSearchCriteriaBuilder.Add(member, searchTerm, searchGroup);

        return query;
    }

    public static IQuery<T> Search<T>(this IQuery<T> query, string memberName,
        string searchTerm, int searchGroup = 1) where T : class
    {
        if (query is null || memberName is null || searchTerm is null) return query;

        query.SqlLikeSearchCriteriaBuilder.Add(memberName, searchTerm, searchGroup);

        return query;
    }
}
