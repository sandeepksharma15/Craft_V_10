using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Controllers.Extensions;

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

    /// <summary>
    /// Configures API versioning for use with Swagger/OpenAPI documentation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to customize versioning options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method configures API versioning specifically for Swagger documentation generation,
    /// ensuring that each API version gets its own Swagger document.
    /// </para>
    /// <para>
    /// <strong>Usage with Swagger:</strong>
    /// <code>
    /// builder.Services.AddControllerApiVersioningWithSwagger();
    /// builder.Services.AddSwaggerGen();
    /// builder.Services.ConfigureSwaggerForVersioning();
    /// </code>
    /// </para>
    /// </remarks>
    public static IServiceCollection AddControllerApiVersioningWithSwagger(
        this IServiceCollection services,
        Action<ApiVersioningOptions>? configureOptions = null)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("X-Api-Version")
            );
            
            configureOptions?.Invoke(options);
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
            
            // Add versioned API explorer for Swagger
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger to support API versioning with separate documents per version.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Generates a separate Swagger document for each API version detected.
    /// Requires <c>AddControllerApiVersioningWithSwagger()</c> to be called first.
    /// </para>
    /// <para>
    /// <strong>Usage:</strong>
    /// <code>
    /// builder.Services.AddControllerApiVersioningWithSwagger();
    /// builder.Services.AddSwaggerGen();
    /// builder.Services.ConfigureSwaggerForVersioning();
    /// 
    /// var app = builder.Build();
    /// app.UseSwagger();
    /// app.UseSwaggerUI(options =>
    /// {
    ///     var provider = app.Services.GetRequiredService&lt;IApiVersionDescriptionProvider&gt;();
    ///     foreach (var description in provider.ApiVersionDescriptions)
    ///     {
    ///         options.SwaggerEndpoint(
    ///             $"/swagger/{description.GroupName}/swagger.json",
    ///             description.GroupName.ToUpperInvariant());
    ///     }
    /// });
    /// </code>
    /// </para>
    /// </remarks>
    public static IServiceCollection ConfigureSwaggerForVersioning(this IServiceCollection services)
    {
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        return services;
    }
}

/// <summary>
/// Configures Swagger options for API versioning.
/// </summary>
internal class ConfigureSwaggerOptions : IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
    /// </summary>
    /// <param name="provider">The API version description provider.</param>
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public void Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        // Add a swagger document for each discovered API version
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    private static Microsoft.OpenApi.Models.OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Craft API",
            Version = description.ApiVersion.ToString(),
            Description = "A production-ready API with versioning support.",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "API Support",
                Email = "support@example.com"
            },
            License = new Microsoft.OpenApi.Models.OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };

        if (description.IsDeprecated)
        {
            info.Description += " (This API version has been deprecated)";
        }

        return info;
    }
}
