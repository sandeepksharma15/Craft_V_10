using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.QuerySpec;

/// <summary>
/// JSON converter for <see cref="EntityFilterBuilder{T}"/> to support custom serialization and deserialization.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class EntityFilterBuilderJsonConverter<T> : JsonConverter<EntityFilterBuilder<T>> where T : class
{
    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
        => objectType == typeof(EntityFilterBuilder<T>);

    /// <inheritdoc />
    public override EntityFilterBuilder<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($"Invalid format for {nameof(EntityFilterBuilder<>)}: expected array of {nameof(EntityFilterCriteria<>)}.");

        var entityFilterBuilder = new EntityFilterBuilder<T>();
        var localOptions = options.GetClone();
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

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EntityFilterBuilder<T> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartArray();

        var localOptions = options.GetClone();
        localOptions.Converters.Add(new EntityFilterCriteriaJsonConverter<T>());

        foreach (var entityFilter in value.EntityFilterList)
            JsonSerializer.Serialize(writer, entityFilter, localOptions);

        writer.WriteEndArray();
    }
}
