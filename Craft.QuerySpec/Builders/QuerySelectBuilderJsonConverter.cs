using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.QuerySpec;

public class QuerySelectBuilderJsonConverter<T, TResult> : JsonConverter<QuerySelectBuilder<T, TResult>>
    where T : class
    where TResult : class
{
    public override bool CanConvert(Type objectType)
        => objectType == typeof(QuerySelectBuilder<T, TResult>);

    public override QuerySelectBuilder<T, TResult> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var querySelectBuilder = new QuerySelectBuilder<T, TResult>();

        // We Want To Clone The Options To Add The SelectDescriptorJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new SelectDescriptorJsonConverter<T, TResult>());

        // Check for array start
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Invalid format for QuerySelectBuilder: expected array of SelectDescriptor");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            // Read the individual Select Descriptor object
            var selectDescriptor = JsonSerializer.Deserialize<SelectDescriptor<T, TResult>>(ref reader, localOptions);

            // Validate and add the SelectDescriptor
            if (selectDescriptor != null)
                querySelectBuilder.Add(selectDescriptor);
            else
                throw new JsonException("Invalid select descriptor encountered in QuerySelectBuilder array");
        }

        return querySelectBuilder;
    }

    public override void Write(Utf8JsonWriter writer, QuerySelectBuilder<T, TResult> value, JsonSerializerOptions options)
    {
        // Start The Array
        writer.WriteStartArray();

        // We Want To Clone The Options To Add The SelectDescriptorJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new SelectDescriptorJsonConverter<T, TResult>());

        foreach (var selectDescriptor in value.SelectDescriptorList)
        {
            var json = JsonSerializer.Serialize(selectDescriptor, localOptions);
            writer.WriteRawValue(json);
        }

        // End the array
        writer.WriteEndArray();
    }
}
