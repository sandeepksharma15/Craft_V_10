using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Controllers;

/// <summary>
/// Extension methods for configuring API versioning for controllers.
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Adds API versioning with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to customize versioning options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Default configuration:
    /// <list type="bullet">
    /// <item>Default API version: 1.0</item>
    /// <item>Assumes default version when unspecified</item>
    /// <item>Reports API versions in response headers</item>
    /// <item>URL segment versioning (e.g., /api/v1/users)</item>
    /// <item>Query string versioning (e.g., /api/users?api-version=1.0)</item>
    /// <item>Header versioning (e.g., X-Api-Version: 1.0)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Usage in Program.cs:</strong>
    /// <code>
    /// builder.Services.AddControllerApiVersioning();
    /// 
    /// var app = builder.Build();
    /// app.MapControllers();
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Usage in Controllers:</strong>
    /// <code>
    /// [ApiVersion("1.0")]
    /// [ApiVersion("2.0")]
    /// [Route("api/v{version:apiVersion}/[controller]")]
    /// public class UsersController : EntityReadController&lt;User, UserDto&gt;
    /// {
    ///     [HttpGet]
    ///     [MapToApiVersion("1.0")]
    ///     public async Task&lt;IActionResult&gt; GetV1() { }
    ///     
    ///     [HttpGet]
    ///     [MapToApiVersion("2.0")]
    ///     public async Task&lt;IActionResult&gt; GetV2() { }
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Note:</strong> For Swagger/OpenAPI documentation with API versioning, use the Craft.OpenAPI module
    /// which provides comprehensive OpenAPI documentation features including automatic versioned document generation.
    /// </para>
    /// </remarks>
    /// <example>
    /// Custom configuration:
    /// <code>
    /// builder.Services.AddControllerApiVersioning(options =>
    /// {
    ///     options.DefaultApiVersion = new ApiVersion(2, 0);
    ///     options.ReportApiVersions = true;
    ///     options.AssumeDefaultVersionWhenUnspecified = false;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddControllerApiVersioning(
        this IServiceCollection services,
        Action<ApiVersioningOptions>? configureOptions = null)
    {
        services.AddApiVersioning(options =>
        {
            // Default API version is 1.0
            options.DefaultApiVersion = new ApiVersion(1, 0);
            
            // Assume default version when client doesn't specify
            options.AssumeDefaultVersionWhenUnspecified = true;
            
            // Report available API versions in response headers
            options.ReportApiVersions = true;
            
            // Support multiple versioning strategies
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),           // /api/v1/users
                new QueryStringApiVersionReader("api-version"),  // /api/users?api-version=1.0
                new QueryStringApiVersionReader("v"),            // /api/users?v=1.0
                new HeaderApiVersionReader("X-Api-Version"),     // X-Api-Version: 1.0
                new HeaderApiVersionReader("Api-Version"),       // Api-Version: 1.0
                new MediaTypeApiVersionReader("version")         // Accept: application/json;version=1.0
            );
            
            // Apply custom configuration if provided
            configureOptions?.Invoke(options);
        })
        .AddMvc()  // Add MVC API versioning
        .AddApiExplorer(options =>
        {
            // Format the version as "'v'major[.minor][-status]"
            options.GroupNameFormat = "'v'VVV";
            
            // Substitute version in URL
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
