using System.Linq.Expressions;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// Builder class for creating order expressions.
/// </summary>
[Serializable]
public class SortOrderBuilder<T> where T : class
{
    /// <summary>
    /// Constructor to initialize the SortOrderBuilder.
    /// </summary>
    public SortOrderBuilder() => OrderDescriptorList = [];

    /// <summary>
    /// List of order expressions.
    /// </summary>
    public List<OrderDescriptor<T>> OrderDescriptorList { get; }

    public long Count => OrderDescriptorList.Count;

    public SortOrderBuilder<T> Add(OrderDescriptor<T> orderInfo)
    {
        ArgumentNullException.ThrowIfNull(orderInfo);
        orderInfo.OrderType = AdjustOrderType(orderInfo.OrderType);
        OrderDescriptorList.Add(orderInfo);
        return this;
    }

    /// <summary>
    /// Adds an order expression based on a property expression.
    /// </summary>
    public SortOrderBuilder<T> Add(Expression<Func<T, object>> propExpr, OrderTypeEnum orderType = OrderTypeEnum.OrderBy)
    {
        ArgumentNullException.ThrowIfNull(propExpr);
        OrderDescriptorList.Add(new OrderDescriptor<T>(propExpr, AdjustOrderType(orderType)));
        return this;
    }

    /// <summary>
    /// Adds an order expression based on a property name.
    /// </summary>
    public SortOrderBuilder<T> Add(string propName, OrderTypeEnum orderType = OrderTypeEnum.OrderBy)
    {
        var propExpr = ExpressionBuilder.GetPropertyExpression<T>(propName);
        OrderDescriptorList.Add(new OrderDescriptor<T>(propExpr!, AdjustOrderType(orderType)));

        return this;
    }

    /// <summary>
    /// Clears all order expressions.
    /// </summary>
    public SortOrderBuilder<T> Clear()
    {
        OrderDescriptorList.Clear();
        return this;
    }

    /// <summary>
    /// Removes an order expression based on a property expression.
    /// </summary>
    public SortOrderBuilder<T> Remove(Expression<Func<T, object>> propExpr)
    {
        ArgumentNullException.ThrowIfNull(propExpr);
        var comparer = new ExpressionSemanticEqualityComparer();
        var orderInfo = OrderDescriptorList.Find(x => comparer.Equals(x.OrderItem, propExpr));

        if (orderInfo != null)
            OrderDescriptorList.Remove(orderInfo);

        return this;
    }

    /// <summary>
    /// Removes an order expression based on a property name.
    /// </summary>
    public SortOrderBuilder<T> Remove(string propName)
    {
        Remove(ExpressionBuilder.GetPropertyExpression<T>(propName)!);
        return this;
    }

    /// <summary>
    /// Adjusts the order type based on existing expressions.
    /// </summary>
    internal OrderTypeEnum AdjustOrderType(OrderTypeEnum orderType)
    {
        if (OrderDescriptorList.Any(x => x.OrderType is OrderTypeEnum.OrderBy or OrderTypeEnum.OrderByDescending))
            if (orderType is OrderTypeEnum.OrderBy)
                orderType = OrderTypeEnum.ThenBy;
            else if (orderType is OrderTypeEnum.OrderByDescending)
                orderType = OrderTypeEnum.ThenByDescending;
        return orderType;
    }
}
