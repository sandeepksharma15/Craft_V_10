using System.Linq.Expressions;

namespace Craft.QuerySpec;

public sealed class OrderEvaluator : IEvaluator
{
    public static OrderEvaluator Instance { get; } = new OrderEvaluator();

    private OrderEvaluator()
    { }

    public IQueryable<T>? GetQuery<T>(IQueryable<T>? queryable, IQuery<T>? query) where T : class
    {
        ArgumentNullException.ThrowIfNull(queryable);

        if (query?.SortOrderBuilder?.OrderDescriptorList.Count == 0)
            return queryable;

        if (query?.SortOrderBuilder?.OrderDescriptorList.Count(x => x.OrderType is OrderTypeEnum.OrderBy
                    or OrderTypeEnum.OrderByDescending) > 1)
            throw new ArgumentException("There is a duplicate order chain");

        IOrderedQueryable<T>? orderedQueryable = null;

        foreach (var orderExpression in query?.SortOrderBuilder?.OrderDescriptorList ?? [])
            if (orderExpression.OrderType == OrderTypeEnum.OrderBy)
                orderedQueryable = queryable?.OrderBy((Expression<Func<T, object>>)orderExpression.OrderItem);
            else if (orderExpression.OrderType == OrderTypeEnum.OrderByDescending)
                orderedQueryable = queryable?.OrderByDescending((Expression<Func<T, object>>)orderExpression.OrderItem);
            else if (orderExpression.OrderType == OrderTypeEnum.ThenBy)
                orderedQueryable = orderedQueryable?.ThenBy((Expression<Func<T, object>>)orderExpression.OrderItem);
            else if (orderExpression.OrderType == OrderTypeEnum.ThenByDescending)
                orderedQueryable = orderedQueryable?.ThenByDescending((Expression<Func<T, object>>)orderExpression.OrderItem);

        if (orderedQueryable is not null)
            queryable = orderedQueryable;

        return queryable;
    }
}
