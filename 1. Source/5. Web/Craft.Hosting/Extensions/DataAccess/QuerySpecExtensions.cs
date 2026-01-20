using Craft.QuerySpec;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Hosting.Extensions;

/// <summary>
/// Extension methods for configuring QuerySpec services.
/// </summary>
public static class QuerySpecExtensions
{
    /// <summary>
    /// Configures JSON serialization to support Query&lt;T&gt; and Query&lt;T, TResult&gt; types.
    /// This adds the necessary JSON converters for proper serialization/deserialization of queries over HTTP.
    /// </summary>
    /// <param name="builder">The IMvcBuilder instance.</param>
    /// <returns>The IMvcBuilder for chaining.</returns>
    /// <remarks>
    /// Call this method after AddControllers() to register QueryJsonConverter globally.
    /// This enables EntityController endpoints to properly deserialize query specifications.
    /// </remarks>
    public static IMvcBuilder AddQuerySpecJsonOptions(this IMvcBuilder builder)
    {
        builder.AddJsonOptions(options =>
        {
            // Add the converter factory that handles all Query<T> and Query<T, TResult> types
            options.JsonSerializerOptions.Converters.Add(new QueryJsonConverterFactory());

            // Set common JSON options that align with QuerySerializerOptions
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        return builder;
    }
}
