using System.Text.Json;
using Craft.Core;

namespace Craft.Domain.Tests.Helpers;

public class PageResponseTests
{
    private readonly JsonSerializerOptions serializerOptions;

    public PageResponseTests()
    {
        serializerOptions = new();
        serializerOptions.Converters.Add(new PageResponseJsonConverter<string>());
    }

    [Fact]
    public void PageResponse_Constructor_SetsProperties()
    {
        // Arrange
        var items = new List<string> { "Item 1", "Item 2" };
        const int totalCount = 100;
        const int currentPage = 2;
        const int pageSize = 10;

        // Act
        var response = new PageResponse<string>(items, totalCount, currentPage, pageSize);

        // Assert
        Assert.Equal(totalCount, response.TotalCount);
        Assert.Equal(currentPage, response.CurrentPage);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Equivalent(items, response.Items);
    }

    [Fact]
    public void PageResponse_Constructor_WithNullItems_ReturnsEmptyCollection()
    {
        // Arrange
        const int totalCount = 100;
        const int currentPage = 2;
        const int pageSize = 10;

        // Act
        var response = new PageResponse<string>(null!, totalCount, currentPage, pageSize);

        // Assert
        Assert.Equal(totalCount, response.TotalCount);
        Assert.Equal(currentPage, response.CurrentPage);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Empty(response.Items);
    }

    [Fact]
    public void PageResponse_Serialization_Deserialization()
    {
        // Arrange
        var items = new List<string> { "Item 1", "Item 2" };
        const int totalCount = 100;
        const int currentPage = 2;
        const int pageSize = 10;
        var response = new PageResponse<string>(items, totalCount, currentPage, pageSize);

        var serializedData = JsonSerializer.Serialize(response);

        // Act
        var deserializedResponse = JsonSerializer.Deserialize<PageResponse<string>>(serializedData);

        // Assert
        Assert.Equivalent(items, deserializedResponse!.Items);
        Assert.Equal(totalCount, deserializedResponse.TotalCount);
        Assert.Equal(currentPage, deserializedResponse.CurrentPage);
        Assert.Equal(pageSize, deserializedResponse.PageSize);
    }

    [Fact]
    public void CanConvert_WithPageResponseType_ReturnsTrue()
    {
        // Arrange
        var converter = new PageResponseJsonConverter<string>();
        var type = typeof(PageResponse<string>);

        // Assert
        Assert.True(converter.CanConvert(type));
    }

    [Fact]
    public void CanConvert_WithOtherType_ReturnsFalse()
    {
        // Arrange
        var converter = new PageResponseJsonConverter<string>();
        var type = typeof(string); // Or any other type

        // Assert
        Assert.False(converter.CanConvert(type));
    }

    [Fact]
    public void Read_ValidJson_DeserializesCorrectly()
    {
        // Arrange
        const string json = @"{ ""Items"": [ ""Item 1"", ""Item 2"" ], ""CurrentPage"": 2, ""PageSize"": 10, ""TotalCount"": 100 }";
        var expectedItems = new List<string> { "Item 1", "Item 2" };

        // Act
        var response = JsonSerializer.Deserialize<PageResponse<string>>(json, serializerOptions);

        // Assert
        Assert.Equivalent(expectedItems, response!.Items);
        Assert.Equal(100, response.TotalCount);
        Assert.Equal(2, response.CurrentPage);
        Assert.Equal(10, response.PageSize);
    }

    [Fact]
    public void Write_PageResponse_SerializesCorrectly()
    {
        // Arrange
        var items = new List<string> { "Item 1", "Item 2" };
        var response = new PageResponse<string>(items, 100, 2, 10);

        // Act
        var serializedJson = JsonSerializer.Serialize(response, serializerOptions);

        // Assert
        Assert.Equal("{\"Items\":[\"Item 1\",\"Item 2\"],\"CurrentPage\":2,\"PageSize\":10,\"TotalCount\":100}", serializedJson);
    }
}
