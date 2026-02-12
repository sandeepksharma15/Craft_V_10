using Craft.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Craft.QuerySpec;

public sealed class PaginationEvaluator : IEvaluator
{
    public static PaginationEvaluator Instance { get; } = new PaginationEvaluator();

    private PaginationEvaluator() { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);

        // Check if pagination is being used
        var hasPagination = (query?.Skip is not null && query.Skip.Value >= 0) ||
                           (query?.Take is not null && query.Take.Value > 0);

        // If pagination is used but no ordering is specified in the query, add default ordering by Id
        // This prevents EF Core warnings about unpredictable results when using Skip/Take without OrderBy
        if (hasPagination)
        {
            var hasOrdering = query?.SortOrderBuilder?.OrderDescriptorList?.Count > 0;

            if (!hasOrdering)
            {
                // Try to order by Id property if it exists
                var idProperty = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                if (idProperty is not null)
                {
                    var parameter = Expression.Parameter(typeof(T), "x");
                    var propertyAccess = Expression.Property(parameter, idProperty);
                    var converted = Expression.Convert(propertyAccess, typeof(object));
                    var lambda = Expression.Lambda<Func<T, object>>(converted, parameter);
                    queryable = queryable.OrderBy(lambda);
                }
            }
        }

        if (query?.Skip is not null && (query.Skip.Value >= 0))
            queryable = queryable.Skip(query.Skip.Value);

        if (query?.Take is not null && (query.Take.Value > 0))
            queryable = queryable.Take(query.Take.Value);

        return queryable;
    }
}

