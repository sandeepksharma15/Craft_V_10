using System.ComponentModel.DataAnnotations;

namespace Craft.OpenAPI;

/// <summary>
/// Configuration options for OpenAPI/Swagger documentation.
/// </summary>
public class SwaggerOptions : IValidatableObject
{
    public const string SectionName = "SwaggerOptions";

    /// <summary>
    /// Gets or sets a value indicating whether Swagger/OpenAPI documentation is enabled.
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// Gets or sets the API title.
    /// </summary>
    public string Title { get; set; } = "API Documentation";

    /// <summary>
    /// Gets or sets the API version.
    /// </summary>
    public string Version { get; set; } = "v1";

    /// <summary>
    /// Gets or sets the API description.
    /// </summary>
    public string Description { get; set; } = "API Documentation";

    /// <summary>
    /// Gets or sets the contact name for the API.
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// Gets or sets the contact email for the API.
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid contact email address")]
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Gets or sets the contact URL for the API.
    /// </summary>
    [Url(ErrorMessage = "Invalid contact URL")]
    public string? ContactUrl { get; set; }

    /// <summary>
    /// Gets or sets the license name for the API.
    /// </summary>
    public string? LicenseName { get; set; }

    /// <summary>
    /// Gets or sets the license URL for the API.
    /// </summary>
    [Url(ErrorMessage = "Invalid license URL")]
    public string? LicenseUrl { get; set; }

    /// <summary>
    /// Gets or sets the terms of service URL.
    /// </summary>
    [Url(ErrorMessage = "Invalid terms of service URL")]
    public string? TermsOfService { get; set; }

    /// <summary>
    /// Gets or sets the route prefix for Swagger UI (default: "swagger").
    /// </summary>
    public string RoutePrefix { get; set; } = "swagger";

