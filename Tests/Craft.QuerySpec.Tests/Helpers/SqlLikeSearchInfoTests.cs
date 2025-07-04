using System.Linq.Expressions;
using System.Text.Json;

namespace Craft.QuerySpec.Tests.Helpers;

public class SqlLikeSearchInfoTests
{
    private static readonly JsonSerializerOptions CachedJsonSerializerOptions = CreateJsonSerializerOptions();

    [Fact]
    public void Constructor_InitializationWithValidValues()
    {
        // Arrange
        Expression<Func<MyResult, string>> searchItem = x => x.ResultName;
        const string searchString = "x%y";
        const int searchGroup = 2;

        // Act
        var searchInfo = new SqlLikeSearchInfo<MyResult>(searchItem, searchString, searchGroup);

        // Assert
        Assert.Equal(searchGroup, searchInfo.SearchGroup);
        Assert.Equal(searchString, searchInfo.SearchString);
        Assert.Equal(searchItem.ToString(), searchInfo.SearchItem?.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_InvalidSearchTerm_ThrowsException(string? searchString)
    {
        // Arrange
        Expression<Func<MyResult, string>> searchItem = x => x.ResultName;

        // Act & Assert
        if (searchString == null) 
            Assert.Throws<ArgumentNullException>(() => new SqlLikeSearchInfo<MyResult>(searchItem, searchString!));
        else
            Assert.Throws<ArgumentException>(() => new SqlLikeSearchInfo<MyResult>(searchItem, searchString!));
    }

    [Fact]
    public void Constructor_NullSearchExpression_ThrowsException()
    {
        // Arrange
        Assert.Throws<ArgumentNullException>(() => new SqlLikeSearchInfo<object>(null!, "validTerm"));
    }

    [Fact]
    public void DefaultConstructor_Initialization()
    {
        // Act
        var searchInfo = new SqlLikeSearchInfo<MyResult>();

        // Assert
        Assert.Equal(0, searchInfo.SearchGroup);
        Assert.Null(searchInfo.SearchString);
        Assert.Null(searchInfo.SearchItem);
    }

    [Fact]
    public void Serialization_RoundTrip_ShouldPreserveSearchItem()
    {
        // Arrange
        Expression<Func<MyResult, string>> searchItem = x => x.ResultName;
        const string searchString = "x%y";
        const int searchGroup = 2;
        var searchInfo = new SqlLikeSearchInfo<MyResult>(searchItem, searchString, searchGroup);

        // Act
        var serializationInfo = JsonSerializer.Serialize(searchInfo, CachedJsonSerializerOptions);
        var deserializedInfo = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(serializationInfo, CachedJsonSerializerOptions);

        // Assert
        Assert.NotNull(deserializedInfo);
        Assert.NotNull(deserializedInfo.SearchItem);
        Assert.Equal(searchString, deserializedInfo.SearchString);
        Assert.Equal(searchGroup, deserializedInfo.SearchGroup);
        Assert.Equal(searchItem.ToString(), deserializedInfo.SearchItem?.ToString());
    }

    [Fact]
    public void JsonConverter_Read_WithNullJson_ReturnsNull()
    {
        // Arrange
        const string json = "null";

        // Act & Assert
        var searchInfo = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);

        // Assert
        Assert.Null(searchInfo);
    }

    [Fact]
    public void JsonConverter_Read_WithInvalidMemberExpression_ThrowsException()
    {
        // Arrange
        const string json = "{\"SearchItem\": \"InvalidMember\", \"SearchString\": \"x%y\", \"SearchGroup\": 2}";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions));
    }

    [Fact]
    public void JsonConverter_Read_WithMissingProperties_AllDefaults()
    {
        // Arrange
        const string json = "{}";

        // Act
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);

        // Assert
        Assert.NotNull(info);
        Assert.Null(info.SearchItem);
        Assert.Null(info.SearchString);
        Assert.Equal(0, info.SearchGroup);
    }

    [Fact]
    public void JsonConverter_Read_WithUnknownProperties_IgnoresThem()
    {
        // Arrange
        const string json = "{\"SearchItem\":\"ResultName\",\"SearchString\":\"abc\",\"SearchGroup\":1,\"Extra\":123}";

        // Act
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);

        // Assert
        Assert.NotNull(info);
        Assert.NotNull(info.SearchItem);
        Assert.Equal("abc", info.SearchString);
        Assert.Equal(1, info.SearchGroup);
    }

    [Fact]
    public void JsonConverter_Read_WithNullSearchItem_DoesNotThrow()
    {
        // Arrange
        const string json = "{\"SearchItem\":null,\"SearchString\":\"abc\",\"SearchGroup\":1}";

        // Act
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);

        // Assert
        Assert.NotNull(info);
        Assert.Null(info.SearchItem);
        Assert.Equal("abc", info.SearchString);
        Assert.Equal(1, info.SearchGroup);
    }

    [Fact]
    public void JsonConverter_Read_WithEmptySearchItem_DoesNotThrow()
    {
        // Arrange
        const string json = "{\"SearchItem\":\"\",\"SearchString\":\"abc\",\"SearchGroup\":1}";

        // Act
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);

        // Assert
        Assert.NotNull(info);
        Assert.Null(info.SearchItem);
        Assert.Equal("abc", info.SearchString);
        Assert.Equal(1, info.SearchGroup);
    }

    [Fact]
    public void JsonConverter_Write_WithNulls_SerializesCorrectly()
    {
        // Arrange
        var info = new SqlLikeSearchInfo<MyResult>();

        // Act
        var json = JsonSerializer.Serialize(info, CachedJsonSerializerOptions);

        // Assert
        Assert.Contains("\"SearchItem\":null", json);
        Assert.Contains("\"SearchString\":null", json);
        Assert.Contains("\"SearchGroup\":0", json);
    }

    [Fact]
    public void JsonConverter_Write_And_Read_RoundTrip_AllNulls()
    {
        // Arrange
        var info = new SqlLikeSearchInfo<MyResult>();
        var json = JsonSerializer.Serialize(info, CachedJsonSerializerOptions);

        // Act
        var deserialized = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.SearchItem);
        Assert.Null(deserialized.SearchString);
        Assert.Equal(0, deserialized.SearchGroup);
    }

    [Fact]
    public void JsonConverter_Read_WithNonObject_ThrowsJsonException()
    {
        // Arrange
        const string json = "[]";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions));
    }

    [Fact]
    public void JsonConverter_Read_WithNonStringSearchGroup_DefaultsToZero()
    {
        // Arrange
        const string json = "{\"SearchItem\":\"ResultName\",\"SearchString\":\"abc\",\"SearchGroup\":\"notanumber\"}";

        // Act
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);

        // Assert
        Assert.NotNull(info);
        Assert.Equal(0, info.SearchGroup);
    }

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new SqlLikeSearchInfoJsonConverter<MyResult>());
        return options;
    }

    private class MyResult
    {
        public long Id { get; set; }
        public string ResultName { get; set; } = string.Empty;
    }
}
