using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Craft.Extensions.System;

namespace Craft.QuerySpec;

/// <summary>
/// A custom JSON converter for <see cref="EntityFilterCriteria{T}"/> that enables serialization and deserialization
/// of filter expressions for entity filtering. Provides robust error handling and validation for JSON operations.
/// </summary>
/// <typeparam name="T">The type of the entities being filtered. Must be a reference type.</typeparam>
/// <remarks>
/// This converter handles the serialization of LINQ expression trees to JSON strings and their reconstruction
/// during deserialization. It includes comprehensive error handling, input validation, and performance optimizations.
/// </remarks>
public sealed class EntityFilterCriteriaJsonConverter<T> : JsonConverter<EntityFilterCriteria<T>> where T : class
{
    #region Constants

    /// <summary>The JSON property name for the filter expression.</summary>
    private const string FilterPropertyName = "Filter";

    /// <summary>Cached regex pattern for removing parameter accessors from expression strings.</summary>
    private static readonly Regex ParameterAccessorRegex = new(@"\((\w+)\.", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    #endregion

    #region JsonConverter Implementation

    /// <summary>
    /// Reads and converts the JSON to an <see cref="EntityFilterCriteria{T}"/> instance.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The converted <see cref="EntityFilterCriteria{T}"/> instance, or null if the JSON represents null.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or contains invalid data.</exception>
    public override EntityFilterCriteria<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Handle null values
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        // Validate that we start with an object
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected StartObject token, but found {reader.TokenType}.");

        Expression<Func<T, bool>>? filter = null;

        var propertyCount = 0;

        try
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyCount++;
                    var propertyName = reader.GetString();

                    if (!reader.Read())
                        throw new JsonException("Unexpected end of JSON input while reading property value.");

                    if (string.Equals(propertyName, FilterPropertyName, StringComparison.Ordinal))
                        filter = ReadFilterExpression(ref reader);
                    else
                        // Skip unknown properties gracefully instead of throwing
                        SkipJsonValue(ref reader);
                }
                else
                    throw new JsonException($"Unexpected token {reader.TokenType} while reading object properties.");
            }

            // Validate that we found the required Filter property
            if (filter is null)
                if (propertyCount == 0)
                    throw new JsonException("Empty JSON object is not valid for EntityFilterCriteria.");
                else
                    throw new JsonException($"Required property '{FilterPropertyName}' not found in JSON object.");

            return new EntityFilterCriteria<T>(filter);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Failed to create EntityFilterCriteria from the provided filter expression.", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new JsonException("Invalid filter expression provided in JSON.", ex);
        }
    }

    /// <summary>
    /// Writes the <see cref="EntityFilterCriteria{T}"/> instance as JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is null.</exception>
    public override void Write(Utf8JsonWriter writer, EntityFilterCriteria<T>? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));

        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        try
        {
            writer.WriteStartObject();

            var filterString = SerializeFilterExpression(value.Filter);
            writer.WriteString(FilterPropertyName, filterString);

            writer.WriteEndObject();
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new JsonException($"Failed to serialize EntityFilterCriteria<{typeof(T).Name}> to JSON.", ex);
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Reads and parses a filter expression from the JSON reader.
    /// </summary>
    /// <param name="reader">The JSON reader positioned at the filter value.</param>
    /// <returns>The parsed LINQ expression.</returns>
    /// <exception cref="JsonException">Thrown when the filter expression is invalid or cannot be parsed.</exception>
    private static Expression<Func<T, bool>> ReadFilterExpression(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null)
            throw new JsonException("Filter expression cannot be null.");

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Filter expression must be a string, but found {reader.TokenType}.");

        var filterString = reader.GetString();

        if (string.IsNullOrWhiteSpace(filterString))
            throw new JsonException("Filter expression cannot be empty or whitespace.");

        try
        {
            // Normalize the filter string by processing quotes and parentheses
            var normalizedFilter = NormalizeFilterString(filterString);

            // Build the expression using the ExpressionTreeBuilder
            var expression = ExpressionTreeBuilder.BuildBinaryTreeExpression<T>(normalizedFilter);

            return expression ?? throw new JsonException($"Unable to parse filter expression: '{filterString}'");
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            throw new JsonException($"Failed to parse filter expression: '{filterString}'", ex);
        }
    }

    /// <summary>
    /// Normalizes a filter string by replacing single quotes with double quotes and removing unnecessary parentheses.
    /// </summary>
    /// <param name="filterString">The raw filter string from JSON.</param>
    /// <returns>The normalized filter string ready for expression parsing.</returns>
    [return: NotNullIfNotNull(nameof(filterString))]
    private static string? NormalizeFilterString(string? filterString)
    {
        if (string.IsNullOrEmpty(filterString))
            return filterString;

        // Replace single quotes with double quotes for string literals
        var normalized = filterString.Replace('\'', '"');

        // Remove outer parentheses if they encompass the entire expression
        normalized = normalized.RemovePreFix("(")?.RemovePostFix(")");

        return normalized ?? filterString; // Ensure we return non-null when input is non-null
    }

    /// <summary>
    /// Serializes a filter expression to a clean string representation suitable for JSON.
    /// </summary>
    /// <param name="filterExpression">The LINQ expression to serialize.</param>
    /// <returns>A clean string representation of the expression.</returns>
    private static string SerializeFilterExpression(Expression<Func<T, bool>> filterExpression)
    {
        ArgumentNullException.ThrowIfNull(filterExpression, nameof(filterExpression));

        try
        {
            var expressionString = filterExpression.Body.ToString();

            // Remove parameter accessor patterns (e.g., "(x." becomes "(")
            var cleanedExpression = ParameterAccessorRegex.Replace(expressionString, "(");

            return cleanedExpression;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to serialize filter expression for type '{typeof(T).Name}'.", ex);
        }
    }

    /// <summary>
    /// Skips the current JSON value in the reader.
    /// </summary>
    /// <param name="reader">The JSON reader to advance.</param>
    private static void SkipJsonValue(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.PropertyName)
            reader.Read();

        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                var depth = 0;
                do
                    if (reader.TokenType == JsonTokenType.StartObject)
                        depth++;
                    else if (reader.TokenType == JsonTokenType.EndObject)
                        depth--;
                while (depth > 0 && reader.Read());
                break;

            case JsonTokenType.StartArray:
                depth = 0;
                do
                    if (reader.TokenType == JsonTokenType.StartArray)
                        depth++;
                    else if (reader.TokenType == JsonTokenType.EndArray)
                        depth--;
                while (depth > 0 && reader.Read());
                break;

            default:
                // For primitive values, we're already positioned correctly
                break;
        }
    }

    #endregion
}
