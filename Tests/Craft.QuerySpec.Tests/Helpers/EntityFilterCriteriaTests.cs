using System.Linq.Expressions;
using System.Text.Json;

namespace Craft.QuerySpec.Tests.Helpers;

public class EntityFilterCriteriaTests
{
    private readonly JsonSerializerOptions options;

    public EntityFilterCriteriaTests()
    {
        options = new JsonSerializerOptions();
        options.Converters.Add(new EntityFilterCriteriaJsonConverter<MyEntity>());
    }

    [Fact]
    public void Constructor_WithValidFilter_InitializesProperties()
    {
        // Arrange
        Expression<Func<string, bool>> filterExpression = s => s.Length > 5;

        // Act
        var whereInfo = new EntityFilterCriteria<string>(filterExpression);

        // Assert
        Assert.Equal(filterExpression.ToString(), whereInfo.Filter.ToString());
        Assert.True(whereInfo.FilterFunc.Invoke("TestString"));
    }

    [Fact]
    public void Serialization_RoundTrip_ReturnsEqualWhereInfo()
    {
        // Arrange
        Expression<Func<MyEntity, bool>> filterExpression = x => x.Name == "John";

        // Act
        var whereInfo = new EntityFilterCriteria<MyEntity>(filterExpression);
        var entity = new MyEntity { Name = "John" };
        var serializationInfo = JsonSerializer.Serialize(whereInfo, options);
        var deserializedWhereInfo = JsonSerializer.Deserialize<EntityFilterCriteria<MyEntity>>(serializationInfo, options);

        // Assert
        Assert.NotNull(deserializedWhereInfo);
        Assert.Equal(filterExpression.ToString(), deserializedWhereInfo.Filter.ToString());
        Assert.True(deserializedWhereInfo.FilterFunc(entity));
    }

    [Fact]
    public void Constructor_NullFilter_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EntityFilterCriteria<MyEntity>(null));
    }

    [Fact]
    public void Constructor_ValidFilter_CompilesCorrectly()
    {
        // Arrange
        Expression<Func<MyEntity, bool>> filter = x => x.Name == "John";

        // Act
        var whereInfo = new EntityFilterCriteria<MyEntity>(filter);
        var entity = new MyEntity { Name = "John" };

        // Assert
        Assert.True(whereInfo.FilterFunc(entity));
    }

    [Fact]
    public void Matches_MatchingEntity_ReturnsTrue()
    {
        // Arrange
        Expression<Func<MyEntity, bool>> filter = x => x.Name == "John";

        // Act
        var whereInfo = new EntityFilterCriteria<MyEntity>(filter);
        var entity = new MyEntity { Name = "John" };

        // Assert
        Assert.True(whereInfo.Matches(entity));
    }

    [Fact]
    public void Matches_NonMatchingEntity_ReturnsFalse()
    {
        // Arrange
        Expression<Func<MyEntity, bool>> filter = x => x.Name == "John";

        // Act
        var whereInfo = new EntityFilterCriteria<MyEntity>(filter);
        var entity = new MyEntity { Name = "Jane" };

        // Assert
        Assert.False(whereInfo.Matches(entity));
    }

    [Fact]
    public void ToString_ReturnsExpectedString()
    {
        // Arrange
        Expression<Func<MyEntity, bool>> filter = x => x.Name == "Test";
        var criteria = new EntityFilterCriteria<MyEntity>(filter);

        // Act
        var result = criteria.ToString();

        // Assert
        Assert.Equal(filter.ToString(), result);
    }

    [Fact]
    public void Matches_NullEntity_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<MyEntity, bool>> filter = x => x.Name == "Test";
        var criteria = new EntityFilterCriteria<MyEntity>(filter);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => criteria.Matches(null!));
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "JSON001:Invalid JSON pattern", Justification = "Test Case")]
    public void JsonConverter_Read_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"Filter\": {"; // Missing closing brace

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityFilterCriteria<MyEntity>>(json, options));
    }

    [Fact]
    public void JsonConverter_Read_InvalidProperty_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"InvalidProperty\": \"Name == 'John'\"}";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityFilterCriteria<MyEntity>>(json, options));
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "JSON001:Invalid JSON pattern", Justification = "Test Case")]
    public void JsonConverter_Read_InvalidSyntax_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"Filter\": \"InvalidSyntax\""; // Missing expression body

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityFilterCriteria<MyEntity>>(json, options));
    }

    [Fact]
    public void JsonConverter_Read_NullJson_ThrowsJsonException()
    {
        // Arrange & Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityFilterCriteria<MyEntity>>("\"null\"", options));
    }

    [Fact]
    public void JsonConverter_Read_EmptyFilterString_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"Filter\":\"\"}";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityFilterCriteria<MyEntity>>(json, options));
    }

    [Fact]
    public void JsonConverter_Read_WhitespaceFilterString_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"Filter\":\"   \"}";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityFilterCriteria<MyEntity>>(json, options));
    }

    [Fact]
    public void JsonConverter_Read_ValidJson_ConstructsCorrectly()
    {
        // Arrange
        Expression<Func<MyEntity, bool>> filterExpression = x => x.Name == "John";
        const string json = "{\"Filter\": \"Name == 'John'\"}";

        // Act
        var result = JsonSerializer.Deserialize<EntityFilterCriteria<MyEntity>>(json, options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(filterExpression.ToString(), result.Filter.ToString());
    }

    [Fact]
    public void JsonConverter_Write_WritesExpectedJson()
    {
        // Arrange
        Expression<Func<MyEntity, bool>> filterExpression = x => x.Name == "John";

        // Act
        var whereInfo = new EntityFilterCriteria<MyEntity>(filterExpression);
        var json = JsonSerializer.Serialize(whereInfo, options);

        // Assert
        Assert.Contains("{\"Filter\":\"(Name == \\u0022John\\u0022)\"}", json);
    }

    [Fact]
    public void JsonConverter_Write_NullValue_WritesJsonNull()
    {
        // Arrange
        var converter = new EntityFilterCriteriaJsonConverter<MyEntity>();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, null!, options);
        writer.Flush();
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        // Assert
        Assert.Equal("null", json);
    }

    private class MyEntity
    {
        public string Name { get; set; } = string.Empty;
    }
}
