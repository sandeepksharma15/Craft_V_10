using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Craft.QuerySpec.Tests.Components;

public class SelectDescriptorJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public class Source
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class Dest
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class DestNoName
    {
        public int Age { get; set; }
    }

    public SelectDescriptorJsonConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new SelectDescriptorJsonConverter<Source, Dest>());
    }

    [Fact]
    public void Write_SerializesAssignorAndAssigneeCorrectly()
    {
        // Arrange
        var desc = new SelectDescriptor<Source, Dest>("Name");
        // Act
        var json = JsonSerializer.Serialize(desc, _options);
        // Assert
        Assert.Contains("Assignor", json);
        Assert.Contains("Assignee", json);
        Assert.Contains("Name", json);
    }

    [Fact]
    public void Write_SerializesWithNullAssignorAndAssignee()
    {
        // Arrange
        var ctor = typeof(SelectDescriptor<Source, Dest>).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
        var desc = (SelectDescriptor<Source, Dest>)ctor!.Invoke(null);
        // Act
        var json = JsonSerializer.Serialize(desc, _options);
        // Assert
        Assert.Contains("Assignor", json);
        Assert.Contains("Assignee", json);
        Assert.Contains("null", json);
    }

    [Fact]
    public void Write_WithNullValue_WritesJsonWithNulls()
    {
        // Act
        var json = JsonSerializer.Serialize<SelectDescriptor<Source, Dest>>(null!, _options);
        // Assert
        Assert.Equal("null", json);
    }

    [Fact]
    public void Read_DeserializesAssignorAndAssigneeCorrectly()
    {
        // Arrange
        var json = "{\"Assignor\":\"Name\",\"Assignee\":\"Name\"}";
        // Act
        var desc = JsonSerializer.Deserialize<SelectDescriptor<Source, Dest>>(json, _options);
        // Assert
        Assert.NotNull(desc);
        Assert.NotNull(desc!.Assignor);
        Assert.NotNull(desc.Assignee);
        Assert.Equal("Name", ((MemberExpression)desc.Assignor!.Body).Member.Name);
        Assert.Equal("Name", ((MemberExpression)desc.Assignee!.Body).Member.Name);
    }

    [Fact]
    public void Read_DeserializesWithOnlyAssignor()
    {
        // Arrange
        var json = "{\"Assignor\":\"Name\"}";
        // Act
        var desc = JsonSerializer.Deserialize<SelectDescriptor<Source, Dest>>(json, _options);
        // Assert
        Assert.NotNull(desc);
        Assert.NotNull(desc!.Assignor);
        Assert.Equal("Name", ((MemberExpression)desc.Assignor!.Body).Member.Name);
        Assert.Null(desc.Assignee);
    }

    [Fact]
    public void Read_DeserializesWithOnlyAssignee()
    {
        // Arrange
        var json = "{\"Assignee\":\"Name\"}";
        // Act
        var desc = JsonSerializer.Deserialize<SelectDescriptor<Source, Dest>>(json, _options);
        // Assert
        Assert.NotNull(desc);
        Assert.Null(desc!.Assignor);
        Assert.NotNull(desc.Assignee);
        Assert.Equal("Name", ((MemberExpression)desc.Assignee!.Body).Member.Name);
    }

    [Fact]
    public void Read_DeserializesWithNullAssignorAndAssignee()
    {
        // Arrange
        var json = "{\"Assignor\":null,\"Assignee\":null}";
        // Act
        var desc = JsonSerializer.Deserialize<SelectDescriptor<Source, Dest>>(json, _options);
        // Assert
        Assert.NotNull(desc);
        Assert.Null(desc!.Assignor);
        Assert.Null(desc.Assignee);
    }

    [Fact]
    public void Read_DeserializesWithMissingProperties()
    {
        // Arrange
        var json = "{}";
        // Act
        var desc = JsonSerializer.Deserialize<SelectDescriptor<Source, Dest>>(json, _options);
        // Assert
        Assert.NotNull(desc);
        Assert.Null(desc!.Assignor);
        Assert.Null(desc.Assignee);
    }

    [Fact]
    public void Read_DeserializesWithEmptyOrWhitespaceProperties()
    {
        // Arrange
        var json = "{\"Assignor\":\"\",\"Assignee\":\"  \"}";
        // Act
        var desc = JsonSerializer.Deserialize<SelectDescriptor<Source, Dest>>(json, _options);
        // Assert
        Assert.NotNull(desc);
        Assert.Null(desc!.Assignor);
        Assert.Null(desc.Assignee);
    }

    [Fact]
    public void Read_DeserializesWithUnknownProperty_IgnoresIt()
    {
        // Arrange
        var json = "{\"Unknown\":\"Value\",\"Assignor\":\"Name\"}";
        // Act
        var desc = JsonSerializer.Deserialize<SelectDescriptor<Source, Dest>>(json, _options);
        // Assert
        Assert.NotNull(desc);
        Assert.NotNull(desc!.Assignor);
        Assert.Equal("Name", ((MemberExpression)desc.Assignor!.Body).Member.Name);
    }

    [Fact]
    public void Read_WithNullToken_ReturnsNull()
    {
        // Arrange
        var json = "null";
        // Act
        var desc = JsonSerializer.Deserialize<SelectDescriptor<Source, Dest>>(json, _options);
        // Assert
        Assert.Null(desc);
    }

    [Fact]
    public void SetProperty_ThrowsIfPropertyNotFound()
    {
        // Arrange
        var ctor = typeof(SelectDescriptor<Source, Dest>).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
        var desc = (SelectDescriptor<Source, Dest>)ctor!.Invoke(null);
        var converter = new SelectDescriptorJsonConverter<Source, Dest>();

        // Use reflection to call private SetProperty

        var method = typeof(SelectDescriptorJsonConverter<Source, Dest>).GetMethod("SetProperty", BindingFlags.NonPublic | BindingFlags.Static);

        // Act & Assert
        var ex = Assert.Throws<TargetInvocationException>(() => method!.Invoke(null, [desc, "NotAProp", "value"]));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("Property 'NotAProp' not found", ex.InnerException!.Message);
    }
}
