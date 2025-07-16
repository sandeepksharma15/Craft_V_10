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
}
