using System.Reflection;
using Craft.Infrastructure.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Extension methods for configuring OpenAPI/Swagger documentation.
/// </summary>
public static class OpenApiExtensions
{
    private const string DefaultSecuritySchemeFormat = "JWT";

    /// <summary>
    /// Adds OpenAPI documentation services with configuration from appsettings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing SwaggerOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddOpenApiDocumentation(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        
        return services.AddOpenApiDocumentation(
            configuration.GetSection(SwaggerOptions.SectionName));
    }

    /// <summary>
    /// Adds OpenAPI documentation services with a configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSection">The configuration section containing SwaggerOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurationSection);

        services.AddOptions<SwaggerOptions>()
            .Bind(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = configurationSection.Get<SwaggerOptions>();

        if (options?.Enable != true)
            return services;

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(swaggerOptions =>
        {
            ConfigureSwaggerDoc(swaggerOptions, options);
            ConfigureDocumentation(swaggerOptions, options);
            ConfigureSecurity(swaggerOptions, options.Security);
            ConfigureXmlComments(swaggerOptions, options);
        });

        return services;
    }

    /// <summary>
    /// Adds OpenAPI documentation services with a configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure Swagger options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddOpenApiDocumentation(options =>
    /// {
    ///     options.Title = "My API";
    ///     options.Version = "v1";
    ///     options.Description = "My API Documentation";
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, Action<SwaggerOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddOptions<SwaggerOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = new SwaggerOptions();
        configureOptions(options);

        if (!options.Enable)
            return services;

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(swaggerOptions =>
        {
            ConfigureSwaggerDoc(swaggerOptions, options);
            ConfigureDocumentation(swaggerOptions, options);
            ConfigureSecurity(swaggerOptions, options.Security);
            ConfigureXmlComments(swaggerOptions, options);
        });

        return services;
    }

    /// <summary>
    /// Uses OpenAPI documentation middleware based on environment and configuration.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The application builder for chaining.</returns>
    /// <example>
    /// <code>
    /// app.UseOpenApiDocumentation();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseOpenApiDocumentation(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = app.Services.GetService<IOptions<SwaggerOptions>>()?.Value;

        if (options?.Enable != true)
            return app;

        var logger = app.Services.GetService<ILogger<SwaggerOptions>>();

        var shouldEnable = ShouldEnableSwagger(app.Environment, options);

        if (!shouldEnable)
        {
            logger?.LogInformation("Swagger/OpenAPI is disabled in {Environment} environment", app.Environment.EnvironmentName);
            return app;
        }

        app.UseSwagger(swaggerOptions =>
        {
            swaggerOptions.RouteTemplate = $"{options.RoutePrefix}/{{documentName}}/swagger.json";
        });

        app.UseSwaggerUI(uiOptions =>
        {
            ConfigureSwaggerUI(uiOptions, options);
        });

        logger?.LogInformation(
            "Swagger/OpenAPI enabled at /{RoutePrefix}",
            options.RoutePrefix);

        return app;
    }

    #region Private Helper Methods

    private static bool ShouldEnableSwagger(IWebHostEnvironment environment, SwaggerOptions options)
    {
        if (environment.IsDevelopment() || environment.IsStaging())
            return true;

        return options.EnableInProduction && environment.IsProduction();
    }

    private static void ConfigureSwaggerDoc(SwaggerGenOptions swaggerOptions, SwaggerOptions options)
    {
        var openApiInfo = new OpenApiInfo
        {
            Title = options.Title,
            Version = options.Version,
            Description = options.Description
        };

        if (!string.IsNullOrWhiteSpace(options.ContactName) ||
            !string.IsNullOrWhiteSpace(options.ContactEmail) ||
            !string.IsNullOrWhiteSpace(options.ContactUrl))
        {
            openApiInfo.Contact = new OpenApiContact
            {
                Name = options.ContactName,
                Email = options.ContactEmail,
                Url = string.IsNullOrWhiteSpace(options.ContactUrl)
                    ? null
                    : new Uri(options.ContactUrl)
            };
        }

        if (!string.IsNullOrWhiteSpace(options.LicenseName))
        {
            openApiInfo.License = new OpenApiLicense
            {
                Name = options.LicenseName,
                Url = string.IsNullOrWhiteSpace(options.LicenseUrl)
                    ? null
                    : new Uri(options.LicenseUrl)
            };
        }

        if (!string.IsNullOrWhiteSpace(options.TermsOfService))
        {
            openApiInfo.TermsOfService = new Uri(options.TermsOfService);
        }

        swaggerOptions.SwaggerDoc(options.Version, openApiInfo);

        if (options.Servers.Count > 0)
        {
            swaggerOptions.AddServer(new OpenApiServer
            {
                Url = options.Servers[0].Url,
                Description = options.Servers[0].Description
            });

            for (int i = 1; i < options.Servers.Count; i++)
            {
                swaggerOptions.AddServer(new OpenApiServer
                {
                    Url = options.Servers[i].Url,
                    Description = options.Servers[i].Description
                });
            }
        }
    }

    private static void ConfigureDocumentation(SwaggerGenOptions swaggerOptions, SwaggerOptions options)
    {
        var docOptions = options.Documentation;

        if (docOptions.IgnoreObsoleteActions)
            swaggerOptions.IgnoreObsoleteActions();

        if (docOptions.IgnoreObsoleteProperties)
            swaggerOptions.IgnoreObsoleteProperties();

        swaggerOptions.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

        if (docOptions.UseOneOfForPolymorphism)
            swaggerOptions.UseOneOfForPolymorphism();

        if (docOptions.UseInlineDefinitionsForEnums)
            swaggerOptions.UseInlineDefinitionsForEnums();

        if (docOptions.TagDescriptions.Count > 0)
        {
            swaggerOptions.DocumentFilter<TagDescriptionDocumentFilter>(docOptions.TagDescriptions);
        }
    }

    private static void ConfigureSecurity(SwaggerGenOptions swaggerOptions, SecurityOptions securityOptions)
    {
        if (securityOptions.EnableJwtBearer)
        {
            var jwtScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = securityOptions.JwtBearerDescription,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = DefaultSecuritySchemeFormat,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = securityOptions.JwtBearerSchemeName
                }
            };

            swaggerOptions.AddSecurityDefinition(securityOptions.JwtBearerSchemeName, jwtScheme);
            swaggerOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtScheme, Array.Empty<string>() }
            });
        }

        if (securityOptions.EnableApiKey)
        {
            var apiKeyScheme = new OpenApiSecurityScheme
            {
                Name = securityOptions.ApiKeyHeaderName,
                Description = securityOptions.ApiKeyDescription,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = securityOptions.ApiKeySchemeName
                }
            };

            swaggerOptions.AddSecurityDefinition(securityOptions.ApiKeySchemeName, apiKeyScheme);
            swaggerOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { apiKeyScheme, Array.Empty<string>() }
            });
        }

        if (securityOptions.EnableOAuth2 && securityOptions.OAuth2 != null)
        {
            var oauth2Scheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(securityOptions.OAuth2.AuthorizationUrl),
                        TokenUrl = new Uri(securityOptions.OAuth2.TokenUrl),
                        Scopes = securityOptions.OAuth2.Scopes
                    }
                },
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            };

            swaggerOptions.AddSecurityDefinition("oauth2", oauth2Scheme);
            swaggerOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { oauth2Scheme, securityOptions.OAuth2.Scopes.Keys.ToArray() }
            });
        }
    }

    private static void ConfigureXmlComments(SwaggerGenOptions swaggerOptions, SwaggerOptions options)
    {
        if (!options.IncludeXmlComments)
            return;

        if (options.XmlDocumentationFiles.Count > 0)
        {
            foreach (var xmlFile in options.XmlDocumentationFiles)
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    swaggerOptions.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
            }
        }
        else
        {
            var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                swaggerOptions.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        }
    }

    private static void ConfigureSwaggerUI(Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions uiOptions, SwaggerOptions options)
    {
        var ui = options.UI;

        uiOptions.SwaggerEndpoint(
            $"/{options.RoutePrefix}/{options.Version}/swagger.json",
            $"{options.Title} {options.Version}");

        uiOptions.RoutePrefix = options.RoutePrefix;
        uiOptions.DocumentTitle = ui.DocumentTitle;

        if (ui.EnableDeepLinking)
            uiOptions.EnableDeepLinking();

        if (ui.DisplayOperationId)
            uiOptions.DisplayOperationId();

        if (ui.DisplayRequestDuration)
            uiOptions.DisplayRequestDuration();

        uiOptions.DefaultModelsExpandDepth(ui.DefaultModelsExpandDepth);

        if (ui.EnableFilter)
            uiOptions.EnableFilter();

        if (ui.PersistAuthorization)
            uiOptions.EnablePersistAuthorization();

        uiOptions.DocExpansion(ParseDocExpansion(ui.DocExpansion));
        uiOptions.DefaultModelRendering(ParseModelRendering(ui.DefaultModelRendering));

        if (!string.IsNullOrWhiteSpace(ui.CustomCssUrl))
            uiOptions.InjectStylesheet(ui.CustomCssUrl);

        if (!string.IsNullOrWhiteSpace(ui.InlineCustomCss))
        {
            uiOptions.InjectStylesheet("/swagger-ui/custom.css");
        }

        if (!string.IsNullOrWhiteSpace(ui.CustomJavaScriptUrl))
            uiOptions.InjectJavascript(ui.CustomJavaScriptUrl);

        if (!string.IsNullOrWhiteSpace(ui.HeadContent))
        {
            uiOptions.HeadContent = ui.HeadContent;
        }
    }

    private static DocExpansion ParseDocExpansion(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "list" => DocExpansion.List,
            "full" => DocExpansion.Full,
            _ => DocExpansion.None
        };
    }

    private static ModelRendering ParseModelRendering(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "model" => ModelRendering.Model,
            _ => ModelRendering.Example
        };
    }

    #endregion

    #region Document Filters

    /// <summary>
    /// Document filter for adding tag descriptions.
    /// </summary>
    private sealed class TagDescriptionDocumentFilter : IDocumentFilter
    {
        private readonly Dictionary<string, string> _tagDescriptions;

        public TagDescriptionDocumentFilter(Dictionary<string, string> tagDescriptions)
        {
            _tagDescriptions = tagDescriptions ?? new Dictionary<string, string>();
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Tags == null)
                swaggerDoc.Tags = new List<OpenApiTag>();

            foreach (var tagDescription in _tagDescriptions)
            {
                var existingTag = swaggerDoc.Tags.FirstOrDefault(t =>
                    t.Name.Equals(tagDescription.Key, StringComparison.OrdinalIgnoreCase));

                if (existingTag != null)
                {
                    existingTag.Description = tagDescription.Value;
                }
                else
                {
                    swaggerDoc.Tags.Add(new OpenApiTag
                    {
                        Name = tagDescription.Key,
                        Description = tagDescription.Value
                    });
                }
            }
        }
    }

    #endregion
}
