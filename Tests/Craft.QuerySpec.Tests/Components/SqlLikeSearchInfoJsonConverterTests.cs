using System.Linq.Expressions;
using System.Text.Json;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec.Tests.Components;

public class SqlLikeSearchInfoJsonConverterTests
{
    private static readonly JsonSerializerOptions CachedJsonSerializerOptions = CreateJsonSerializerOptions();

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new SqlLikeSearchInfoJsonConverter<MyResult>());
        return options;
    }

    private class MyResult
    {
        public long Id { get; set; }
        public string ResultName { get; set; } = string.Empty;
    }

    [Fact]
    public void Serialize_AllPropertiesSet_ProducesExpectedJson()
    {
        var info = new SqlLikeSearchInfo<MyResult>(x => x.ResultName, "abc", 5);
        var json = JsonSerializer.Serialize(info, CachedJsonSerializerOptions);
        Assert.Contains("\"SearchItem\":\"ResultName\"", json);
        Assert.Contains("\"SearchString\":\"abc\"", json);
        Assert.Contains("\"SearchGroup\":5", json);
    }

    [Fact]
    public void Serialize_NullSearchItem_ProducesNullSearchItemInJson()
    {
        var info = (SqlLikeSearchInfo<MyResult>)Activator.CreateInstance(typeof(SqlLikeSearchInfo<MyResult>), true)!;
        info.SearchString = "abc";
        info.SearchGroup = 2;
        info.SearchItem = null;
        var json = JsonSerializer.Serialize(info, CachedJsonSerializerOptions);
        Assert.Contains("\"SearchItem\":null", json);
        Assert.Contains("\"SearchString\":\"abc\"", json);
        Assert.Contains("\"SearchGroup\":2", json);
    }

    [Fact]
    public void Serialize_NullSearchString_ProducesNullSearchStringInJson()
    {
        var info = (SqlLikeSearchInfo<MyResult>)Activator.CreateInstance(typeof(SqlLikeSearchInfo<MyResult>), true)!;
        info.SearchItem = "ResultName".CreateMemberExpression<MyResult, object>();
        info.SearchString = null;
        info.SearchGroup = 3;
        var json = JsonSerializer.Serialize(info, CachedJsonSerializerOptions);
        Assert.Contains("\"SearchString\":null", json);
        Assert.Contains("\"SearchGroup\":3", json);
    }

    [Fact]
    public void Serialize_DefaultSearchGroup_ProducesZeroInJson()
    {
        var info = (SqlLikeSearchInfo<MyResult>)Activator.CreateInstance(typeof(SqlLikeSearchInfo<MyResult>), true)!;
        info.SearchItem = "ResultName".CreateMemberExpression<MyResult, object>();
        info.SearchString = "abc";
        info.SearchGroup = 0;
        var json = JsonSerializer.Serialize(info, CachedJsonSerializerOptions);
        Assert.Contains("\"SearchGroup\":0", json);
    }

    [Fact]
    public void Deserialize_AllPropertiesSet_DeserializesCorrectly()
    {
        var json = "{\"SearchItem\":\"ResultName\",\"SearchString\":\"abc\",\"SearchGroup\":7}";
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);
        Assert.NotNull(info);
        Assert.NotNull(info!.SearchItem);
        Assert.Equal("abc", info.SearchString);
        Assert.Equal(7, info.SearchGroup);
        Assert.Equal("ResultName", info.SearchItem?.Body is MemberExpression m ? m.Member.Name : null);
    }

    [Fact]
    public void Deserialize_MissingProperties_DefaultsApplied()
    {
        var json = "{\"SearchString\":\"abc\"}";
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);
        Assert.NotNull(info);
        Assert.Null(info!.SearchItem);
        Assert.Equal("abc", info.SearchString);
        Assert.Equal(0, info.SearchGroup);
    }

    [Fact]
    public void Deserialize_NullSearchItem_DeserializesAsNull()
    {
        var json = "{\"SearchItem\":null,\"SearchString\":\"abc\",\"SearchGroup\":1}";
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);
        Assert.NotNull(info);
        Assert.Null(info!.SearchItem);
        Assert.Equal("abc", info.SearchString);
        Assert.Equal(1, info.SearchGroup);
    }

    [Fact]
    public void Deserialize_NullSearchString_DeserializesAsNull()
    {
        var json = "{\"SearchItem\":\"ResultName\",\"SearchString\":null,\"SearchGroup\":1}";
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);
        Assert.NotNull(info);
        Assert.Equal("ResultName", info!.SearchItem?.Body is MemberExpression m ? m.Member.Name : null);
        Assert.Null(info.SearchString);
        Assert.Equal(1, info.SearchGroup);
    }

    [Fact]
    public void Deserialize_ZeroSearchGroup_DeserializesAsZero()
    {
        var json = "{\"SearchItem\":\"ResultName\",\"SearchString\":\"abc\",\"SearchGroup\":0}";
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);
        Assert.NotNull(info);
        Assert.Equal(0, info!.SearchGroup);
    }

    [Fact]
    public void Deserialize_UnknownProperty_Ignored()
    {
        var json = "{\"SearchItem\":\"ResultName\",\"SearchString\":\"abc\",\"SearchGroup\":1,\"Unknown\":123}";
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);
        Assert.NotNull(info);
        Assert.Equal("abc", info!.SearchString);
        Assert.Equal(1, info.SearchGroup);
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsJsonException()
    {
        var json = "\"notanobject\"";
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions));
        Assert.Contains("Expected start of object", ex.Message);
    }

    [Fact]
    public void Deserialize_NullJson_ReturnsNull()
    {
        var json = "null";
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);
        Assert.Null(info);
    }

    [Fact]
    public void Deserialize_InvalidMemberName_ThrowsArgumentException()
    {
        var json = "{\"SearchItem\":\"DoesNotExist\",\"SearchString\":\"abc\",\"SearchGroup\":1}";
        var ex = Assert.Throws<ArgumentException>(() => JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions));
        Assert.Contains("Failed to create member expression", ex.Message);
    }

    [Fact]
    public void RoundTrip_SerializeDeserialize_ProducesEquivalentObject()
    {
        var original = new SqlLikeSearchInfo<MyResult>(x => x.ResultName, "abc", 3);
        var json = JsonSerializer.Serialize(original, CachedJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);
        Assert.NotNull(deserialized);
        Assert.Equal("abc", deserialized!.SearchString);
        Assert.Equal(3, deserialized.SearchGroup);
        Assert.Equal("ResultName", deserialized.SearchItem?.Body is MemberExpression m ? m.Member.Name : null);
    }

    [Fact]
    public void Deserialize_EmptyObject_AllDefaults()
    {
        var json = "{}";
        var info = JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions);
        Assert.NotNull(info);
        Assert.Null(info!.SearchItem);
        Assert.Null(info.SearchString);
        Assert.Equal(0, info.SearchGroup);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "JSON001:Invalid JSON pattern", Justification = "<Pending>")]
    public void Deserialize_UnexpectedToken_ThrowsJsonException()
    {
        // Malformed JSON: property name not followed by value
        var json = "{\"SearchItem\":}";
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SqlLikeSearchInfo<MyResult>>(json, CachedJsonSerializerOptions));
        Assert.Contains("invalid start", ex.Message);
    }
}
