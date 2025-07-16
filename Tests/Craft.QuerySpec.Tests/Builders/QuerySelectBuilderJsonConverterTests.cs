using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Craft.QuerySpec;

namespace Craft.QuerySpec.Tests.Builders;

public class QuerySelectBuilderJsonConverterTests
{
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
        options.Converters.Add(new SelectDescriptorJsonConverter<Source, Dest>());
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
        var builder = new QuerySelectBuilder<Source, Dest>();
        var options = GetOptions<Source, Dest>();
        var json = JsonSerializer.Serialize(builder, options);
        Assert.Equal("[]", json);
    }

    [Fact]
    public void Write_SerializesPopulatedBuilderToArray()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        builder.Add("Name", "Name");
        builder.Add("Age", "Age");
        var options = GetOptions<Source, Dest>();
        var json = JsonSerializer.Serialize(builder, options);
        Assert.StartsWith("[", json);
        Assert.Contains("Name", json);
        Assert.Contains("Age", json);
    }

    [Fact]
    public void Write_ThrowsOnNullWriter()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        var options = GetOptions<Source, Dest>();
        var converter = new QuerySelectBuilderJsonConverter<Source, Dest>();
        Assert.Throws<ArgumentNullException>(() => converter.Write(null!, builder, options));
    }

    [Fact]
    public void Write_ThrowsOnNullValue()
    {
        var options = GetOptions<Source, Dest>();
        var converter = new QuerySelectBuilderJsonConverter<Source, Dest>();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        Assert.Throws<ArgumentNullException>(() => converter.Write(writer, null!, options));
    }

    [Fact]
    public void Write_ThrowsOnNullOptions()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        var converter = new QuerySelectBuilderJsonConverter<Source, Dest>();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        Assert.Throws<ArgumentNullException>(() => converter.Write(writer, builder, null!));
    }

    [Fact]
    public void Read_DeserializesEmptyArrayToEmptyBuilder()
    {
        var options = GetOptions<Source, Dest>();
        var builder = JsonSerializer.Deserialize<QuerySelectBuilder<Source, Dest>>("[]", options);
        Assert.NotNull(builder);
        Assert.Empty(builder!.SelectDescriptorList);
    }

    [Fact]
    public void Read_DeserializesValidArrayToPopulatedBuilder()
    {
        var options = GetOptions<Source, Dest>();
        var json = "[{\"Assignor\":\"Name\",\"Assignee\":\"Name\"},{\"Assignor\":\"Age\",\"Assignee\":\"Age\"}]";
        var builder = JsonSerializer.Deserialize<QuerySelectBuilder<Source, Dest>>(json, options);
        Assert.NotNull(builder);
        Assert.Equal(2, builder!.SelectDescriptorList.Count);
        Assert.Equal("Name", ((System.Linq.Expressions.MemberExpression)builder.SelectDescriptorList[0].Assignor!.Body).Member.Name);
        Assert.Equal("Age", ((System.Linq.Expressions.MemberExpression)builder.SelectDescriptorList[1].Assignor!.Body).Member.Name);
    }

    [Fact]
    public void Read_ThrowsIfNotArray()
    {
        var options = GetOptions<Source, Dest>();
        var json = "{\"Assignor\":\"Name\"}";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QuerySelectBuilder<Source, Dest>>(json, options));
    }

    [Fact]
    public void Read_ThrowsIfDescriptorIsInvalid()
    {
        var options = GetOptions<Source, Dest>();
        var json = "[123]"; // Not a valid object
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<QuerySelectBuilder<Source, Dest>>(json, options));
    }

    [Fact]
    public void CanConvert_ReturnsTrueForCorrectType()
    {
        var converter = new QuerySelectBuilderJsonConverter<Source, Dest>();
        Assert.True(converter.CanConvert(typeof(QuerySelectBuilder<Source, Dest>)));
    }

    [Fact]
    public void CanConvert_ReturnsFalseForIncorrectType()
    {
        var converter = new QuerySelectBuilderJsonConverter<Source, Dest>();
        Assert.False(converter.CanConvert(typeof(string)));
    }

    [Fact]
    public void SerializeDeserialize_RoundTrip_PreservesData()
    {
        var builder = new QuerySelectBuilder<Source, Dest>();
        builder.Add("Name", "Name");
        builder.Add("Age", "Age");
        var options = GetOptions<Source, Dest>();
        var json = JsonSerializer.Serialize(builder, options);
        var deserialized = JsonSerializer.Deserialize<QuerySelectBuilder<Source, Dest>>(json, options);
        Assert.NotNull(deserialized);
        Assert.Equal(builder.SelectDescriptorList.Count, deserialized.SelectDescriptorList.Count);
        for (int i = 0; i < builder.SelectDescriptorList.Count; i++)
        {
            Assert.Equal(((MemberExpression)builder.SelectDescriptorList[i].Assignor!.Body).Member.Name,
                         ((MemberExpression)deserialized.SelectDescriptorList[i].Assignor!.Body).Member.Name);
        }
    }
}
