using System.Linq.Expressions;
using System.Text.Json;

namespace Craft.QuerySpec.Tests.Helpers;

public class SqlLikeSearchInfoTests
{
    private class MyResult
    {
        public long Id { get; set; }
        public string ResultName { get; set; } = string.Empty;
    }

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
        var serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new SqlLikeSearchInfoJsonConverter<MyResult>());

        // Act
        var serializationInfo = JsonSerializer.Serialize(searchInfo, serializeOptions);
        var deserializedInfo = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(serializationInfo, serializeOptions);

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
        var serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new SqlLikeSearchInfoJsonConverter<MyResult>());
        const string json = "null";

        // Act & Assert
        var searchInfo = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, serializeOptions);

        // Assert
        Assert.Null(searchInfo);
    }

    [Fact]
    public void JsonConverter_Read_WithInvalidMemberExpression_ThrowsException()
    {
        // Arrange
        var serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new SqlLikeSearchInfoJsonConverter<MyResult>());
        const string json = "{\"SearchItem\": \"InvalidMember\", \"SearchString\": \"x%y\", \"SearchGroup\": 2}";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, serializeOptions));
    }
}
