using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Craft.Controllers.Extensions;

/// <summary>
/// Extension methods for configuring enhanced Swagger/OpenAPI documentation for controllers.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds enhanced Swagger/OpenAPI documentation with comprehensive configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to customize Swagger options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Default features:
    /// <list type="bullet">
    /// <item>JWT Bearer authentication support</item>
    /// <item>XML documentation comments inclusion</item>
    /// <item>API versioning support</item>
    /// <item>Detailed operation descriptions</item>
    /// <item>Example values for models</item>
    /// <item>Authorization requirements display</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Usage in Program.cs:</strong>
    /// <code>
    /// builder.Services.AddEnhancedSwagger();
    /// 
    /// var app = builder.Build();
    /// app.UseSwagger();
    /// app.UseSwaggerUI(options =>
    /// {
    ///     options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    ///     options.RoutePrefix = "swagger";
    /// });
    /// </code>
    /// </para>
    /// </remarks>
    /// <example>
    /// Custom configuration:
    /// <code>
    /// builder.Services.AddEnhancedSwagger(options =>
    /// {
    ///     options.SwaggerDoc("v1", new OpenApiInfo
    ///     {
    ///         Title = "My Custom API",
    ///         Version = "v1",
    ///         Description = "Custom description"
    ///     });
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddEnhancedSwagger(
        this IServiceCollection services,
        Action<SwaggerGenOptions>? configureOptions = null)
    {
        services.AddSwaggerGen(options =>
        {
            // Default API info
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Craft API",
                Version = "v1",
                Description = @"
# Craft API Documentation

A production-ready REST API built with Craft.Controllers.

## Features
- **CRUD Operations**: Complete Create, Read, Update, Delete operations
- **Pagination**: Built-in pagination support for list endpoints
- **Rate Limiting**: Request throttling for API stability
- **API Versioning**: Multiple API versions support
- **Authentication**: JWT Bearer token authentication
- **Error Handling**: Standardized error responses

## Getting Started
1. Obtain a JWT token from the `/api/auth/login` endpoint
2. Use the 'Authorize' button to set your bearer token
3. Explore the available endpoints

## Rate Limits
- **Read operations**: 100 requests/minute
- **Write operations**: 30 requests/minute
- **Delete operations**: 10 requests/minute
- **Bulk operations**: 5 requests/minute

## Response Codes
- **200 OK**: Request successful
- **201 Created**: Resource created successfully
- **400 Bad Request**: Invalid request data
- **401 Unauthorized**: Missing or invalid authentication
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **429 Too Many Requests**: Rate limit exceeded
- **500 Internal Server Error**: Server error
",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@example.com",
                    Url = new Uri("https://example.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                },
                TermsOfService = new Uri("https://example.com/terms")
            });

            // Add JWT Bearer authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = @"
JWT Authorization header using the Bearer scheme.

Enter your JWT token in the text input below.

Example: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'

**Note:** Do NOT include the 'Bearer' prefix. Just paste the token.
"
            });

            // Add API Key authentication
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Name = "X-Api-Key",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "API Key authentication. Enter your API key in the text input below."
            });

            // Apply security globally
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            // Include XML comments
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (var xmlFile in xmlFiles)
            {
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
            }

            // Use full type names to avoid conflicts
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

            // Add operation filters
            options.OperationFilter<AddResponseHeadersFilter>();
            options.OperationFilter<AddRateLimitingInfoFilter>();
            options.OperationFilter<AddAuthorizationInfoFilter>();

            // Add schema filters
            options.SchemaFilter<EnumSchemaFilter>();
            options.SchemaFilter<RequiredNotNullableSchemaFilter>();

            // Apply custom configuration if provided
            configureOptions?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI with enhanced options.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="routePrefix">The route prefix for Swagger UI (default: "swagger").</param>
    /// <returns>The web application for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Enhanced features:
    /// <list type="bullet">
    /// <item>Deep linking enabled</item>
    /// <item>Request duration display</item>
    /// <item>Try it out enabled by default</item>
    /// <item>Syntax highlighting</item>
    /// <item>Persistent authorization</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static IApplicationBuilder UseEnhancedSwaggerUI(
        this IApplicationBuilder app,
        string routePrefix = "swagger")
    {
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            options.RoutePrefix = routePrefix;
            
            // UI customization
            options.DocumentTitle = "Craft API Documentation";
            options.EnableDeepLinking();
            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();
            options.EnableFilter();
            options.ShowExtensions();
            
            // Persistence
            options.EnablePersistAuthorization();
            
            // Display settings
            options.DefaultModelsExpandDepth(2);
            options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            options.DisplayOperationId();
            
            // Custom CSS (optional)
            options.InjectStylesheet("/swagger-ui/custom.css");
        });

        return app;
    }
}

