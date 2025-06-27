using System.Linq.Expressions;
using System.Text.Json;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Builders;

public class SortOrderBuilderTests
{
    private readonly JsonSerializerOptions serializeOptions;

    public SortOrderBuilderTests()
    {
        // Arrange
        serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new SortOrderBuilderJsonConverter<Company>());
    }

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
    public void Write_Should_Serialize_OrderBuilder_OrderExpressions()
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<Company>();
        orderBuilder.Add(c => c.Name!);
        orderBuilder.Add(c => c.Id);

        // Act
        var serializedOrderBuilder = JsonSerializer.Serialize(orderBuilder, serializeOptions);

        // Assert
        Assert.Contains("Name", serializedOrderBuilder);
        Assert.Contains("Id", serializedOrderBuilder);
    }

    [Fact]
    public void Read_Should_Deserialize_OrderBuilder_With_OrderExpressions()
    {
        // Arrange
        const string json = "[{\"OrderItem\":\"Name\",\"OrderType\":1},{\"OrderItem\":\"Id\",\"OrderType\":3}]";

        // Act
        var orderBuilder = JsonSerializer.Deserialize<SortOrderBuilder<Company>>(json, serializeOptions);

        // Assert
        Assert.NotNull(orderBuilder);
        Assert.Equal(2, orderBuilder.OrderDescriptorList.Count);
        Assert.Contains("x.Name", orderBuilder.OrderDescriptorList[0].OrderItem.Body.ToString());
        Assert.Contains("x.Id", orderBuilder.OrderDescriptorList[1].OrderItem.Body.ToString());
    }

    [Fact]
    public void Add_NullOrderDescriptor_ThrowsArgumentNullException()
    {
        var builder = new SortOrderBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Add((OrderDescriptor<Company>)null!));
    }

    [Fact]
    public void Add_NullPropertyExpression_ThrowsArgumentNullException()
    {
        var builder = new SortOrderBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Add((Expression<Func<Company, object>>)null!));
    }

    [Fact]
    public void Add_InvalidPropertyName_ThrowsArgumentNullException()
    {
        var builder = new SortOrderBuilder<Company>();

        Assert.Throws<ArgumentException>(() => builder.Add("NotAProp"));
    }

    [Fact]
    public void Remove_NullPropertyExpression_ThrowsArgumentNullException()
    {
        var builder = new SortOrderBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Remove((Expression<Func<Company, object>>)null!));
    }

    [Fact]
    public void Remove_InvalidPropertyName_ThrowsArgumentNullException()
    {
        var builder = new SortOrderBuilder<Company>();
        Assert.Throws<ArgumentNullException>(() => builder.Remove("NotAProp"));
    }

    [Fact]
    public void Remove_OnEmptyBuilder_DoesNotThrow()
    {
        var builder = new SortOrderBuilder<Company>();
        var expr = (Expression<Func<Company, object>>)(x => x.Name!);
        builder.Remove(expr);
        Assert.Empty(builder.OrderDescriptorList);
    }

    [Fact]
    public void Add_DuplicateOrderExpressions_AddsBoth()
    {
        var builder = new SortOrderBuilder<Company>();
        Expression<Func<Company, object>> expr = x => x.Name!;
        builder.Add(expr);
        builder.Add(expr);
        Assert.Equal(2, builder.OrderDescriptorList.Count);
    }

    [Fact]
    public void Clear_OnEmptyBuilder_DoesNotThrow()
    {
        var builder = new SortOrderBuilder<Company>();
        builder.Clear();
        Assert.Empty(builder.OrderDescriptorList);
    }

    [Fact]
    public void Add_And_Remove_WithDifferentOrderTypes()
    {
        var builder = new SortOrderBuilder<Company>();
        builder.Add(x => x.Id, OrderTypeEnum.OrderByDescending);
        builder.Add(x => x.Name!, OrderTypeEnum.OrderBy);
        builder.Remove(x => x.Id);
        Assert.Single(builder.OrderDescriptorList);
        Assert.Contains(builder.OrderDescriptorList, o => o.OrderItem.Body.ToString().Contains("x.Name"));
    }
}
