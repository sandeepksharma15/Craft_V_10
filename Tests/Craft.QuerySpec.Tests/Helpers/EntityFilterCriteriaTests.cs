using System.Linq.Expressions;
using System.Text.Json;
using DocumentFormat.OpenXml.Wordprocessing;

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
    public void JsonConverter_Read_InvalidSyntax_ThrowsJsonException()
    {
        // Arrange
        const string json = "{\"Filter\": \"InvalidSyntax\""; // Missing expression body

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EntityFilterCriteria<MyEntity>>(json, options));
    }

    [Fact]
    public void JsonConverter_Read_NullJson_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => JsonSerializer.Deserialize<EntityFilterCriteria<MyEntity>>("\"null\"", options));
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

    private class MyEntity
    {
        public string Name { get; set; } = string.Empty;
    }
}
