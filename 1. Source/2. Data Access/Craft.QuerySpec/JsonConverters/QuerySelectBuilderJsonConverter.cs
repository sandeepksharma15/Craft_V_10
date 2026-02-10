using Craft.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.QuerySpec;

/// <summary>
/// Extension methods for JsonSerializerOptions.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Creates a shallow clone of JsonSerializerOptions.
    /// </summary>
    /// <param name="options">The options to clone.</param>
    /// <returns>A cloned instance of JsonSerializerOptions.</returns>
    public static JsonSerializerOptions GetClone(this JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var clone = new JsonSerializerOptions()
        {
            AllowTrailingCommas = options.AllowTrailingCommas,
            DefaultBufferSize = options.DefaultBufferSize,
            DictionaryKeyPolicy = options.DictionaryKeyPolicy,
            DefaultIgnoreCondition = options.DefaultIgnoreCondition,
            IgnoreReadOnlyFields = options.IgnoreReadOnlyFields,
            IgnoreReadOnlyProperties = options.IgnoreReadOnlyProperties,
            IncludeFields = options.IncludeFields,
            MaxDepth = options.MaxDepth,
            NumberHandling = options.NumberHandling,
            PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive,
            PropertyNamingPolicy = options.PropertyNamingPolicy,
            ReadCommentHandling = options.ReadCommentHandling,
            WriteIndented = options.WriteIndented
        };

        // Copy converters
        foreach (var converter in options.Converters)
            clone.Converters.Add(converter);

        return clone;
    }
}

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
            if (selectDescriptor == null || selectDescriptor.Assignee == null && selectDescriptor.Assignor == null)
                throw new JsonException("Invalid select descriptor encountered in QuerySelectBuilder array");
            else
                querySelectBuilder.Add(selectDescriptor);
        }

        return querySelectBuilder;
    }

    public override void Write(Utf8JsonWriter writer, QuerySelectBuilder<T, TResult> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        // Start The Array
        writer.WriteStartArray();

        // We Want To Clone The Options To Add The SelectDescriptorJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new SelectDescriptorJsonConverter<T, TResult>());

        foreach (var selectDescriptor in value.SelectDescriptorList)
            JsonSerializer.Serialize(writer, selectDescriptor, localOptions);

        // End the array
        writer.WriteEndArray();
    }
}

