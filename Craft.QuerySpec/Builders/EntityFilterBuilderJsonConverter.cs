using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.QuerySpec;

public class EntityFilterBuilderJsonConverter<T> : JsonConverter<EntityFilterBuilder<T>> where T : class
{
    public override bool CanConvert(Type objectType)
        => objectType == typeof(EntityFilterBuilder<T>);

    public override EntityFilterBuilder<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var entityFilterBuilder = new EntityFilterBuilder<T>();

        // We Want To Clone The Options To Add The EntityFilterCriteriaJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new EntityFilterCriteriaJsonConverter<T>());

        // Check for array start
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Invalid format for SortOrderBuilder: expected array of OrderDescriptor");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            var entityFilterCriteria = JsonSerializer.Deserialize<EntityFilterCriteria<T>>(ref reader, localOptions);

            if (entityFilterCriteria != null)
                entityFilterBuilder.EntityFilterList.Add(entityFilterCriteria);
            else
                throw new JsonException("Invalid format for EntityFilterBuilder: expected array of EntityFilterCriteria");
        }

        return entityFilterBuilder;
    }

    public override void Write(Utf8JsonWriter writer, EntityFilterBuilder<T> value, JsonSerializerOptions options)
    {
        // Start The Array
        writer.WriteStartArray();

        // We Want To Clone The Options To Add The EntityFilterCriteriaJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new EntityFilterCriteriaJsonConverter<T>());

        foreach (var entityFilter in value.EntityFilterList)
        {
            var json = JsonSerializer.Serialize(entityFilter, localOptions);
            writer.WriteRawValue(json);
        }

        // End the array
        writer.WriteEndArray();
    }
}
