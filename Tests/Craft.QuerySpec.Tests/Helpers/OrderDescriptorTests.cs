using System.Linq.Expressions;
using System.Text.Json;

namespace Craft.QuerySpec.Tests.Helpers;

public class OrderDescriptorTests
{
    private readonly JsonSerializerOptions serializeOptions;

    public OrderDescriptorTests()
    {
        serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new OrderDescriptorJsonConverter<MyTestClass>());
    }

    [Fact]
    public void Constructor_WithValidOrderItem_InitializesProperties()
    {
        // Arrange
        Expression<Func<string, object?>> orderItemExpression = s => s.Length;

        // Act
        var orderInfo = new OrderDescriptor<string>(orderItemExpression!, 
            OrderTypeEnum.OrderBy);

        // Assert
        Assert.Equal(orderItemExpression.ToString(), orderInfo.OrderItem.ToString());
        Assert.Equal(OrderTypeEnum.OrderBy, orderInfo.OrderType);
    }

    [Fact]
    public void Read_WithInvalidMemberExpression_ThrowsException()
    {
        // Arrange
        const string json = "{\"OrderItem\": \"InvalidMember\", \"OrderType\": 0}";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => JsonSerializer.Deserialize<OrderDescriptor<MyTestClass>>(json, serializeOptions));
    }

    [Fact]
    public void Read_WithNullJson_ReturnsNull()
    {
        // Arrange
        const string json = "null";

        // Act & Assert
        var orderInfo = JsonSerializer.Deserialize<OrderDescriptor<MyTestClass>>(json, 
            serializeOptions);
        Assert.Null(orderInfo);
    }

    [Fact]
    public void Serialization_RoundTrip_ReturnsEqualOrderInfo()
    {
        // Arrange
        Expression<Func<MyTestClass, object>> orderItemExpression = x => x.MyProperty;
        var orderInfo = new OrderDescriptor<MyTestClass>(orderItemExpression, OrderTypeEnum.OrderByDescending);

        // Act
        var serializationInfo = JsonSerializer.Serialize(orderInfo, serializeOptions);
        var deserializedOrderInfo = JsonSerializer.Deserialize<OrderDescriptor<MyTestClass>>(serializationInfo, serializeOptions);

        // Assert
        Assert.NotNull(deserializedOrderInfo);
        Assert.Equal(OrderTypeEnum.OrderByDescending, deserializedOrderInfo.OrderType);

        // Verify that the deserialized OrderItem is a valid expression
        var compiledDelegate = deserializedOrderInfo.OrderItem.Compile();
        var myTestClass = new MyTestClass { MyProperty = "TestValue" };
        var propertyValue = compiledDelegate.DynamicInvoke(myTestClass);
        Assert.Equal("TestValue", propertyValue);
    }

    [Fact]
    public void Write_SerializesOrderDescriptorToJsonCorrectly()
    {
        // Arrange
        Expression<Func<MyTestClass, object>> orderItemExpression = x => x.MyProperty;

        // Act
        var orderInfo = new OrderDescriptor<MyTestClass>(orderItemExpression, OrderTypeEnum.OrderBy);
        var json = JsonSerializer.Serialize(orderInfo, serializeOptions);

        // Assert
        Assert.Contains("OrderItem", json);
        Assert.Contains("OrderType", json);
        Assert.Contains("MyProperty", json);
    }

    [Fact]
    public void Constructor_WithNullOrderItem_DoesNotThrows()
    {
        // Arrange & Act
        var ex = Record.Exception(() => new OrderDescriptor<MyTestClass>(null!, OrderTypeEnum.OrderBy));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void Constructor_DefaultsToOrderBy()
    {
        // Arrange & Act
        Expression<Func<MyTestClass, object>> orderItemExpression = x => x.MyProperty;
        var orderInfo = new OrderDescriptor<MyTestClass>(orderItemExpression);

        // Assert
        Assert.Equal(OrderTypeEnum.OrderBy, orderInfo.OrderType);
    }

    public class MyTestClass
    {
        public int Id { get; set; }
        public string MyProperty { get; set; } = string.Empty;
    }
}
