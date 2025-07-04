using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Craft.Extensions.System;

namespace Craft.QuerySpec;

/// <summary>
/// A custom JSON converter for the <see cref="EntityFilterCriteria{T}"/> class, enabling serialization and deserialization of filter expressions for entity filtering.
/// </summary>
/// <typeparam name="T">The type of the entities being filtered.</typeparam>
public sealed class EntityFilterCriteriaJsonConverter<T> : JsonConverter<EntityFilterCriteria<T>> where T : class
{
    /// <inheritdoc />
    public override EntityFilterCriteria<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Expression<Func<T, bool>>? filter = null;

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected StartObject token, got {reader.TokenType}.");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? propertyName = reader.GetString();

                reader.Read();

                if (propertyName == nameof(EntityFilterCriteria<>.Filter))
                {
                    var str = reader.GetString();

                    str = str?.Replace('\'', '\"');
                    str = str?.RemovePreFix("(");
                    str = str?.RemovePostFix(")");

                    filter = ExpressionTreeBuilder.BuildBinaryTreeExpression<T>(str);
                }
                else
                {
                    throw new JsonException($"Expected a valid property value instead of {reader.GetString()}");
                }
            }
        }

        if (filter == null)
            throw new JsonException("Missing or invalid 'Filter' property in JSON.");

        return new EntityFilterCriteria<T>(filter);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, EntityFilterCriteria<T> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        writer.WriteStartObject();
        writer.WriteString(nameof(EntityFilterCriteria<>.Filter), RemoveAccessor(value.Filter.Body.ToString()));
        writer.WriteEndObject();
    }

    /// <summary>
    /// Removes parameter accessor prefixes from the filter string for cleaner serialization.
    /// </summary>
    private static string RemoveAccessor(string source)
    {
        if (string.IsNullOrEmpty(source)) return source;
        // Remove patterns like (x.Property) => (Property)
        return Regex.Replace(source, @"\((\w+)\.", "(");
    }
}
