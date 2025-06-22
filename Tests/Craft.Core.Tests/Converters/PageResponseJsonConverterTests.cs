using System.Text.Json;

namespace Craft.Core.Tests.Converters;

public class PageResponseJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public PageResponseJsonConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new PageResponseJsonConverter<string>());
    }

    [Fact]
    public void CanConvert_ReturnsTrue_ForPageResponseType()
    {
        // Arrange
        var converter = new PageResponseJsonConverter<string>();

        // Act & Assert
        Assert.True(converter.CanConvert(typeof(PageResponse<string>)));
    }

    [Fact]
    public void CanConvert_ReturnsFalse_ForOtherType()
    {
        // Arrange
        var converter = new PageResponseJsonConverter<string>();

        // Act & Assert
        Assert.False(converter.CanConvert(typeof(string)));
    }

    [Fact]
    public void Write_Serializes_PageResponse_To_Expected_Json()
    {
        // Arrange
        var items = new List<string> { "A", "B" };
        var response = new PageResponse<string>(items, 50, 3, 2);

        // Act
        var json = JsonSerializer.Serialize(response, _options);

        // Assert
        Assert.Equal("{\"Items\":[\"A\",\"B\"],\"CurrentPage\":3,\"PageSize\":2,\"TotalCount\":50}", json);
    }

    [Fact]
    public void Read_Deserializes_Valid_Json_To_PageResponse()
    {
        // Arrange
        var json = "{\"Items\":[\"A\",\"B\"],\"CurrentPage\":3,\"PageSize\":2,\"TotalCount\":50}";

        // Act
        var result = JsonSerializer.Deserialize<PageResponse<string>>(json, _options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result!.CurrentPage);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(50, result.TotalCount);
        Assert.Equivalent(new List<string> { "A", "B" }, result.Items);
    }

    [Fact]
    public void Read_Throws_If_Not_Object_Start()
    {
        // Arrange
        var json = "[]";

        // Act & Assert
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PageResponse<string>>(json, _options));
        Assert.Contains("expected start object", ex.Message);
    }

    [Fact]
    public void Read_Throws_If_PropertyName_Missing()
    {
        // Arrange
        // Malformed JSON: property name is not a string
        var json = "{ 123: 1 }";

        // Act & Assert
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PageResponse<string>>(json, _options));
    }

    [Fact]
    public void Read_Skips_Unknown_Properties()
    {
        // Arrange
        var json = "{\"Items\":[\"A\"],\"CurrentPage\":1,\"PageSize\":1,\"TotalCount\":1,\"Unknown\":123}";

        // Act
        var result = JsonSerializer.Deserialize<PageResponse<string>>(json, _options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.CurrentPage);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(1, result.TotalCount);
        Assert.Equivalent(new List<string> { "A" }, result.Items);
    }

    [Fact]
    public void Read_Uses_Defaults_If_Properties_Missing()
    {
        // Arrange
        var json = "{ }";

        // Act
        var result = JsonSerializer.Deserialize<PageResponse<string>>(json, _options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.CurrentPage); // default
        Assert.Equal(10, result.PageSize); // default
        Assert.Equal(0, result.TotalCount); // default
        Assert.Empty(result.Items); // default
    }

    [Fact]
    public void Write_Handles_Empty_Items()
    {
        // Arrange
        var response = new PageResponse<string>([], 0, 1, 10);

        // Act
        var json = JsonSerializer.Serialize(response, _options);

        // Assert
        Assert.Contains("\"Items\":[]", json);
    }
}
