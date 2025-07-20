using System.Text.Json;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Converters;

public class SqlLikeSearchCriteriaBuilderJsonConverterTests
{
    private readonly JsonSerializerOptions serializeOptions;

    public SqlLikeSearchCriteriaBuilderJsonConverterTests()
    {
        serializeOptions = new JsonSerializerOptions();
        serializeOptions.Converters.Add(new SqlSearchCriteriaBuilderJsonConverter<Company>());

        // Ensure reflection-based property access works in all environments
        AppContext.SetSwitch("System.Text.Json.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
    }

    [Fact]
    public void Read_DeserializesValidJsonToSqlSearchCriteriaBuilder()
    {
        // Arrange
        const string json = "[{\"SearchItem\": \"Name\", \"SearchString\": \"John\", \"SearchGroup\": 1}]";

        // Act
        var searchBuilder = JsonSerializer.Deserialize<SqlLikeSearchCriteriaBuilder<Company>>(json, serializeOptions);

        // Assert
        Assert.Equal(1, searchBuilder?.Count);
        Assert.Contains("x.Name", searchBuilder?.SqlLikeSearchCriteriaList[0]?.SearchItem?.Body.ToString());
        Assert.Equal("John", searchBuilder?.SqlLikeSearchCriteriaList[0]?.SearchString);
        Assert.Equal(1, searchBuilder?.SqlLikeSearchCriteriaList[0]?.SearchGroup);
    }

    [Fact]
    public void Read_ThrowsJsonExceptionForInvalidJsonFormat()
    {
        // Arrange
        const string json = "{}"; // Not an array

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SqlLikeSearchCriteriaBuilder<Company>>(json, serializeOptions));
    }

    [Fact]
    public void Write_SerializesSqlSearchCriteriaBuilderToJsonCorrectly()
    {
        // Arrange
        var searchBuilder = new SqlLikeSearchCriteriaBuilder<Company>();
        searchBuilder.Add(new SqlLikeSearchInfo<Company>(x => x.Name!, "Alice", 2));

        // Act
        var json = JsonSerializer.Serialize(searchBuilder, serializeOptions);

        // Assert
        Assert.Equal("[{\"SearchItem\":\"Name\",\"SearchString\":\"Alice\",\"SearchGroup\":2}]", json);
    }

    [Fact]
    public void CanConvert_ReturnsTrueForCorrectTypeAndFalseForOtherTypes()
    {
        // Arrange
        var converter = new SqlSearchCriteriaBuilderJsonConverter<Company>();

        // Act
        var resultTrue = converter.CanConvert(typeof(SqlLikeSearchCriteriaBuilder<Company>));
        var resultFalse = converter.CanConvert(typeof(string));

        // Assert
        Assert.True(resultTrue);
        Assert.False(resultFalse);
    }

    [Fact]
    public void Read_DeserializesEmptyArrayToEmptyBuilder()
    {
        // Arrange
        const string json = "[]";

        // Act
        var searchBuilder = JsonSerializer.Deserialize<SqlLikeSearchCriteriaBuilder<Company>>(json, serializeOptions);

        // Assert
        Assert.NotNull(searchBuilder);
        Assert.Empty(searchBuilder.SqlLikeSearchCriteriaList);
    }

    [Fact]
    public void Read_ThrowsJsonExceptionForInvalidArrayElement()
    {
        // Arrange
        const string json = "[123]";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SqlLikeSearchCriteriaBuilder<Company>>(json, serializeOptions));
    }

    [Fact]
    public void Write_SerializesEmptyBuilderToEmptyArray()
    {
        // Arrange
        var searchBuilder = new SqlLikeSearchCriteriaBuilder<Company>();

        // Act
        var json = JsonSerializer.Serialize(searchBuilder, serializeOptions);

        // Assert
        Assert.Equal("[]", json);
    }

    [Fact]
    public void Write_SerializesBuilderWithMultipleItems()
    {
        // Arrange
        var searchBuilder = new SqlLikeSearchCriteriaBuilder<Company>();
        searchBuilder.Add(new SqlLikeSearchInfo<Company>(x => x.Name!, "Alice", 2));
        searchBuilder.Add(new SqlLikeSearchInfo<Company>(x => x.CountryId, "1", 3));

        // Act
        var json = JsonSerializer.Serialize(searchBuilder, serializeOptions);

        // Assert
        Assert.Contains("\"SearchItem\":\"Name\",\"SearchString\":\"Alice\",\"SearchGroup\":2", json);
        Assert.Contains("\"SearchItem\":\"CountryId\",\"SearchString\":\"1\",\"SearchGroup\":3", json);
    }

    [Fact]
    public void Write_ThrowsArgumentNullExceptionForNullWriter()
    {
        // Arrange
        var converter = new SqlSearchCriteriaBuilderJsonConverter<Company>();
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        var options = new JsonSerializerOptions();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => converter.Write(null!, builder, options));
    }

    [Fact]
    public void Write_ThrowsArgumentNullExceptionForNullValue()
    {
        // Arrange
        var converter = new SqlSearchCriteriaBuilderJsonConverter<Company>();
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => converter.Write(writer, null!, options));
    }

    [Fact]
    public void Write_ThrowsArgumentNullExceptionForNullOptions()
    {
        // Arrange
        var converter = new SqlSearchCriteriaBuilderJsonConverter<Company>();
        var builder = new SqlLikeSearchCriteriaBuilder<Company>();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => converter.Write(writer, builder, null!));
    }

    [Fact]
    public void RoundTrip_SerializeThenDeserialize_PreservesData()
    {
        // Arrange
        var searchBuilder = new SqlLikeSearchCriteriaBuilder<Company>();
        searchBuilder.Add(new SqlLikeSearchInfo<Company>(x => x.Name!, "Test", 5));
        searchBuilder.Add(new SqlLikeSearchInfo<Company>(x => x.CountryId, "42", 7));

        // Act
        var json = JsonSerializer.Serialize(searchBuilder, serializeOptions);
        var deserialized = JsonSerializer.Deserialize<SqlLikeSearchCriteriaBuilder<Company>>(json, serializeOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Count);
        Assert.Equal("Test", deserialized.SqlLikeSearchCriteriaList[0].SearchString);
        Assert.Equal(5, deserialized.SqlLikeSearchCriteriaList[0].SearchGroup);
        Assert.Equal("42", deserialized.SqlLikeSearchCriteriaList[1].SearchString);
        Assert.Equal(7, deserialized.SqlLikeSearchCriteriaList[1].SearchGroup);
    }

    [Fact]
    public void Read_ThrowsJsonExceptionIfNullElementInArray()
    {
        // Arrange
        const string json = "[null]";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SqlLikeSearchCriteriaBuilder<Company>>(json, serializeOptions));
    }
}
