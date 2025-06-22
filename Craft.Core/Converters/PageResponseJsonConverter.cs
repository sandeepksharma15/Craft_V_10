using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.Core;

/// <summary>
/// Provides custom JSON serialization and deserialization for <see cref="PageResponse{T}"/> objects.
/// </summary>
/// <remarks>This converter handles the serialization and deserialization of paginated responses, ensuring that
/// the <see cref="PageResponse{T}"/> object is correctly mapped to and from JSON. The JSON format is expected to
/// include the following properties: <list type="bullet"> <item><term>Items</term><description>An array of items of
/// type <typeparamref name="T"/>.</description></item> <item><term>CurrentPage</term><description>The current page
/// number.</description></item> <item><term>PageSize</term><description>The number of items per
/// page.</description></item> <item><term>TotalCount</term><description>The total number of items across all
/// pages.</description></item> </list></remarks>
/// <typeparam name="T">The type of items contained within the <see cref="PageResponse{T}"/>. Must be a reference type.</typeparam>

/// Usage: This converter can be used with the JSON serializer in the following way:
/// var options = new JsonSerializerOptions { Converters = { new PageResponseJsonConverter<YourType>() } };
/// var json = JsonSerializer.Serialize(pageResponse, options);
/// OR
/// builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new PageResponseJsonConverter()); });
/// 
public class PageResponseJsonConverter<T> : JsonConverter<PageResponse<T>> where T : class
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(PageResponse<T>);

    public override PageResponse<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        IEnumerable<T> items = [];
        int currentPage = 1;
        int pageSize = 10;
        long totalCount = 0;

        // Start the object
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Invalid format for Query: expected start object");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Invalid format for Query: expected property name");

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "Items":
                    items = JsonSerializer.Deserialize<IEnumerable<T>>(ref reader, options) ?? [];
                    break;
                case "CurrentPage":
                    currentPage = JsonSerializer.Deserialize<int>(ref reader, options);
                    break;
                case "PageSize":
                    pageSize = JsonSerializer.Deserialize<int>(ref reader, options);
                    break;
                case "TotalCount":
                    totalCount = JsonSerializer.Deserialize<long>(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return new PageResponse<T>(items!, totalCount, currentPage, pageSize);
    }

    public override void Write(Utf8JsonWriter writer, PageResponse<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Items");
        JsonSerializer.Serialize(writer, value.Items, options);

        writer.WritePropertyName("CurrentPage");
        JsonSerializer.Serialize(writer, value.CurrentPage, options);

        writer.WritePropertyName("PageSize");
        JsonSerializer.Serialize(writer, value.PageSize, options);

        writer.WritePropertyName("TotalCount");
        JsonSerializer.Serialize(writer, value.TotalCount, options);

        writer.WriteEndObject();
    }
}
