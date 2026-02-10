using Craft.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// JSON converter for <see cref="OrderDescriptor{T}"/> to handle serialization and deserialization of order expressions.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class OrderDescriptorJsonConverter<T> : JsonConverter<OrderDescriptor<T>> where T : class
{
    public override OrderDescriptor<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        LambdaExpression? orderItem = null;
        OrderTypeEnum orderType = OrderTypeEnum.OrderBy;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token.");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                if (propertyName == nameof(OrderDescriptor<>.OrderItem))
                {
                    var memberName = reader.GetString();

                    if (string.IsNullOrWhiteSpace(memberName))
                        throw new JsonException("OrderItem property cannot be null or empty.");

                    // Create the member expression
                    var memberExpression = typeof(T).CreateMemberExpression(memberName) 
                        ?? throw new JsonException($"Could not create member expression for '{memberName}'.");

                    // Convert to Expression<Func<T, object>> by boxing if necessary
                    var parameter = memberExpression.Parameters[0];
                    var body = memberExpression.Body;

                    // Box value types to object
                    if (body.Type.IsValueType)
                    {
                        body = Expression.Convert(body, typeof(object));
                    }
                    else if (body.Type != typeof(object))
                    {
                        // For reference types, cast to object
                        body = Expression.Convert(body, typeof(object));
                    }

                    orderItem = Expression.Lambda<Func<T, object>>(body, parameter);
                }
                else if (propertyName == nameof(OrderDescriptor<>.OrderType))
                {
                    if (reader.TokenType != JsonTokenType.Number)
                        throw new JsonException("OrderType must be an integer.");

                    orderType = (OrderTypeEnum)reader.GetInt32();
                }
                else
                    throw new JsonException($"Unknown property '{propertyName}' in OrderDescriptor.");
            }
        }

        return orderItem == null
            ? throw new JsonException("Missing required property 'OrderItem'.")
            : new OrderDescriptor<T>(orderItem, orderType);
    }

    public override void Write(Utf8JsonWriter writer, OrderDescriptor<T> value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        var memberName = value.OrderItem.GetPropertyInfo()?.Name
            ?? throw new JsonException("OrderItem expression does not reference a property.");

        writer.WriteString(nameof(OrderDescriptor<>.OrderItem), memberName);
        writer.WriteNumber(nameof(OrderDescriptor<>.OrderType), (int)value.OrderType);

        writer.WriteEndObject();
    }
}

