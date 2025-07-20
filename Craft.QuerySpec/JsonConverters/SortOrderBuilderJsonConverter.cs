using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.QuerySpec;

public class SortOrderBuilderJsonConverter<T> : JsonConverter<SortOrderBuilder<T>> where T : class
{
    public override bool CanConvert(Type objectType)
        => objectType == typeof(SortOrderBuilder<T>);

    public override SortOrderBuilder<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Create a new SortOrderBuilder
        var orderBuilder = new SortOrderBuilder<T>();

        // We Want To Clone The Options To Add The OrderDescriptorJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new OrderDescriptorJsonConverter<T>());

        // Check for array start
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Invalid format for SortOrderBuilder: expected array of OrderDescriptor");

        // Read each order expression
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            // Read the individual order expression object
            var orderInfo = JsonSerializer.Deserialize<OrderDescriptor<T>>(ref reader, localOptions);

            // Validate and add the order expression
            if (orderInfo != null)
                orderBuilder.Add(orderInfo);
            else
                throw new JsonException("Invalid order expression encountered in SortOrderBuilder array.");
        }

        return orderBuilder;
    }

    public override void Write(Utf8JsonWriter writer, SortOrderBuilder<T> value, JsonSerializerOptions options)
    {
        // Start The Array
        writer.WriteStartArray();

        // We Want To Clone The Options To Add The OrderDescriptorJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new OrderDescriptorJsonConverter<T>());

        foreach (var order in value.OrderDescriptorList)
        {
            var json = JsonSerializer.Serialize(order, localOptions);
            writer.WriteRawValue(json);
        }

        // End the array
        writer.WriteEndArray();
    }
}
