using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.Core;

/// <summary>
/// A factory for creating JSON converters that handle serialization and deserialization of  <see
/// cref="PageResponse{T}"/> objects.
/// </summary>
/// <remarks>This factory dynamically generates a <see cref="JsonConverter"/> for the generic type  <see
/// cref="PageResponse{T}"/> based on the type argument provided. It is used by the  <see cref="System.Text.Json"/>
/// serialization infrastructure to support custom serialization  logic for paginated response objects.</remarks>
/// 
/// Usage: This factory can be registered with the JSON serializer options in the following way:
/// var options = new JsonSerializerOptions();
/// options.Converters.Add(new PageResponseJsonConverterFactory());
/// OR
/// builder.Services.AddControllers().AddJsonOptions(opts => { opts.JsonSerializerOptions.Converters.Add(new PageResponseJsonConverterFactory()); });

public class PageResponseJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(PageResponse<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type elementType = typeToConvert.GetGenericArguments()[0];
        Type converterType = typeof(PageResponseJsonConverter<>).MakeGenericType(elementType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
