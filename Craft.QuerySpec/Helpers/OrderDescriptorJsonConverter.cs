using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// A custom JSON converter specifically designed to serialize and deserialize instances of the OrderDescriptor<T> class,
/// ensuring proper handling of LambdaExpressions representing order items and OrderTypeEnum values.
/// </summary>
/// <typeparam name="T">The type of the entity being ordered.</typeparam>
public class OrderDescriptorJsonConverter<T> : JsonConverter<OrderDescriptor<T>> where T : class
{
    public override OrderDescriptor<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        OrderDescriptor<T> orderInfo = new(null!);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                if (propertyName == nameof(OrderDescriptor<>.OrderItem))
                    orderInfo.OrderItem = typeof(T).CreateMemberExpression(reader.GetString());

                if (propertyName == nameof(OrderDescriptor<>.OrderType))
                    orderInfo.OrderType = (OrderTypeEnum)reader.GetInt32();
            }
        }

        return orderInfo;
    }

    public override void Write(Utf8JsonWriter writer, OrderDescriptor<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var memberName = value.OrderItem.GetPropertyInfo().Name;
        writer.WriteString(nameof(OrderDescriptor<>.OrderItem), memberName);
        writer.WriteNumber(nameof(OrderDescriptor<>.OrderType), (int)value.OrderType);

        writer.WriteEndObject();
    }
}
