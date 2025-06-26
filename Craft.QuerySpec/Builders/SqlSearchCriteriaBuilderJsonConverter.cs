using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.QuerySpec;

public class SqlSearchCriteriaBuilderJsonConverter<T> : JsonConverter<SqlLikeSearchCriteriaBuilder<T>> where T : class
{
    public override bool CanConvert(Type objectType)
        => objectType == typeof(SqlLikeSearchCriteriaBuilder<T>);

    public override SqlLikeSearchCriteriaBuilder<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var searchBuilder = new SqlLikeSearchCriteriaBuilder<T>();

        // We Want To Clone The Options To Add The SqlLikeSearchInfoJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new SqlLikeSearchInfoJsonConverter<T>());

        // Check for array start
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Invalid format for SqlSearchCriteriaBuilder: expected array of SqlLikeSearchInfo");

        // Read each order expression
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            // Read the individual SqlLikeSearchInfo object
            var searchInfo = JsonSerializer.Deserialize<SqlLikeSearchInfo<T>>(ref reader, localOptions);

            // Validate and add the order expression
            if (searchInfo != null)
                searchBuilder.Add(searchInfo);
            else
                throw new JsonException("Invalid SqlLikeSearchInfo encountered in SqlSearchCriteriaBuilder array");
        }

        return searchBuilder;
    }

    public override void Write(Utf8JsonWriter writer, SqlLikeSearchCriteriaBuilder<T> value, JsonSerializerOptions options)
    {
        // Start The Array
        writer.WriteStartArray();

        // We Want To Clone The Options To Add The SqlLikeSearchInfoJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new SqlLikeSearchInfoJsonConverter<T>());

        foreach (var searchInfo in value.SqlLikeSearchCriteriaList)
        {
            var json = JsonSerializer.Serialize(searchInfo, localOptions);
            writer.WriteRawValue(json);
        }

        // End the array
        writer.WriteEndArray();
    }
}
