using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.Core;

/// <summary>
/// Provides custom JSON serialization and deserialization for <see cref="PageResponse{T}"/> objects.
/// </summary>
/// <remarks>
/// <para>This converter handles the serialization and deserialization of paginated responses, ensuring that
/// the <see cref="PageResponse{T}"/> object is correctly mapped to and from JSON.</para>
/// <para>The JSON format is expected to include the following properties:</para>
/// <list type="bullet">
///   <item><term>Items/items</term><description>An array of items of type T</description></item>
///   <item><term>CurrentPage/currentPage</term><description>The current page number</description></item>
///   <item><term>PageSize/pageSize</term><description>The number of items per page</description></item>
///   <item><term>TotalCount/totalCount</term><description>The total number of items across all pages</description></item>
/// </list>
/// </remarks>
/// <typeparam name="T">The type of items contained within the <see cref="PageResponse{T}"/>. Must be a reference type.</typeparam>
/// <example>
/// <code>
/// var options = new JsonSerializerOptions { Converters = { new PageResponseJsonConverterFactory() } };
/// var json = JsonSerializer.Serialize(pageResponse, options);
/// </code>
/// </example>
public class PageResponseJsonConverter<T> : JsonConverter<PageResponse<T>> where T : class
{
    private static readonly StringComparer PropertyComparer = StringComparer.OrdinalIgnoreCase;

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(PageResponse<T>);

    public override PageResponse<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Invalid JSON format for {nameof(PageResponse<>)}: expected start of object.");

        IEnumerable<T> items = [];
        int currentPage = 1;
        int pageSize = 10;
        long totalCount = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Invalid JSON format for {nameof(PageResponse<>)}: expected property name.");

            var propertyName = reader.GetString();
            reader.Read();

            if (propertyName is null)
            {
                reader.Skip();
                continue;
            }

            // Case-insensitive property matching
            if (PropertyComparer.Equals(propertyName, nameof(PageResponse<>.Items)))
                items = JsonSerializer.Deserialize<IEnumerable<T>>(ref reader, options) ?? [];
            else if (PropertyComparer.Equals(propertyName, nameof(PageResponse<>.CurrentPage)))
                currentPage = reader.GetInt32();
            else if (PropertyComparer.Equals(propertyName, nameof(PageResponse<>.PageSize)))
                pageSize = reader.GetInt32();
            else if (PropertyComparer.Equals(propertyName, nameof(PageResponse<>.TotalCount)))
                totalCount = reader.GetInt64();
            else
                reader.Skip();
        }

        return new PageResponse<T>(items, totalCount, currentPage, pageSize);
    }

    public override void Write(Utf8JsonWriter writer, PageResponse<T> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);

        var namingPolicy = options.PropertyNamingPolicy;

        writer.WriteStartObject();

        writer.WritePropertyName(namingPolicy?.ConvertName(nameof(value.Items)) ?? nameof(value.Items));
        JsonSerializer.Serialize(writer, value.Items, options);

        writer.WritePropertyName(namingPolicy?.ConvertName(nameof(value.CurrentPage)) ?? nameof(value.CurrentPage));
        writer.WriteNumberValue(value.CurrentPage);

        writer.WritePropertyName(namingPolicy?.ConvertName(nameof(value.PageSize)) ?? nameof(value.PageSize));
        writer.WriteNumberValue(value.PageSize);

        writer.WritePropertyName(namingPolicy?.ConvertName(nameof(value.TotalCount)) ?? nameof(value.TotalCount));
        writer.WriteNumberValue(value.TotalCount);

        writer.WritePropertyName(namingPolicy?.ConvertName(nameof(value.TotalPages)) ?? nameof(value.TotalPages));
        writer.WriteNumberValue(value.TotalPages);

        writer.WritePropertyName(namingPolicy?.ConvertName(nameof(value.HasNextPage)) ?? nameof(value.HasNextPage));
        writer.WriteBooleanValue(value.HasNextPage);

        writer.WritePropertyName(namingPolicy?.ConvertName(nameof(value.HasPreviousPage)) ?? nameof(value.HasPreviousPage));
        writer.WriteBooleanValue(value.HasPreviousPage);

        writer.WriteEndObject();
    }
}
