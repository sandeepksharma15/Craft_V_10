using Craft.Core;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// JSON converter for <see cref="SqlLikeSearchInfo{T}"/> to support custom serialization and deserialization.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class SqlLikeSearchInfoJsonConverter<T> : JsonConverter<SqlLikeSearchInfo<T>> where T : class
{
    // Use constants for property names to avoid magic strings and ensure refactor safety
    private const string SearchItemProperty = "SearchItem";
    private const string SearchStringProperty = "SearchString";
    private const string SearchGroupProperty = "SearchGroup";

    /// <inheritdoc />
    public override SqlLikeSearchInfo<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected start of object, got {reader.TokenType}.");

        var searchInfo = new SqlLikeSearchInfo<T>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Expected property name, got {reader.TokenType}.");

            var propertyName = reader.GetString();
            if (!reader.Read())
                throw new JsonException("Unexpected end of JSON while reading property value.");

            switch (propertyName)
            {
                case SearchItemProperty:
                    var memberName = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    if (!string.IsNullOrWhiteSpace(memberName))
                        try
                        {
                            searchInfo.SearchItem = typeof(T).CreateMemberExpression(memberName);
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException($"Failed to create member expression for '{memberName}' on type '{typeof(T).Name}'.", ex);
                        }
                    else
                        searchInfo.SearchItem = null;
                    break;
                case SearchStringProperty:
                    searchInfo.SearchString = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    break;
                case SearchGroupProperty:
                    searchInfo.SearchGroup = reader.TokenType == JsonTokenType.Number ? reader.GetInt32() : 0;
                    break;
                default:
                    // Skip unknown properties
                    reader.Skip();
                    break;
            }
        }

        return searchInfo;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SqlLikeSearchInfo<T>? value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Write the property name of the member expression, or null if not set
        var memberName = value?.SearchItem?.GetPropertyInfo()?.Name;
        writer.WriteString(SearchItemProperty, memberName);
        writer.WriteString(SearchStringProperty, value?.SearchString);
        writer.WriteNumber(SearchGroupProperty, value?.SearchGroup ?? 0);

        writer.WriteEndObject();
    }
}