    /// <summary>
    /// Gets or sets a value indicating whether to enable Swagger in production.
    /// </summary>
    public bool EnableInProduction { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include XML comments documentation.
    /// </summary>
    public bool IncludeXmlComments { get; set; } = true;

    /// <summary>
    /// Gets or sets the XML documentation file paths (relative to app base directory).
    /// </summary>
    public List<string> XmlDocumentationFiles { get; set; } = [];

    /// <summary>
    /// Gets or sets server URLs for different environments.
    /// </summary>
    public List<ServerUrl> Servers { get; set; } = [];

    /// <summary>
    /// Gets or sets security scheme configurations.
    /// </summary>
    public SecurityOptions Security { get; set; } = new();

    /// <summary>
    /// Gets or sets UI customization options.
    /// </summary>
    public SwaggerCustomUIOptions UI { get; set; } = new();

    /// <summary>
    /// Gets or sets documentation enhancement options.
    /// </summary>
    public DocumentationOptions Documentation { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Only validate required fields if Swagger is enabled
        if (Enable)
        {
            if (string.IsNullOrWhiteSpace(Title))
                yield return new ValidationResult(
                    "API title is required when Swagger is enabled",
                    [nameof(Title)]);

            if (string.IsNullOrWhiteSpace(Version))
                yield return new ValidationResult(
                    "API version is required when Swagger is enabled",
                    [nameof(Version)]);
        }

        // URL validation applies regardless of Enable status
        if (!string.IsNullOrWhiteSpace(ContactUrl) && !Uri.TryCreate(ContactUrl, UriKind.Absolute, out _))
            yield return new ValidationResult(
                "Contact URL must be a valid absolute URI",
                [nameof(ContactUrl)]);

        if (!string.IsNullOrWhiteSpace(LicenseUrl) && !Uri.TryCreate(LicenseUrl, UriKind.Absolute, out _))
            yield return new ValidationResult(
                "License URL must be a valid absolute URI",
                [nameof(LicenseUrl)]);

        if (!string.IsNullOrWhiteSpace(TermsOfService) && !Uri.TryCreate(TermsOfService, UriKind.Absolute, out _))
            yield return new ValidationResult(
                "Terms of Service URL must be a valid absolute URI",
                [nameof(TermsOfService)]);
    }
}

/// <summary>
/// Server URL configuration for different environments.
/// </summary>
public class ServerUrl
{
    /// <summary>
    /// Gets or sets the server URL.
    /// </summary>
    [Required(ErrorMessage = "Server URL is required")]
    [Url(ErrorMessage = "Invalid server URL")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the server description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Security configuration options for OpenAPI.
/// </summary>
public class SecurityOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to enable JWT Bearer authentication.
    /// </summary>
    public bool EnableJwtBearer { get; set; } = true;

    /// <summary>
    /// Gets or sets the JWT Bearer scheme name.
    /// </summary>
    public string JwtBearerSchemeName { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the JWT Bearer description.
    /// </summary>
    public string JwtBearerDescription { get; set; } = "Enter JWT Bearer token";

    /// <summary>
    /// Gets or sets a value indicating whether to enable API Key authentication.
    /// </summary>
    public bool EnableApiKey { get; set; } = false;

    /// <summary>
    /// Gets or sets the API Key scheme name.
    /// </summary>
    public string ApiKeySchemeName { get; set; } = "ApiKey";

    /// <summary>
    /// Gets or sets the API Key header name.
    /// </summary>
    public string ApiKeyHeaderName { get; set; } = "X-API-Key";

    /// <summary>
    /// Gets or sets the API Key description.
    /// </summary>
    public string ApiKeyDescription { get; set; } = "Enter API Key";

    /// <summary>
    /// Gets or sets a value indicating whether to enable OAuth2 authentication.
    /// </summary>
    public bool EnableOAuth2 { get; set; } = false;

    /// <summary>
    /// Gets or sets OAuth2 configuration.
    /// </summary>
    public OAuth2Options? OAuth2 { get; set; }
}

/// <summary>
/// OAuth2 configuration options.
/// </summary>
public class OAuth2Options
{
    /// <summary>
    /// Gets or sets the authorization URL.
    /// </summary>
    [Required(ErrorMessage = "Authorization URL is required for OAuth2")]
    [Url(ErrorMessage = "Invalid authorization URL")]
    public string AuthorizationUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token URL.
    /// </summary>
    [Required(ErrorMessage = "Token URL is required for OAuth2")]
    [Url(ErrorMessage = "Invalid token URL")]
    public string TokenUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth2 scopes.
    /// </summary>
    public Dictionary<string, string> Scopes { get; set; } = [];

    /// <summary>
    /// Gets or sets the OAuth2 client ID.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the OAuth2 client secret.
    /// </summary>
    public string? ClientSecret { get; set; }
}

/// <summary>
/// Swagger UI customization options.
/// </summary>
public class SwaggerCustomUIOptions
{
    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string DocumentTitle { get; set; } = "API Documentation";

    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    public string? HeadContent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable deep linking.
    /// </summary>
    public bool EnableDeepLinking { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to display operation ID.
    /// </summary>
    public bool DisplayOperationId { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to display request duration.
    /// </summary>
    public bool DisplayRequestDuration { get; set; } = true;

    /// <summary>
    /// Gets or sets the default models expand depth.
    /// </summary>
    [Range(-1, 10, ErrorMessage = "DefaultModelsExpandDepth must be between -1 and 10")]
    public int DefaultModelsExpandDepth { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether to enable filtering.
    /// </summary>
    public bool EnableFilter { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable "Try it out" by default.
    /// </summary>
    public bool EnableTryItOutByDefault { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to persist authorization.
    /// </summary>
    public bool PersistAuthorization { get; set; } = true;

    /// <summary>
    /// Gets or sets the doc expansion setting ("none", "list", "full").
    /// </summary>
    public string DocExpansion { get; set; } = "none";

    /// <summary>
    /// Gets or sets the default model rendering ("example" or "model").
    /// </summary>
    public string DefaultModelRendering { get; set; } = "example";

    /// <summary>
    /// Gets or sets custom CSS URL.
    /// </summary>
    [Url(ErrorMessage = "Invalid custom CSS URL")]
    public string? CustomCssUrl { get; set; }

    /// <summary>
    /// Gets or sets inline custom CSS.
    /// </summary>
    public string? InlineCustomCss { get; set; }

    /// <summary>
    /// Gets or sets custom JavaScript URL.
    /// </summary>
    [Url(ErrorMessage = "Invalid custom JavaScript URL")]
    public string? CustomJavaScriptUrl { get; set; }
}

/// <summary>
/// Documentation enhancement options.
/// </summary>
public class DocumentationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to ignore obsolete actions.
    /// </summary>
    public bool IgnoreObsoleteActions { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to ignore obsolete properties.
    /// </summary>
    public bool IgnoreObsoleteProperties { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show enum descriptions.
    /// </summary>
    public bool ShowEnumDescriptions { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show default values.
    /// </summary>
    public bool ShowDefaultValues { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use all of properties for required.
    /// </summary>
    public bool UseAllOfForRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to use one of for polymorphism.
    /// </summary>
    public bool UseOneOfForPolymorphism { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use inline definitions for enums.
    /// </summary>
    public bool UseInlineDefinitionsForEnums { get; set; } = false;

    /// <summary>
    /// Gets or sets tag descriptions.
    /// </summary>
    public Dictionary<string, string> TagDescriptions { get; set; } = [];
}

