using System.Text.Json;

namespace Craft.QuerySpec;

/// <summary>
/// Provides configured JsonSerializerOptions for Query serialization/deserialization.
/// </summary>
public static class QuerySerializerOptions
{
    private static JsonSerializerOptions? _defaultOptions;

    /// <summary>
    /// Gets the default JsonSerializerOptions configured with QueryJsonConverterFactory.
    /// This instance is cached and reused for performance.
    /// </summary>
    public static JsonSerializerOptions Default
    {
        get
        {
            if (_defaultOptions == null)
            {
                _defaultOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                _defaultOptions.Converters.Add(new QueryJsonConverterFactory());
            }

            return _defaultOptions;
        }
    }

    /// <summary>
    /// Creates JsonSerializerOptions configured for Query&lt;T&gt; serialization.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>Configured JsonSerializerOptions instance.</returns>
    public static JsonSerializerOptions Create<T>() where T : class
    {
        var options = new JsonSerializerOptions(Default);
        options.Converters.Add(new QueryJsonConverter<T>());
        return options;
    }

    /// <summary>
    /// Creates JsonSerializerOptions configured for Query&lt;T, TResult&gt; serialization.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <returns>Configured JsonSerializerOptions instance.</returns>
    public static JsonSerializerOptions Create<T, TResult>() where T : class where TResult : class
    {
        var options = new JsonSerializerOptions(Default);
        options.Converters.Add(new QueryJsonConverter<T, TResult>());
        return options;
    }
}
