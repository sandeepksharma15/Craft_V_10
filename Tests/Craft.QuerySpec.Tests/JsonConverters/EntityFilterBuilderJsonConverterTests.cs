using System.Text.Json;
using System.Reflection;

namespace Craft.QuerySpec.Tests.Converters;

public class EntityFilterBuilderJsonConverterTests
{
    // Test entity for filter operations
    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    private static JsonSerializerOptions GetOptions<T>() where T : class
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EntityFilterBuilderJsonConverter<T>());
        options.Converters.Add(new EntityFilterCriteriaJsonConverter<T>());
        return options;
    }

    [Fact]
    public void Serialize_EmptyBuilder_WritesEmptyArray()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        var options = GetOptions<TestEntity>();
        var json = JsonSerializer.Serialize(builder, options);
        Assert.Equal("[]", json);
    }

    [Fact]
    public void Serialize_PopulatedBuilder_WritesArrayOfCriteria()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        builder.Add(x => x.Name == "John").Add(x => x.Age > 18);
        var options = GetOptions<TestEntity>();
        var json = JsonSerializer.Serialize(builder, options);
        Assert.StartsWith("[", json);
        Assert.Contains("Name", json);
        Assert.Contains("Age", json);
    }

    [Fact]
    public void Deserialize_EmptyArrayJson_ReturnsEmptyBuilder()
    {
        var options = GetOptions<TestEntity>();
        var builder = JsonSerializer.Deserialize<EntityFilterBuilder<TestEntity>>("[]", options);
        Assert.NotNull(builder);
        Assert.Empty(builder.EntityFilterList);
    }

    [Fact]
    public void Deserialize_ValidArrayJson_ReturnsPopulatedBuilder()
    {
        var options = GetOptions<TestEntity>();
        var json = "[{\"Filter\":\"Name == \\\"John\\\"\"},{\"Filter\":\"Age > 18\"}]";
        var builder = JsonSerializer.Deserialize<EntityFilterBuilder<TestEntity>>(json, options);
        Assert.NotNull(builder);
        Assert.Equal(2, builder.EntityFilterList.Count);
        Assert.Contains(builder.EntityFilterList, c => c.Filter.ToString().Contains("Name"));
        Assert.Contains(builder.EntityFilterList, c => c.Filter.ToString().Contains("Age"));
    }

    [Fact]
    public void Deserialize_InvalidJson_NotArray_ThrowsJsonException()
    {
        var options = GetOptions<TestEntity>();
        var json = "{\"Filter\":\"Name == 'John'\"}";
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<EntityFilterBuilder<TestEntity>>(json, options));
    }

    [Fact]
    public void Deserialize_InvalidCriteria_ThrowsJsonException()
    {
        var options = GetOptions<TestEntity>();
        var json = "[123]"; // Not a valid criteria object
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<EntityFilterBuilder<TestEntity>>(json, options));
    }

    [Fact]
    public void SerializeDeserialize_RoundTrip_PreservesCriteria()
    {
        var builder = new EntityFilterBuilder<TestEntity>();
        builder.Add(x => x.Name == "John").Add(x => x.Age > 18);
        var options = GetOptions<TestEntity>();
        var json = JsonSerializer.Serialize(builder, options);
        var deserialized = JsonSerializer.Deserialize<EntityFilterBuilder<TestEntity>>(json, options);
        Assert.NotNull(deserialized);
        Assert.Equal(builder.EntityFilterList.Count, deserialized.EntityFilterList.Count);
        for (int i = 0; i < builder.EntityFilterList.Count; i++)
            Assert.Equal(builder.EntityFilterList[i].Filter.ToString(), deserialized.EntityFilterList[i].Filter.ToString());
    }

    [Fact]
    public void Write_NullWriter_ThrowsArgumentNullException()
    {
        var converter = new EntityFilterBuilderJsonConverter<TestEntity>();
        var builder = new EntityFilterBuilder<TestEntity>();
        var options = GetOptions<TestEntity>();
        Assert.Throws<ArgumentNullException>(() =>
            converter.Write(null!, builder, options));
    }

    [Fact]
    public void Write_NullValue_ThrowsArgumentNullException()
    {
        var converter = new EntityFilterBuilderJsonConverter<TestEntity>();
        var options = GetOptions<TestEntity>();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        Assert.Throws<ArgumentNullException>(() =>
            converter.Write(writer, null!, options));
    }

    [Fact]
    public void CreateLocalOptions_NullOptions_ThrowsArgumentNullException()
    {
        var type = typeof(EntityFilterBuilderJsonConverter<TestEntity>);
        var method = type.GetMethod("CreateLocalOptions", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var ex = Assert.Throws<TargetInvocationException>(() => method!.Invoke(null, [null]));
        Assert.IsType<ArgumentNullException>(ex.InnerException);
    }

    [Fact]
    public void CanConvert_ReturnsTrueForCorrectType()
    {
        var converter = new EntityFilterBuilderJsonConverter<TestEntity>();
        Assert.True(converter.CanConvert(typeof(EntityFilterBuilder<TestEntity>)));
    }

    [Fact]
    public void CanConvert_ReturnsFalseForIncorrectType()
    {
        var converter = new EntityFilterBuilderJsonConverter<TestEntity>();
        Assert.False(converter.CanConvert(typeof(string)));
    }
}
