using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.QuerySpec;

/// <summary>
/// JSON converter factory for Query types. Automatically creates the appropriate QueryJsonConverter
/// based on the target type being deserialized.
/// </summary>
public class QueryJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        var genericType = typeToConvert.GetGenericTypeDefinition();

        // Handle both concrete Query<T> types and IQuery<T> interfaces
        return genericType == typeof(Query<>) || genericType == typeof(Query<,>) ||
               genericType == typeof(IQuery<>) || genericType == typeof(IQuery<,>);
    }

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArguments = typeToConvert.GetGenericArguments();
        var isInterface = typeToConvert.IsInterface;

        if (genericArguments.Length == 1)
        {
            // Query<T> or IQuery<T>
            var entityType = genericArguments[0];
            var converterType = typeof(QueryJsonConverter<>).MakeGenericType(entityType);
            return (JsonConverter?)Activator.CreateInstance(converterType);
        }
        else if (genericArguments.Length == 2)
        {
            // Query<T, TResult> or IQuery<T, TResult>
            var entityType = genericArguments[0];
            var resultType = genericArguments[1];
            var converterType = typeof(QueryJsonConverter<,>).MakeGenericType(entityType, resultType);
            return (JsonConverter?)Activator.CreateInstance(converterType);
        }

        return null;
    }
}
