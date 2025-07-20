using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Craft.TestDataStore.Models;
using Craft.QuerySpec;
using Xunit;

namespace Craft.QuerySpec.Tests.Builders;

public class SortOrderBuilderJsonConverterTests
{
    private readonly JsonSerializerOptions serializeOptions;

    public SortOrderBuilderJsonConverterTests()
    {
        // Arrange
        serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new SortOrderBuilderJsonConverter<Company>());
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
    public void Write_Should_Serialize_Empty_OrderBuilder_As_EmptyArray()
    {
        // Arrange
        var orderBuilder = new SortOrderBuilder<Company>();

        // Act
        var serializedOrderBuilder = JsonSerializer.Serialize(orderBuilder, serializeOptions);

        // Assert
        Assert.Equal("[]", serializedOrderBuilder);
    }

    [Fact]
    public void Write_Should_Serialize_Null_OrderBuilder_As_Null()
    {
        // Arrange
        SortOrderBuilder<Company> orderBuilder = null!;

        // Act
        var serializedOrderBuilder = JsonSerializer.Serialize(orderBuilder, serializeOptions);

        // Assert
        Assert.Equal("null", serializedOrderBuilder);
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
    public void Read_Should_Deserialize_EmptyArray_To_EmptyBuilder()
    {
        // Arrange
        const string json = "[]";

        // Act
        var orderBuilder = JsonSerializer.Deserialize<SortOrderBuilder<Company>>(json, serializeOptions);

        // Assert
        Assert.NotNull(orderBuilder);
        Assert.Empty(orderBuilder.OrderDescriptorList);
    }

    [Fact]
    public void Read_Should_Deserialize_Null_To_Null()
    {
        // Arrange
        const string json = "null";

        // Act
        var orderBuilder = JsonSerializer.Deserialize<SortOrderBuilder<Company>>(json, serializeOptions);

        // Assert
        Assert.Null(orderBuilder);
    }

    [Fact]
    public void Read_InvalidJson_NotArray_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"OrderItem\":\"Name\",\"OrderType\":1}";

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<SortOrderBuilder<Company>>(json, serializeOptions));
    }

    [Fact]
    public void Read_ArrayWithInvalidOrderDescriptor_ThrowsJsonException()
    {
        // Arrange
        const string json = "[{\"OrderType\":1}]"; // Missing OrderItem

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<SortOrderBuilder<Company>>(json, serializeOptions));
    }

    [Fact]
    public void Write_WithNullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        var converter = new SortOrderBuilderJsonConverter<Company>();
        var orderBuilder = new SortOrderBuilder<Company>();
        var options = new JsonSerializerOptions();
        options.Converters.Add(new SortOrderBuilderJsonConverter<Company>());

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => converter.Write(null!, orderBuilder, options));
    }

    [Fact]
    public void CanConvert_Returns_True_For_SortOrderBuilderType()
    {
        // Arrange
        var converter = new SortOrderBuilderJsonConverter<Company>();

        // Act
        var result = converter.CanConvert(typeof(SortOrderBuilder<Company>));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_Returns_False_For_OtherType()
    {
        // Arrange
        var converter = new SortOrderBuilderJsonConverter<Company>();

        // Act
        var result = converter.CanConvert(typeof(string));

        // Assert
        Assert.False(result);
    }
}
