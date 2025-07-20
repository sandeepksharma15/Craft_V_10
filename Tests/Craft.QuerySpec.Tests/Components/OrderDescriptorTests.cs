using System.Linq.Expressions;
using static Craft.QuerySpec.Tests.Converters.OrderDescriptorJsonConverterTests;

namespace Craft.QuerySpec.Tests.Components;

public class OrderDescriptorTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Ctor_LambdaExpression_SetsProperties()
    {
        // Arrange
        Expression<Func<TestEntity, object>> expr = x => x.Id;
        var lambda = (LambdaExpression)expr;

        // Act
        var desc = new OrderDescriptor<TestEntity>(lambda, OrderTypeEnum.OrderByDescending);

        // Assert
        Assert.Equal(lambda, desc.OrderItem);
        Assert.Equal(OrderTypeEnum.OrderByDescending, desc.OrderType);
    }

    [Fact]
    public void Ctor_ExpressionFunc_SetsProperties()
    {
        // Arrange
        Expression<Func<TestEntity, object>> expr = x => x.Name;

        // Act
        var desc = new OrderDescriptor<TestEntity>(expr, OrderTypeEnum.ThenBy);

        // Assert
        Assert.Equal(expr, desc.OrderItem);
        Assert.Equal(OrderTypeEnum.ThenBy, desc.OrderType);
    }

    [Fact]
    public void Ctor_DefaultOrderType_IsOrderBy()
    {
        // Arrange
        Expression<Func<TestEntity, object>> expr = x => x.Id;
        var lambda = (LambdaExpression)expr;

        // Act
        var desc1 = new OrderDescriptor<TestEntity>(lambda);
        var desc2 = new OrderDescriptor<TestEntity>(expr);

        // Assert
        Assert.Equal(OrderTypeEnum.OrderBy, desc1.OrderType);
        Assert.Equal(OrderTypeEnum.OrderBy, desc2.OrderType);
    }

    [Theory]
    [InlineData(OrderTypeEnum.OrderBy)]
    [InlineData(OrderTypeEnum.OrderByDescending)]
    [InlineData(OrderTypeEnum.ThenBy)]
    [InlineData(OrderTypeEnum.ThenByDescending)]
    public void Ctor_AllOrderTypes_AreSettable(OrderTypeEnum orderType)
    {
        // Arrange
        Expression<Func<TestEntity, object>> expr = x => x.Name;
        var lambda = (LambdaExpression)expr;

        // Act
        var desc1 = new OrderDescriptor<TestEntity>(lambda, orderType);
        var desc2 = new OrderDescriptor<TestEntity>(expr, orderType);

        // Assert
        Assert.Equal(orderType, desc1.OrderType);
        Assert.Equal(orderType, desc2.OrderType);
    }

    [Fact]
    public void OrderItem_CanBeSet_AndGet()
    {
        // Arrange
        Expression<Func<TestEntity, object>> expr1 = x => x.Id;
        Expression<Func<TestEntity, object>> expr2 = x => x.Name;

        var desc = new OrderDescriptor<TestEntity>(expr1) { OrderItem = expr2 };

        // Assert
        Assert.Equal(expr2, desc.OrderItem);
    }

    [Fact]
    public void OrderType_CanBeSet_AndGet()
    {
        // Arrange
        Expression<Func<TestEntity, object>> expr = x => x.Id;
        var desc = new OrderDescriptor<TestEntity>(expr) { OrderType = OrderTypeEnum.ThenByDescending };

        // Assert
        Assert.Equal(OrderTypeEnum.ThenByDescending, desc.OrderType);
    }

    [Fact]
    public void Ctor_NullLambdaExpression_DoesNotThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Record.Exception(() => new OrderDescriptor<TestEntity>((LambdaExpression)null!));
        Assert.Null(ex);
    }

    [Fact]
    public void Ctor_NullExpressionFunc_DoesNotThrowsArgumentNullException()
    {
        // Act & Assert
        var ex = Record.Exception(() => new OrderDescriptor<TestEntity>(null!));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData(OrderTypeEnum.OrderBy)]
    [InlineData(OrderTypeEnum.OrderByDescending)]
    [InlineData(OrderTypeEnum.ThenBy)]
    [InlineData(OrderTypeEnum.ThenByDescending)]
    public void Constructor_SetsAllOrderTypeEnumValues(OrderTypeEnum orderType)
    {
        // Arrange & Act
        Expression<Func<TestEntity, object>> orderItemExpression = x => x.Name;
        var orderInfo = new OrderDescriptor<TestEntity>(orderItemExpression, orderType);

        // Assert
        Assert.Equal(orderType, orderInfo.OrderType);
    }
}
