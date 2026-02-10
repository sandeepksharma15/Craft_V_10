using Craft.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.QuerySpec;

/// <summary>
/// Custom JSON converter for <see cref="FilterCriteria"/> that handles serialization of the <see cref="Type"/> property.
/// </summary>
public sealed class FilterCriteriaJsonConverter : JsonConverter<FilterCriteria>
{
    /// <summary>
    /// Reads and converts the JSON to a <see cref="FilterCriteria"/> instance.
    /// </summary>
    public override FilterCriteria? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token.");

        string? propertyTypeName = null;
        string? name = null;
        JsonElement? valueElement = null;
        ComparisonType comparison = ComparisonType.EqualTo;
        string? displayTitle = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token.");

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case nameof(FilterCriteria.PropertyType):
                    propertyTypeName = reader.GetString();
                    break;
                case nameof(FilterCriteria.Name):
                    name = reader.GetString();
                    break;
                case nameof(FilterCriteria.Value):
                    // Store as JsonElement to deserialize later with correct type
                    valueElement = JsonElement.ParseValue(ref reader);
                    break;
                case nameof(FilterCriteria.Comparison):
                    comparison = JsonSerializer.Deserialize<ComparisonType>(ref reader, options);
                    break;
                case nameof(FilterCriteria.DisplayTitle):
                    displayTitle = reader.GetString();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (string.IsNullOrEmpty(propertyTypeName))
            throw new JsonException("PropertyType is required.");

        if (string.IsNullOrEmpty(name))
            throw new JsonException("Name is required.");

        var propertyType = Type.GetType(propertyTypeName)
            ?? throw new JsonException($"Unable to resolve type: {propertyTypeName}");

        // Deserialize the value with the correct type
        object? value = null;
        if (valueElement.HasValue && valueElement.Value.ValueKind != JsonValueKind.Null)
        {
            try
            {
                // Deserialize value using the actual property type
                value = JsonSerializer.Deserialize(valueElement.Value.GetRawText(), propertyType, options);
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to deserialize value for property '{name}' of type '{propertyType.Name}'.", ex);
            }
        }

        return new FilterCriteria(propertyType, name, value, comparison, displayTitle);
    }

    /// <summary>
    /// Writes the <see cref="FilterCriteria"/> instance as JSON.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, FilterCriteria value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartObject();

        writer.WriteString(nameof(FilterCriteria.PropertyType), value.PropertyType.AssemblyQualifiedName);
        writer.WriteString(nameof(FilterCriteria.Name), value.Name);

        writer.WritePropertyName(nameof(FilterCriteria.Value));
        JsonSerializer.Serialize(writer, value.Value, options);

        writer.WritePropertyName(nameof(FilterCriteria.Comparison));
        JsonSerializer.Serialize(writer, value.Comparison, options);

        writer.WriteString(nameof(FilterCriteria.DisplayTitle), value.DisplayTitle);

        writer.WriteEndObject();
    }
}

