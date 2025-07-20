using System.Linq.Expressions;
using System.Text.Json;

namespace Craft.QuerySpec.Tests.Converters;

public class OrderDescriptorJsonConverterTests
{
    private readonly JsonSerializerOptions serializeOptions;

    public class MyTestClass
    {
        public int Id { get; set; }
        public string MyProperty { get; set; } = string.Empty;
    }

    public OrderDescriptorJsonConverterTests()
    {
        serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new OrderDescriptorJsonConverter<MyTestClass>());
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
    public void Write_WithNullOrderDescriptor_WritesJsonNull()
    {
        // Arrange & Act
        var json = JsonSerializer.Serialize<OrderDescriptor<MyTestClass>>(null!, serializeOptions);

        // Assert
        Assert.Equal("null", json);
    }

    [Fact]
    public void Read_WithNullJson_ReturnsNull()
    {
        // Arrange
        const string json = "null";

        // Act
        var orderInfo = JsonSerializer.Deserialize<OrderDescriptor<MyTestClass>>(json, serializeOptions);

        // Assert
        Assert.Null(orderInfo);
    }

    [Fact]
    public void Read_WithMissingOrderItem_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"OrderType\": 1}";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OrderDescriptor<MyTestClass>>(json, serializeOptions));
    }

    [Fact]
    public void Read_WithInvalidMemberExpression_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"OrderItem\": \"InvalidMember\", \"OrderType\": 1}";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => JsonSerializer.Deserialize<OrderDescriptor<MyTestClass>>(json, serializeOptions));
    }

    [Fact]
    public void Read_WithInvalidOrderType_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"OrderItem\": \"MyProperty\", \"OrderType\": \"notAnInt\"}";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OrderDescriptor<MyTestClass>>(json, serializeOptions));
    }

    [Fact]
    public void Read_WithUnknownProperty_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"OrderItem\": \"MyProperty\", \"OrderType\": 1, \"Unknown\": 123}";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OrderDescriptor<MyTestClass>>(json, serializeOptions));
    }

    [Fact]
    public void Write_WithOrderItemNotAProperty_ThrowsJsonException()
    {
        // Arrange
        Expression<Func<MyTestClass, object>> expr = x => x.Id + 1;
        var orderInfo = new OrderDescriptor<MyTestClass>(expr, OrderTypeEnum.OrderBy);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => JsonSerializer.Serialize(orderInfo, serializeOptions));
    }
}
