using Craft.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.QuerySpec;

/// <summary>
/// JSON converter for <see cref="EntityFilterBuilder{T}"/> to support custom serialization and deserialization.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class EntityFilterBuilderJsonConverter<T> : JsonConverter<EntityFilterBuilder<T>> where T : class
{
    /// <summary>
    /// Reads and converts the JSON to an <see cref="EntityFilterBuilder{T}"/> instance.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized <see cref="EntityFilterBuilder{T}"/>.</returns>
    /// <exception cref="JsonException">Thrown if the JSON format is invalid.</exception>
    public override EntityFilterBuilder<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($"Invalid format for {nameof(EntityFilterBuilder<>)}: expected array of {nameof(EntityFilterCriteria<>)}.");

        var entityFilterBuilder = new EntityFilterBuilder<T>();
        var localOptions = CreateLocalOptions(options);
        localOptions.Converters.Add(new EntityFilterCriteriaJsonConverter<T>());

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            var entityFilterCriteria = JsonSerializer.Deserialize<EntityFilterCriteria<T>>(ref reader, localOptions)
                ?? throw new JsonException($"Invalid format for {nameof(EntityFilterBuilder<>)}: expected array of {nameof(EntityFilterCriteria<>)}.");

            entityFilterBuilder.EntityFilterList.Add(entityFilterCriteria);
        }

        return entityFilterBuilder;
    }

    /// <summary>
    /// Writes the <see cref="EntityFilterBuilder{T}"/> to JSON.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    /// <exception cref="ArgumentNullException">Thrown if writer or value is null.</exception>
    public override void Write(Utf8JsonWriter writer, EntityFilterBuilder<T> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartArray();

        var localOptions = CreateLocalOptions(options);
        localOptions.Converters.Add(new EntityFilterCriteriaJsonConverter<T>());

        foreach (var entityFilter in value.EntityFilterList)
            JsonSerializer.Serialize(writer, entityFilter, localOptions);

        writer.WriteEndArray();
    }

    /// <summary>
    /// Indicates whether this converter can convert the specified object type.
    /// </summary>
    /// <param name="objectType">The type to check.</param>
    /// <returns>True if the type is <see cref="EntityFilterBuilder{T}"/>; otherwise, false.</returns>
    public override bool CanConvert(Type objectType)
        => objectType == typeof(EntityFilterBuilder<T>);

    /// <summary>
    /// Creates a shallow copy of the provided <see cref="JsonSerializerOptions"/> with selected properties copied.
    /// </summary>
    /// <param name="options">The options to copy from.</param>
    /// <returns>A new <see cref="JsonSerializerOptions"/> instance.</returns>
    private static JsonSerializerOptions CreateLocalOptions(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var clone = new JsonSerializerOptions
        {
            AllowTrailingCommas = options.AllowTrailingCommas,
            DefaultBufferSize = options.DefaultBufferSize,
            DictionaryKeyPolicy = options.DictionaryKeyPolicy,
            DefaultIgnoreCondition = options.DefaultIgnoreCondition,
            IgnoreReadOnlyFields = options.IgnoreReadOnlyFields,
            IgnoreReadOnlyProperties = options.IgnoreReadOnlyProperties,
            MaxDepth = options.MaxDepth,
            NumberHandling = options.NumberHandling,
            PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive,
            PropertyNamingPolicy = options.PropertyNamingPolicy,
            ReadCommentHandling = options.ReadCommentHandling,
            ReferenceHandler = options.ReferenceHandler,
            TypeInfoResolver = options.TypeInfoResolver,
            UnknownTypeHandling = options.UnknownTypeHandling,
            WriteIndented = options.WriteIndented
        };

        // Do not copy converters, as we want to add only the required one.
        return clone;
    }
}

