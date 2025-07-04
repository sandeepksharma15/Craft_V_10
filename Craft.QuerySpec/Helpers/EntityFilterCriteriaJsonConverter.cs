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
    /// <summary>
    /// The property name for the filter, cached for performance and maintainability.
    /// </summary>
    private static readonly string FilterPropertyName = nameof(EntityFilterCriteria<>.Filter);

    /// <inheritdoc />
    /// <remarks>
    /// The filter string is expected to be a valid C# boolean expression (e.g., "x => x.Property == value").
    /// Only simple binary expressions are supported. Complex/nested expressions may not be parsed correctly.
    /// </remarks>
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

                if (propertyName == FilterPropertyName)
                {
                    var str = reader.GetString();

                    if (string.IsNullOrWhiteSpace(str))
                        throw new JsonException($"The '{FilterPropertyName}' property value is null or empty.");

                    try
                    {
                        str = str.Replace('\'', '\"');
                        str = str.RemovePreFix("(");
                        str = str.RemovePostFix(")");

                        // Validate basic format (very basic, can be extended)
                        if (!str!.Contains("==") && !str!.Contains("!=") && !str!.Contains('>') && !str!.Contains('<'))
                            throw new JsonException($"The filter string '{str}' does not appear to be a valid binary expression.");

                        filter = ExpressionTreeBuilder.BuildBinaryTreeExpression<T>(str);
                    }
                    catch (Exception ex)
                    {
                        throw new JsonException($"Failed to parse filter expression: '{str}'. See inner exception for details.", ex);
                    }
                }
                else
                {
                    throw new JsonException($"Unexpected property '{propertyName}' in EntityFilterCriteria JSON. Only '{FilterPropertyName}' is supported.");
                }
            }
        }

        if (filter == null)
            throw new JsonException($"Missing or invalid '{FilterPropertyName}' property in JSON.");

        return new EntityFilterCriteria<T>(filter);
    }

    /// <inheritdoc />
    /// <remarks>
    /// If the value is null, writes a JSON null. Otherwise, writes the filter as a string property.
    /// </remarks>
    public override void Write(Utf8JsonWriter writer, EntityFilterCriteria<T> value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteString(FilterPropertyName, RemoveAccessor(value.Filter.Body.ToString()));
        writer.WriteEndObject();
    }

    /// <summary>
    /// Removes all parameter accessor prefixes from the filter string for cleaner serialization.
    /// For example, converts "(x.Property1 == 5 && x.Property2 == 10)" to "(Property1 == 5 && Property2 == 10)".
    /// </summary>
    /// <param name="source">The filter string to clean.</param>
    /// <returns>The cleaned filter string.</returns>
    private static string RemoveAccessor(string source)
    {
        if (string.IsNullOrEmpty(source)) return source;
        // Remove all patterns like x.Property, y.Property, etc. (handles nested and multiple accessors)
        return Regex.Replace(source, @"\b\w+\.", string.Empty);
    }
}
