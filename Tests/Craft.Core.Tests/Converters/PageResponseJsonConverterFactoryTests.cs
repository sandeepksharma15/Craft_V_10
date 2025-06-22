using System.Text.Json;

namespace Craft.Core.Tests.Converters;

public class PageResponseJsonConverterFactoryTests
{
    [Fact]
    public void CanConvert_ReturnsTrue_For_PageResponse_GenericType()
    {
        // Arrange
        var factory = new PageResponseJsonConverterFactory();

        // Act & Assert
        Assert.True(factory.CanConvert(typeof(PageResponse<string>)));
        Assert.True(factory.CanConvert(typeof(PageResponse<object>)));
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_NonGeneric_Or_NonPageResponse_Type()
    {
        // Arrange
        var factory = new PageResponseJsonConverterFactory();

        // Act & Assert
        Assert.False(factory.CanConvert(typeof(string)));
        Assert.False(factory.CanConvert(typeof(object)));
        Assert.False(factory.CanConvert(typeof(System.Collections.Generic.List<string>)));
    }

    [Fact]
    public void CreateConverter_Returns_PageResponseJsonConverter_For_GenericType()
    {
        // Arrange
        var factory = new PageResponseJsonConverterFactory();
        var options = new JsonSerializerOptions();
        var converter = factory.CreateConverter(typeof(PageResponse<string>), options);

        // Act & Assert
        Assert.NotNull(converter);
        Assert.IsType<PageResponseJsonConverter<string>>(converter);
    }

    [Fact]
    public void CreateConverter_Throws_If_Type_Is_Not_Generic()
    {
        // Arrange
        var factory = new PageResponseJsonConverterFactory();
        var options = new JsonSerializerOptions();

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => factory.CreateConverter(typeof(string), options));
    }

    [Fact]
    public void Converter_Integration_Works_With_JsonSerializer()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        options.Converters.Add(new PageResponseJsonConverterFactory());
        var page = new PageResponse<string>(["A", "B"], 2, 1, 10);

        // Act & Assert
        var json = JsonSerializer.Serialize(page, options);
        var deserialized = JsonSerializer.Deserialize<PageResponse<string>>(json, options);
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized!.TotalCount);
        Assert.Equivalent(new[] { "A", "B" }, deserialized.Items);
    }
}