/// <summary>
/// Operation filter to add response headers information to Swagger documentation.
/// </summary>
internal class AddResponseHeadersFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add common response headers
        foreach (var response in operation.Responses.Values)
        {
            response.Headers ??= new Dictionary<string, OpenApiHeader>();

            if (!response.Headers.ContainsKey("X-Request-Id"))
            {
                response.Headers.Add("X-Request-Id", new OpenApiHeader
                {
                    Description = "Unique request identifier for tracking",
                    Schema = new OpenApiSchema { Type = "string" }
                });
            }

            if (!response.Headers.ContainsKey("X-RateLimit-Limit"))
            {
                response.Headers.Add("X-RateLimit-Limit", new OpenApiHeader
                {
                    Description = "Rate limit ceiling for the endpoint",
                    Schema = new OpenApiSchema { Type = "integer" }
                });
            }

            if (!response.Headers.ContainsKey("X-RateLimit-Remaining"))
            {
                response.Headers.Add("X-RateLimit-Remaining", new OpenApiHeader
                {
                    Description = "Remaining requests in the current window",
                    Schema = new OpenApiSchema { Type = "integer" }
                });
            }
        }

        // Add 429 response if rate limiting is applied
        var hasRateLimiting = context.MethodInfo.GetCustomAttributes(true)
            .Any(attr => attr.GetType().Name.Contains("EnableRateLimiting"));

        if (hasRateLimiting && !operation.Responses.ContainsKey("429"))
        {
            operation.Responses.Add("429", new OpenApiResponse
            {
                Description = "Too Many Requests - Rate limit exceeded",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["error"] = new OpenApiSchema { Type = "string" },
                                ["message"] = new OpenApiSchema { Type = "string" },
                                ["retryAfter"] = new OpenApiSchema { Type = "number" }
                            }
                        }
                    }
                },
                Headers = new Dictionary<string, OpenApiHeader>
                {
                    ["Retry-After"] = new OpenApiHeader
                    {
                        Description = "Seconds to wait before retrying",
                        Schema = new OpenApiSchema { Type = "integer" }
                    }
                }
            });
        }
    }
}

/// <summary>
/// Operation filter to add rate limiting information to operation descriptions.
/// </summary>
internal class AddRateLimitingInfoFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var rateLimitAttribute = context.MethodInfo.GetCustomAttributes(true)
            .FirstOrDefault(attr => attr.GetType().Name.Contains("EnableRateLimiting"));

        if (rateLimitAttribute != null)
        {
            var policy = rateLimitAttribute.GetType().GetProperty("PolicyName")?.GetValue(rateLimitAttribute)?.ToString();
            
            var rateLimitInfo = policy switch
            {
                "read-policy" => "**Rate Limit**: 100 requests per minute",
                "write-policy" => "**Rate Limit**: 30 requests per minute",
                "delete-policy" => "**Rate Limit**: 10 requests per minute",
                "bulk-policy" => "**Rate Limit**: 5 requests per minute",
                _ => "**Rate Limit**: Applied"
            };

            operation.Description = $"{operation.Description}\n\n{rateLimitInfo}";
        }
    }
}

/// <summary>
/// Operation filter to add authorization information to operation descriptions.
/// </summary>
internal class AddAuthorizationInfoFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authorizeAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

        if (authorizeAttributes?.Any() == true)
        {
            var roles = authorizeAttributes
                .Where(a => !string.IsNullOrEmpty(a.Roles))
                .Select(a => a.Roles)
                .ToList();

            var policies = authorizeAttributes
                .Where(a => !string.IsNullOrEmpty(a.Policy))
                .Select(a => a.Policy)
                .ToList();

            if (roles.Any() || policies.Any())
            {
                var authInfo = new List<string>();
                
                if (roles.Any())
                    authInfo.Add($"**Required Roles**: {string.Join(", ", roles)}");
                
                if (policies.Any())
                    authInfo.Add($"**Required Policies**: {string.Join(", ", policies)}");

                operation.Description = $"{operation.Description}\n\n{string.Join("\n\n", authInfo)}";
            }

            // Add 401 and 403 responses
            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Unauthorized - Authentication required"
                });
            }

            if (!operation.Responses.ContainsKey("403"))
            {
                operation.Responses.Add("403", new OpenApiResponse
                {
                    Description = "Forbidden - Insufficient permissions"
                });
            }
        }
    }
}

/// <summary>
/// Schema filter to add enum value descriptions.
/// </summary>
internal class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumValues = Enum.GetValues(context.Type);
            
            foreach (var value in enumValues)
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(value.ToString()));
            }
            
            schema.Type = "string";
            schema.Format = null;
        }
    }
}

/// <summary>
/// Schema filter to mark required non-nullable properties.
/// </summary>
internal class RequiredNotNullableSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null)
            return;

        var notNullableProperties = context.Type
            .GetProperties()
            .Where(p => !IsNullable(p.PropertyType))
            .Select(p => p.Name.ToLowerInvariant());

        foreach (var property in notNullableProperties)
        {
            if (schema.Required?.Contains(property) != true)
            {
                schema.Required ??= new HashSet<string>();
                schema.Required.Add(property);
            }
        }
    }

    private static bool IsNullable(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null ||
               !type.IsValueType ||
               type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}
