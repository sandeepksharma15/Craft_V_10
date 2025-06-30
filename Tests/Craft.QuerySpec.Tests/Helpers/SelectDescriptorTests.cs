using System.Linq.Expressions;
using System.Text.Json;
using Craft.TestDataStore.Models;

namespace Craft.QuerySpec.Tests.Helpers;

public class SelectDescriptorTests
{
    private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new()
    {
        Converters = { new SelectDescriptorJsonConverter<Company, MyResult>() }
    };

    private class MyResult
    {
        public long Id { get; set; }
        public string ResultName { get; set; } = string.Empty;
    }

    [Fact]
    public void Serialization_RoundTrip_ShouldPreserveSelectItem()
    {
        // Arrange
        var assignorExpression = (Expression<Func<Company, string>>)(x => x.Name!);
        var assigneeExpression = (Expression<Func<MyResult, string>>)(x => x.ResultName!);
        var selectInfo = new SelectDescriptor<Company, MyResult>(assignorExpression, assigneeExpression);

        // Act
        var serializationInfo = JsonSerializer.Serialize(selectInfo, CachedJsonSerializerOptions);
        var deserializedSelectInfo = JsonSerializer.Deserialize<SelectDescriptor<Company, MyResult>>(serializationInfo, CachedJsonSerializerOptions);

        // Assert
        Assert.NotNull(deserializedSelectInfo);
        Assert.Equal(assignorExpression.ToString(), deserializedSelectInfo.Assignor?.ToString());
        Assert.Equal(assigneeExpression.ToString(), deserializedSelectInfo.Assignee?.ToString());
    }

    [Fact]
    public void JsonConverter_Read_WithNullJson_ReturnsNull()
    {
        // Arrange
        const string json = "null";

        // Act & Assert
        var selectInfo = JsonSerializer.Deserialize<SelectDescriptor<Company, MyResult>>(json, CachedJsonSerializerOptions);
        Assert.Null(selectInfo);
    }

    [Fact]
    public void JsonConverter_Read_WithInvalidMemberExpression_ThrowsException()
    {
        // Arrange
        const string json = "{\"Assignor\": \"InvalidMember\", \"Assignee\": \"ResultName\"}";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => JsonSerializer.Deserialize<SelectDescriptor<Company, MyResult>>(json, CachedJsonSerializerOptions));
    }
}
