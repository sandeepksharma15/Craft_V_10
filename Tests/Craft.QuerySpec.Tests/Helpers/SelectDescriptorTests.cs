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

        // Act
        var selectInfo = JsonSerializer.Deserialize<SelectDescriptor<Company, MyResult>>(json, CachedJsonSerializerOptions);

        // Assert
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

    [Fact]
    public void Constructor_NullAssignor_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SelectDescriptor<Company, MyResult>((LambdaExpression)null!));
    }

    [Fact]
    public void Constructor_NonMemberAssignor_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<Company, string>> expr = x => x.ToString();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Company, MyResult>(expr));
    }

    [Fact]
    public void Constructor_NullAssignorOrAssignee_ThrowsArgumentNullException()
    {
        // Arrange
        Expression<Func<Company, string>> assignor = x => x.Name!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SelectDescriptor<Company, MyResult>(null!, assignor));
        Assert.Throws<ArgumentNullException>(() => new SelectDescriptor<Company, MyResult>(assignor, null!));
    }

    [Fact]
    public void Constructor_NonMemberAssignee_ThrowsArgumentException()
    {
        // Arrange
        Expression<Func<Company, string>> assignor = x => x.Name!;
        Expression<Func<MyResult, string>> assignee = x => x.ToString()!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Company, MyResult>(assignor, assignee));
    }

    [Fact]
    public void Constructor_AssignorPropertyName_NullOrWhitespace_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SelectDescriptor<Company, MyResult>((string)null!));
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Company, MyResult>(" "));
    }

    [Fact]
    public void Constructor_AssignorPropertyName_DoesNotExist_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Company, MyResult>("NonExistentProperty"));
    }

    [Fact]
    public void Constructor_AssignorAndAssigneePropertyName_NullOrWhitespace_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SelectDescriptor<Company, MyResult>(null!, "ResultName"));
        Assert.Throws<ArgumentNullException>(() => new SelectDescriptor<Company, MyResult>("Name", null!));
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Company, MyResult>(" ", "ResultName"));
        Assert.Throws<ArgumentException>(() => new SelectDescriptor<Company, MyResult>("Name", " "));
    }

    [Fact]
    public void InternalConstructor_ForSerialization_DoesNotThrow()
    {
        // Arrange
        var ctor = typeof(SelectDescriptor<Company, MyResult>).GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, Type.EmptyTypes, null);

        // Act
        var instance = ctor?.Invoke(null);

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public void JsonConverter_Read_WithMissingProperties_DoesNotThrowAndPropertiesAreNull()
    {
        // Arrange
        const string json = "{}";

        // Act
        var selectInfo = JsonSerializer.Deserialize<SelectDescriptor<Company, MyResult>>(json, CachedJsonSerializerOptions);

        // Assert
        Assert.NotNull(selectInfo);
        Assert.Null(selectInfo.Assignor);
        Assert.Null(selectInfo.Assignee);
    }

    [Fact]
    public void JsonConverter_Read_WithEmptyOrWhitespaceProperties_DoesNotThrowAndPropertiesAreNull()
    {
        // Arrange
        const string json = "{\"Assignor\": \"\", \"Assignee\": \" \"}";

        // Act
        var selectInfo = JsonSerializer.Deserialize<SelectDescriptor<Company, MyResult>>(json, CachedJsonSerializerOptions);

        // Assert
        Assert.NotNull(selectInfo);
        Assert.Null(selectInfo.Assignor);
        Assert.Null(selectInfo.Assignee);
    }

    [Fact]
    public void JsonConverter_Write_WithNullAssignorOrAssignee_WritesNulls()
    {
        // Arrange
        var ctor = typeof(SelectDescriptor<Company, MyResult>).GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        var emptyInfo = (SelectDescriptor<Company, MyResult>)ctor!.Invoke(null);

        // Act
        var json = JsonSerializer.Serialize(emptyInfo, CachedJsonSerializerOptions);

        // Assert
        Assert.Contains("\"Assignor\":null", json);
        Assert.Contains("\"Assignee\":null", json);
    }
}
