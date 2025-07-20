using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.QuerySpec.Tests.Converters;

public class QuerySelectBuilderJsonConverterTests
{
    // Use a single set of test classes for all tests
    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }
    public class TestResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private static JsonSerializerOptions GetOptions<T, TResult>()
        where T : class
        where TResult : class
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new QuerySelectBuilderJsonConverter<T, TResult>());
        options.Converters.Add(new SelectDescriptorJsonConverter<T, TResult>());
        return options;
    }

    [Fact]
    public void GetClone_ClonesAllPropertiesAndConverters()
    {
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            DefaultBufferSize = 123,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            IncludeFields = true,
            MaxDepth = 10,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        options.Converters.Add(new SelectDescriptorJsonConverter<TestEntity, TestResult>());
        var clone = options.GetClone();
        Assert.NotSame(options, clone);
        Assert.Equal(options.AllowTrailingCommas, clone.AllowTrailingCommas);
        Assert.Equal(options.DefaultBufferSize, clone.DefaultBufferSize);
        Assert.Equal(options.IgnoreReadOnlyFields, clone.IgnoreReadOnlyFields);
        Assert.Equal(options.IgnoreReadOnlyProperties, clone.IgnoreReadOnlyProperties);
        Assert.Equal(options.IncludeFields, clone.IncludeFields);
        Assert.Equal(options.MaxDepth, clone.MaxDepth);
        Assert.Equal(options.NumberHandling, clone.NumberHandling);
        Assert.Equal(options.PropertyNameCaseInsensitive, clone.PropertyNameCaseInsensitive);
        Assert.Equal(options.WriteIndented, clone.WriteIndented);
        Assert.Equal(options.Converters.Count, clone.Converters.Count);
    }

    [Fact]
    public void GetClone_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => JsonSerializerOptionsExtensions.GetClone(null!));
    }

    [Fact]
    public void Write_SerializesEmptyBuilderToEmptyArray()
    {
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        var options = GetOptions<TestEntity, TestResult>();
        var json = JsonSerializer.Serialize(builder, options);
        Assert.Equal("[]", json);
    }

    [Fact]
    public void Write_SerializesPopulatedBuilderToArray()
    {
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        builder.Add("Name", "Name");
        builder.Add("Age", "Age");
        var options = GetOptions<TestEntity, TestResult>();
        var json = JsonSerializer.Serialize(builder, options);
        Assert.StartsWith("[", json);
        Assert.Contains("Name", json);
        Assert.Contains("Age", json);
    }

    [Fact]
    public void Write_ThrowsOnNullWriter()
    {
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        var options = GetOptions<TestEntity, TestResult>();
        var converter = new QuerySelectBuilderJsonConverter<TestEntity, TestResult>();
        Assert.Throws<ArgumentNullException>(() => converter.Write(null!, builder, options));
    }

    [Fact]
    public void Write_ThrowsOnNullValue()
    {
        var options = GetOptions<TestEntity, TestResult>();
        var converter = new QuerySelectBuilderJsonConverter<TestEntity, TestResult>();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        Assert.Throws<ArgumentNullException>(() => converter.Write(writer, null!, options));
    }

    [Fact]
    public void Write_ThrowsOnNullOptions()
    {
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        var converter = new QuerySelectBuilderJsonConverter<TestEntity, TestResult>();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        Assert.Throws<ArgumentNullException>(() => converter.Write(writer, builder, null!));
    }

    [Fact]
    public void Read_DeserializesEmptyArrayToEmptyBuilder()
    {
        var options = GetOptions<TestEntity, TestResult>();
        var builder = JsonSerializer.Deserialize<QuerySelectBuilder<TestEntity, TestResult>>("[]", options);
        Assert.NotNull(builder);
        Assert.Empty(builder!.SelectDescriptorList);
    }

    [Fact]
    public void Read_DeserializesValidArrayToPopulatedBuilder()
    {
        var options = GetOptions<TestEntity, TestResult>();
        var json = "[{\"Assignor\":\"Name\",\"Assignee\":\"Name\"},{\"Assignor\":\"Age\",\"Assignee\":\"Age\"}]";
        var builder = JsonSerializer.Deserialize<QuerySelectBuilder<TestEntity, TestResult>>(json, options);
        Assert.NotNull(builder);
        Assert.Equal(2, builder!.SelectDescriptorList.Count);
        Assert.Equal("Name", ((MemberExpression)builder.SelectDescriptorList[0].Assignor!.Body).Member.Name);
        Assert.Equal("Age", ((MemberExpression)builder.SelectDescriptorList[1].Assignor!.Body).Member.Name);
    }

    [Fact]
    public void Read_ThrowsIfNotArray()
    {
        var options = GetOptions<TestEntity, TestResult>();
        var json = "{\"Assignor\":\"Name\"}";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QuerySelectBuilder<TestEntity, TestResult>>(json, options));
    }

    [Fact]
    public void Read_ThrowsIfDescriptorIsInvalid()
    {
        var options = GetOptions<TestEntity, TestResult>();
        var json = "[123]"; // Not a valid object
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QuerySelectBuilder<TestEntity, TestResult>>(json, options));
    }

    [Fact]
    public void CanConvert_ReturnsTrueForCorrectType()
    {
        var converter = new QuerySelectBuilderJsonConverter<TestEntity, TestResult>();
        Assert.True(converter.CanConvert(typeof(QuerySelectBuilder<TestEntity, TestResult>)));
    }

    [Fact]
    public void CanConvert_ReturnsFalseForIncorrectType()
    {
        var converter = new QuerySelectBuilderJsonConverter<TestEntity, TestResult>();
        Assert.False(converter.CanConvert(typeof(string)));
    }

    [Fact]
    public void SerializeDeserialize_RoundTrip_PreservesData()
    {
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        builder.Add("Name", "Name");
        builder.Add("Age", "Age");
        var options = GetOptions<TestEntity, TestResult>();
        var json = JsonSerializer.Serialize(builder, options);
        var deserialized = JsonSerializer.Deserialize<QuerySelectBuilder<TestEntity, TestResult>>(json, options);
        Assert.NotNull(deserialized);
        Assert.Equal(builder.SelectDescriptorList.Count, deserialized.SelectDescriptorList.Count);
        for (int i = 0; i < builder.SelectDescriptorList.Count; i++)
            Assert.Equal(((MemberExpression)builder.SelectDescriptorList[i].Assignor!.Body).Member.Name,
                         ((MemberExpression)deserialized.SelectDescriptorList[i].Assignor!.Body).Member.Name);
    }

    // Additional moved tests from QuerySelectBuilderTests.cs
    [Fact]
    public void Write_SerializesEntityFilterBuilderToJsonCorrectly()
    {
        // Arrange
        var querySelectBuilder = new QuerySelectBuilder<TestEntity, TestResult>();
        querySelectBuilder.Add(new SelectDescriptor<TestEntity, TestResult>("Name", "Name"));
        var options = GetOptions<TestEntity, TestResult>();
        // Act
        var json = JsonSerializer.Serialize(querySelectBuilder, options);
        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Equal("[{\"Assignor\":\"Name\",\"Assignee\":\"Name\"}]", json);
    }

    [Fact]
    public void Read_DeserializesValidJsonToEntityFilterBuilder()
    {
        // Arrange
        const string validJson = "[{\"Assignor\":\"Name\",\"Assignee\":\"Name\"}]";
        var options = GetOptions<TestEntity, TestResult>();
        // Act
        var querySelectBuilder = JsonSerializer.Deserialize<QuerySelectBuilder<TestEntity, TestResult>>(validJson, options);
        // Assert
        Assert.NotNull(querySelectBuilder);
        Assert.Equal(1, querySelectBuilder.Count);
        Assert.Single(querySelectBuilder.SelectDescriptorList);
        Assert.Contains("x.Name", querySelectBuilder.SelectDescriptorList[0].Assignee?.Body.ToString());
        Assert.Contains("x.Name", querySelectBuilder.SelectDescriptorList[0].Assignor?.Body.ToString());
    }

    [Fact]
    public void Write_SerializesEmptyQuerySelectBuilderToJsonCorrectly()
    {
        // Arrange
        var builder = new QuerySelectBuilder<TestEntity, TestResult>();
        var options = GetOptions<TestEntity, TestResult>();
        // Act
        var json = JsonSerializer.Serialize(builder, options);
        // Assert
        Assert.Equal("[]", json);
    }

    [Fact]
    public void Read_DeserializesEmptyArrayToQuerySelectBuilder()
    {
        // Arrange
        const string emptyJson = "[]";
        var options = GetOptions<TestEntity, TestResult>();
        // Act
        var builder = JsonSerializer.Deserialize<QuerySelectBuilder<TestEntity, TestResult>>(emptyJson, options);
        // Assert
        Assert.NotNull(builder);
        Assert.Empty(builder.SelectDescriptorList);
    }

    [Fact]
    public void Read_ThrowsJsonExceptionForInvalidFormat()
    {
        // Arrange
        const string invalidJson = "{}"; // Not an array
        var options = GetOptions<TestEntity, TestResult>();
        // Act & Assert
        void act() => JsonSerializer.Deserialize<QuerySelectBuilder<TestEntity, TestResult>>(invalidJson, options);
        Assert.Throws<JsonException>(act);
    }
}
