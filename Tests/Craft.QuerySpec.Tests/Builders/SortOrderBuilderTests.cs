using System.Linq.Expressions;
using System.Text.Json;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Builders;

public class SortOrderBuilderTests
{
    [Fact]
    public void Add_Method_Should_Add_OrderExpression()
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<Company>();

        // Act
        orderBuilder.Add(x => x.Name!);

        // Assert
        Assert.NotEmpty(orderBuilder.OrderDescriptorList);
        Assert.NotNull(orderBuilder.OrderDescriptorList[0].OrderItem);
        Assert.Equal("x.Name", orderBuilder.OrderDescriptorList[0].OrderItem.Body.ToString());
    }

    [Fact]
    public void Add_WithExistingOrderBy_ShouldAdjustOrderTypeToThenBy()
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<Company>();
        Expression<Func<Company, object>> propExpr1 = x => x.Id;
        Expression<Func<Company, object>> propExpr2 = x => x.Name!;
        orderBuilder.Add(propExpr1);

        // Act
        var result = orderBuilder.Add(propExpr2, OrderTypeEnum.OrderBy);

        // Assert
        Assert.Equal(OrderTypeEnum.ThenBy, result.OrderDescriptorList[1].OrderType);
    }

    [Fact]
    public void Add_WithExistingOrderByDescending_ShouldAdjustOrderTypeToThenByDescending()
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<Company>();
        Expression<Func<Company, object>> propExpr1 = x => x.Id;
        Expression<Func<Company, object>> propExpr2 = x => x.Name!;
        orderBuilder.Add(propExpr1, OrderTypeEnum.OrderByDescending);

        // Act
        var result = orderBuilder.Add(propExpr2, OrderTypeEnum.OrderBy);

        // Assert
        Assert.Equal(OrderTypeEnum.ThenBy, result.OrderDescriptorList[1].OrderType);
    }

    [Fact]
    public void AddProperty_Method_Should_Add_OrderExpression()
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<Company>();

        // Act
        orderBuilder.Add("Name");

        // Assert
        Assert.NotEmpty(orderBuilder.OrderDescriptorList);
        Assert.NotNull(orderBuilder.OrderDescriptorList[0].OrderItem);
    }

    [Theory]
    [InlineData(OrderTypeEnum.OrderBy)]
    [InlineData(OrderTypeEnum.OrderByDescending)]
    public void AdjustOrderType_WhenExistingOrderExpressionsPresent_ShouldReturnThenBy(OrderTypeEnum existingOrderType)
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<object>();
        orderBuilder.OrderDescriptorList.Add(new OrderDescriptor<object>(null!, existingOrderType));

        // Act
        var adjustedOrderType = orderBuilder.AdjustOrderType(OrderTypeEnum.OrderBy);

        // Assert
        Assert.Equal(OrderTypeEnum.ThenBy, adjustedOrderType);
    }

    [Theory]
    [InlineData(OrderTypeEnum.OrderBy)]
    [InlineData(OrderTypeEnum.OrderByDescending)]
    public void AdjustOrderType_WhenExistingOrderExpressionsPresentAndOrderTypeIsDescending_ShouldReturnThenByDescending(OrderTypeEnum existingOrderType)
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<object>();
        orderBuilder.OrderDescriptorList.Add(new OrderDescriptor<object>(null!, existingOrderType));

        // Act
        var adjustedOrderType = orderBuilder.AdjustOrderType(OrderTypeEnum.OrderByDescending);

        // Assert
        Assert.Equal(OrderTypeEnum.ThenByDescending, adjustedOrderType);
    }

    [Fact]
    public void AdjustOrderType_WhenNoExistingOrderExpressions_ShouldReturnOriginalOrderType()
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<object>();

        // Act
        var adjustedOrderType = orderBuilder.AdjustOrderType(OrderTypeEnum.OrderBy);

        // Assert
        Assert.Equal(OrderTypeEnum.OrderBy, adjustedOrderType);
    }

    [Fact]
    public void Clear_Method_Should_Empty_OrderExpressions()
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<Company>();
        orderBuilder.Add(x => x.Name!);

        // Act
        orderBuilder.Clear();

        // Assert
        Assert.Empty(orderBuilder.OrderDescriptorList);
    }

    [Fact]
    public void Remove_Method_Should_Remove_OrderExpression()
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<Company>();
        Expression<Func<Company, object>> orderExpr = x => x.Name!;
        orderBuilder.Add(orderExpr);

        // Act
        orderBuilder.Remove(orderExpr);

        // Assert
        Assert.Empty(orderBuilder.OrderDescriptorList);
    }

    [Fact]
    public void RemoveProperty_Method_Should_Remove_OrderExpression()
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<Company>();
        orderBuilder.Add("Name");

        // Act
        orderBuilder.Remove("Name");

        // Assert
        Assert.Empty(orderBuilder.OrderDescriptorList);
    }

    [Fact]
    public void OrderBuilder_Should_Initialize_OrderExpressions_List()
    {
        // Arrange & Act
        var orderBuilder = new SortOrderBuilder<Company>();

        // Assert
        Assert.NotNull(orderBuilder.OrderDescriptorList);
        Assert.Empty(orderBuilder.OrderDescriptorList);
    }

    [Fact]
    public void OrderBuilder_Add_Should_Add_OrderInfo_To_OrderExpressions()
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<Company>();
        Expression<Func<Company, object>> orderExpr = x => x.Id;
        var orderInfo = new OrderDescriptor<Company>(orderExpr);

        // Act
        orderBuilder.Add(orderInfo);

        // Assert
        Assert.Contains(orderInfo, orderBuilder.OrderDescriptorList);
    }

    [Fact]
    public void Add_NullOrderDescriptor_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((OrderDescriptor<Company>)null!));
    }

    [Fact]
    public void Add_NullPropertyExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<Company, object>>)null!));
    }

    [Fact]
    public void Add_InvalidPropertyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Add("NotAProp"));
    }

    [Fact]
    public void Remove_NullPropertyExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Remove((Expression<Func<Company, object>>)null!));
    }

    [Fact]
    public void Remove_InvalidPropertyName_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Remove("NotAProp"));
    }

    [Fact]
    public void Remove_OnEmptyBuilder_DoesNotThrow()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();
        var expr = (Expression<Func<Company, object>>)(x => x.Name!);

        // Act
        builder.Remove(expr);

        // Assert
        Assert.Empty(builder.OrderDescriptorList);
    }

    [Fact]
    public void Add_DuplicateOrderExpressions_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();
        Expression<Func<Company, object>> expr = x => x.Name!;

        // Act
        builder.Add(expr);

        // Assert
        Assert.Throws<ArgumentException>(() => builder.Add(expr));
    }

    [Fact]
    public void Clear_OnEmptyBuilder_DoesNotThrow()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act
        builder.Clear();

        // Assert
        Assert.Empty(builder.OrderDescriptorList);
    }

    [Fact]
    public void Add_And_Remove_WithDifferentOrderTypes()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act
        builder.Add(x => x.Id, OrderTypeEnum.OrderByDescending);
        builder.Add(x => x.Name!, OrderTypeEnum.OrderBy);
        builder.Remove(x => x.Id);

        // Assert
        Assert.Single(builder.OrderDescriptorList);
        Assert.Contains(builder.OrderDescriptorList, o => o.OrderItem.Body.ToString().Contains("x.Name"));
    }

    [Fact]
    public void ToString_ShouldReturnOrderChainString()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act
        builder.Add(x => x.Name!);
        builder.Add(x => x.Id, OrderTypeEnum.OrderByDescending);
        var str = builder.ToString();

        // Assert
        Assert.Contains("OrderBy", str);
        Assert.Contains("ThenByDescending", str);
        Assert.Contains("x.Name", str);
        Assert.Contains("x.Id", str);
    }

    [Fact]
    public void Remove_NonExistentPropertyExpression_DoesNotThrowOrRemove()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act
        builder.Add(x => x.Name!);
        var expr = (Expression<Func<Company, object>>)(x => x.Id);
        builder.Remove(expr);

        // Assert
        Assert.Single(builder.OrderDescriptorList);
        Assert.Contains(builder.OrderDescriptorList, o => o.OrderItem.Body.ToString().Contains("x.Name"));
    }

    [Fact]
    public void Remove_NullPropertyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Remove((string)null!));
    }

    [Fact]
    public void Add_NullPropertyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Add((string)null!));
    }

    [Fact]
    public void Add_DuplicatePropertyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act
        builder.Add("Name");

        // Assert
        Assert.Throws<ArgumentException>(() => builder.Add("Name"));
    }

    [Fact]
    public void Add_WhitespacePropertyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Add("   "));
    }

    [Fact]
    public void Remove_WhitespacePropertyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Remove("   "));
    }

    [Fact]
    public void Add_InvalidButExistingPropertyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        // 'Equals' exists but is not a property
        Assert.Throws<ArgumentException>(() => builder.Add("Equals"));
    }

    [Fact]
    public void CountProperty_ShouldReflectOrderDescriptorListCount()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Equal(0, builder.Count);
        builder.Add(x => x.Name!);
        Assert.Equal(1, builder.Count);
        builder.Add(x => x.Id);
        Assert.Equal(2, builder.Count);
        builder.Clear();
        Assert.Equal(0, builder.Count);
    }

    [Fact]
    public void OrderDescriptorList_IsMutable()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();
        builder.Add(x => x.Name!);

        // Act
        builder.OrderDescriptorList.Clear();

        // Assert
        Assert.Empty(builder.OrderDescriptorList);
    }

    [Fact]
    public void Add_And_Remove_WithNestedPropertyExpression_Works()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();
        Expression<Func<Company, object>> expr = x => x.Country!.Name!;

        // Act
        builder.Add(expr);

        // Assert
        Assert.Single(builder.OrderDescriptorList);

        // Act
        builder.Remove(expr);

        // Assert
        Assert.Empty(builder.OrderDescriptorList);
    }

    [Fact]
    public void Remove_NonExistentPropertyName_DoesNotThrowOrRemove()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();
        builder.Add("Name");

        // Act
        builder.Remove("Id");

        // Assert
        Assert.Single(builder.OrderDescriptorList);
    }

    [Fact]
    public void Add_EmptyPropertyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Add(""));
    }

    [Fact]
    public void Remove_EmptyPropertyName_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SortOrderBuilder<Company>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.Remove(""));
    }
}
