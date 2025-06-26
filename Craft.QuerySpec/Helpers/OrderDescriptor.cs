using System.Linq.Expressions;

namespace Craft.QuerySpec;

[Serializable]
public class OrderDescriptor<T> where T : class
{
    public OrderDescriptor(LambdaExpression orderItem, OrderTypeEnum orderType = OrderTypeEnum.OrderBy)
    {
        OrderItem = orderItem;
        OrderType = orderType;
    }

    public OrderDescriptor(Expression<Func<T, object>> orderItem, OrderTypeEnum orderType = OrderTypeEnum.OrderBy)
    {
        OrderItem = orderItem;
        OrderType = orderType;
    }

    public LambdaExpression OrderItem { get; internal set; }
    public OrderTypeEnum OrderType { get; internal set; }
}
