using System.Linq.Expressions;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// Builder class for creating order expressions.
/// </summary>
/// <remarks>
/// This class is NOT thread-safe. Do not share instances across threads.
/// </remarks>
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

    /// <summary>
    /// Gets the number of order expressions.
    /// </summary>
    public int Count => OrderDescriptorList.Count;

    public SortOrderBuilder<T> Add(OrderDescriptor<T> orderInfo)
    {
        ArgumentNullException.ThrowIfNull(orderInfo);

        if (CheckForDuplicate(orderInfo.OrderItem))
            throw new ArgumentException($"Order '{orderInfo.OrderItem.Body}' already exists in the list.");

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

        Add(new OrderDescriptor<T>(propExpr, orderType));

        return this;
    }

    /// <summary>
    /// Adds an order expression based on a property name.
    /// </summary>
    public SortOrderBuilder<T> Add(string propName, OrderTypeEnum orderType = OrderTypeEnum.OrderBy)
    {
        if (propName.IsNullOrWhiteSpace())
            throw new ArgumentException($"Property {nameof(propName)} cannot be null or empty.");

        var propExpr = ExpressionBuilder.GetPropertyExpression<T>(propName)
            ?? throw new ArgumentException($"Property '{propName}' does not exist on type '{typeof(T).Name}'.");

        Add(new OrderDescriptor<T>(propExpr!, orderType));

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

    private bool CheckForDuplicate(LambdaExpression propExpr)
    {
        ArgumentNullException.ThrowIfNull(propExpr);

        var comparer = new ExpressionSemanticEqualityComparer();

        return OrderDescriptorList.Any(x => comparer.Equals(x.OrderItem, propExpr));
    }

    /// <summary>
    /// Removes an order expression based on a property name.
    /// </summary>
    public SortOrderBuilder<T> Remove(string propName)
    {
        if (propName.IsNullOrWhiteSpace())
            throw new ArgumentException($"Property {nameof(propName)} cannot be null or empty.");

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

    /// <summary>
    /// Returns a string representation of the current order chain for debugging.
    /// </summary>
    public override string ToString()
    {
        return string.Join(", ", OrderDescriptorList.Select(x => $"{x.OrderType}: {x.OrderItem}"));
    }
}
