using System.ComponentModel.DataAnnotations;

namespace Craft.OpenAPI.Tests;

/// <summary>
/// Tests for SwaggerOptions validation and configuration.
/// </summary>
public class SwaggerOptionsTests
{
    [Fact]
    public void SectionName_Should_Be_SwaggerOptions()
    {
        Assert.Equal("SwaggerOptions", SwaggerOptions.SectionName);
    }

    [Fact]
    public void Default_Values_Should_Be_Set_Correctly()
    {
        // Arrange & Act
        var options = new SwaggerOptions();

        // Assert
        Assert.True(options.Enable);
        Assert.Equal("API Documentation", options.Title);
        Assert.Equal("v1", options.Version);
        Assert.Equal("API Documentation", options.Description);
        Assert.Equal("swagger", options.RoutePrefix);
        Assert.False(options.EnableInProduction);
        Assert.True(options.IncludeXmlComments);
        Assert.Empty(options.XmlDocumentationFiles);
        Assert.Empty(options.Servers);
        Assert.NotNull(options.Security);
        Assert.NotNull(options.UI);
        Assert.NotNull(options.Documentation);
    }

    [Fact]
    public void Validate_Should_Pass_When_Enabled_With_Valid_Properties()
    {
        // Arrange
        var options = new SwaggerOptions
        {
            Enable = true,
            Title = "Test API",
            Version = "v1",
            Description = "Test Description"
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_Should_Pass_When_Disabled()
    {
        // Arrange
        var options = new SwaggerOptions
        {
            Enable = false,
            Title = string.Empty,
            Version = string.Empty
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_Should_Fail_When_Title_Is_Empty_And_Enabled(string? title)
    {
        // Arrange
        var options = new SwaggerOptions
        {
            Enable = true,
            Title = title!,
            Version = "v1"
        };

        // Act
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));

        // Assert
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(SwaggerOptions.Title)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_Should_Fail_When_Version_Is_Empty_And_Enabled(string? version)
    {
        // Arrange
        var options = new SwaggerOptions
        {
            Enable = true,
            Title = "Test API",
            Version = version!
        };

        // Act
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));

        // Assert
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(SwaggerOptions.Version)));
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("not a url")]
    public void Validate_Should_Fail_When_ContactUrl_Is_Invalid(string contactUrl)
    {
        // Arrange
        var options = new SwaggerOptions
        {
            Enable = true,
            Title = "Test API",
            Version = "v1",
            ContactUrl = contactUrl
        };

        // Act
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));

        // Assert
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(SwaggerOptions.ContactUrl)));
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("not a url")]
    public void Validate_Should_Fail_When_LicenseUrl_Is_Invalid(string licenseUrl)
    {
        // Arrange
        var options = new SwaggerOptions
        {
            Enable = true,
            Title = "Test API",
            Version = "v1",
            LicenseUrl = licenseUrl
        };

        // Act
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));

        // Assert
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(SwaggerOptions.LicenseUrl)));
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("not a url")]
    public void Validate_Should_Fail_When_TermsOfService_Is_Invalid(string termsOfService)
    {
        // Arrange
        var options = new SwaggerOptions
        {
            Enable = true,
            Title = "Test API",
            Version = "v1",
            TermsOfService = termsOfService
        };

        // Act
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));

        // Assert
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(SwaggerOptions.TermsOfService)));
    }

    [Fact]
    public void Validate_Should_Pass_With_Valid_URLs()
    {
        // Arrange
        var options = new SwaggerOptions
        {
            Enable = true,
            Title = "Test API",
            Version = "v1",
            ContactUrl = "https://example.com/contact",
            LicenseUrl = "https://example.com/license",
            TermsOfService = "https://example.com/terms"
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Properties_Can_Be_Set_And_Retrieved()
    {
        // Arrange
        var options = new SwaggerOptions
        {
            Enable = false,
            Title = "Custom API",
            Version = "v2",
            Description = "Custom Description",
            ContactName = "John Doe",
            ContactEmail = "john@example.com",
            ContactUrl = "https://example.com",
            LicenseName = "MIT",
            LicenseUrl = "https://opensource.org/licenses/MIT",
            TermsOfService = "https://example.com/terms",
            RoutePrefix = "api-docs",
            EnableInProduction = true,
            IncludeXmlComments = false
        };

        // Assert
        Assert.False(options.Enable);
        Assert.Equal("Custom API", options.Title);
        Assert.Equal("v2", options.Version);
        Assert.Equal("Custom Description", options.Description);
        Assert.Equal("John Doe", options.ContactName);
        Assert.Equal("john@example.com", options.ContactEmail);
        Assert.Equal("https://example.com", options.ContactUrl);
        Assert.Equal("MIT", options.LicenseName);
        Assert.Equal("https://opensource.org/licenses/MIT", options.LicenseUrl);
        Assert.Equal("https://example.com/terms", options.TermsOfService);
        Assert.Equal("api-docs", options.RoutePrefix);
        Assert.True(options.EnableInProduction);
        Assert.False(options.IncludeXmlComments);
    }
}

/// <summary>
/// Tests for ServerUrl validation.
/// </summary>
public class ServerUrlTests
{
    [Fact]
    public void ServerUrl_Should_Validate_Required_Url()
    {
        // Arrange
        var serverUrl = new ServerUrl
        {
            Url = string.Empty,
            Description = "Test Server"
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(serverUrl);
        var isValid = Validator.TryValidateObject(serverUrl, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ServerUrl.Url)));
    }

    [Fact]
    public void ServerUrl_Should_Validate_Valid_Url()
    {
        // Arrange
        var serverUrl = new ServerUrl
        {
            Url = "https://api.example.com",
            Description = "Production Server"
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(serverUrl);
        var isValid = Validator.TryValidateObject(serverUrl, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }
}

/// <summary>
/// Tests for SecurityOptions configuration.
/// </summary>
public class SecurityOptionsTests
{
    [Fact]
    public void Default_Values_Should_Be_Set_Correctly()
    {
        // Arrange & Act
        var options = new SecurityOptions();

        // Assert
        Assert.True(options.EnableJwtBearer);
        Assert.Equal("Bearer", options.JwtBearerSchemeName);
        Assert.Equal("Enter JWT Bearer token", options.JwtBearerDescription);
        Assert.False(options.EnableApiKey);
        Assert.Equal("ApiKey", options.ApiKeySchemeName);
        Assert.Equal("X-API-Key", options.ApiKeyHeaderName);
        Assert.Equal("Enter API Key", options.ApiKeyDescription);
        Assert.False(options.EnableOAuth2);
        Assert.Null(options.OAuth2);
    }

    [Fact]
    public void Properties_Can_Be_Set_And_Retrieved()
    {
        // Arrange
        var options = new SecurityOptions
        {
            EnableJwtBearer = false,
            JwtBearerSchemeName = "CustomBearer",
            JwtBearerDescription = "Custom JWT Description",
            EnableApiKey = true,
            ApiKeySchemeName = "CustomApiKey",
            ApiKeyHeaderName = "X-Custom-Key",
            ApiKeyDescription = "Custom API Key Description",
            EnableOAuth2 = true,
            OAuth2 = new OAuth2Options
            {
                AuthorizationUrl = "https://auth.example.com/authorize",
                TokenUrl = "https://auth.example.com/token"
            }
        };

        // Assert
        Assert.False(options.EnableJwtBearer);
        Assert.Equal("CustomBearer", options.JwtBearerSchemeName);
        Assert.Equal("Custom JWT Description", options.JwtBearerDescription);
        Assert.True(options.EnableApiKey);
        Assert.Equal("CustomApiKey", options.ApiKeySchemeName);
        Assert.Equal("X-Custom-Key", options.ApiKeyHeaderName);
        Assert.Equal("Custom API Key Description", options.ApiKeyDescription);
        Assert.True(options.EnableOAuth2);
        Assert.NotNull(options.OAuth2);
    }
}

/// <summary>
/// Tests for OAuth2Options validation.
/// </summary>
public class OAuth2OptionsTests
{
    [Fact]
    public void OAuth2Options_Should_Validate_Required_Urls()
    {
        // Arrange
        var options = new OAuth2Options
        {
            AuthorizationUrl = string.Empty,
            TokenUrl = string.Empty
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(OAuth2Options.AuthorizationUrl)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(OAuth2Options.TokenUrl)));
    }

    [Fact]
    public void OAuth2Options_Should_Validate_Valid_Configuration()
    {
        // Arrange
        var options = new OAuth2Options
        {
            AuthorizationUrl = "https://auth.example.com/authorize",
            TokenUrl = "https://auth.example.com/token",
            Scopes = new Dictionary<string, string>
            {
                ["read"] = "Read access",
                ["write"] = "Write access"
            },
            ClientId = "client-id",
            ClientSecret = "client-secret"
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }
}

/// <summary>
/// Tests for SwaggerUIOptions configuration.
/// </summary>
public class SwaggerUIOptionsTests
{
    [Fact]
    public void Default_Values_Should_Be_Set_Correctly()
    {
        // Arrange & Act
        var options = new SwaggerCustomUIOptions();

        // Assert
        Assert.Equal("API Documentation", options.DocumentTitle);
        Assert.True(options.EnableDeepLinking);
        Assert.False(options.DisplayOperationId);
        Assert.True(options.DisplayRequestDuration);
        Assert.Equal(1, options.DefaultModelsExpandDepth);
        Assert.True(options.EnableFilter);
        Assert.False(options.EnableTryItOutByDefault);
        Assert.True(options.PersistAuthorization);
        Assert.Equal("none", options.DocExpansion);
        Assert.Equal("example", options.DefaultModelRendering);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(11)]
    public void SwaggerUIOptions_Should_Validate_DefaultModelsExpandDepth_Range(int depth)
    {
        // Arrange
        var options = new SwaggerCustomUIOptions
        {
            DefaultModelsExpandDepth = depth
        };

        // Act
        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(SwaggerCustomUIOptions.DefaultModelsExpandDepth)));
    }

    [Fact]
    public void SwaggerUIOptions_Properties_Can_Be_Set()
    {
        // Arrange
        var options = new SwaggerCustomUIOptions
        {
            DocumentTitle = "Custom API Docs",
            HeadContent = "<meta name='custom' content='value'>",
            EnableDeepLinking = false,
            DisplayOperationId = true,
            DisplayRequestDuration = false,
            DefaultModelsExpandDepth = 5,
            EnableFilter = false,
            EnableTryItOutByDefault = true,
            PersistAuthorization = false,
            DocExpansion = "full",
            DefaultModelRendering = "model",
            CustomCssUrl = "https://example.com/custom.css",
            InlineCustomCss = ".swagger-ui { color: red; }",
            CustomJavaScriptUrl = "https://example.com/custom.js"
        };

        // Assert
        Assert.Equal("Custom API Docs", options.DocumentTitle);
        Assert.Equal("<meta name='custom' content='value'>", options.HeadContent);
        Assert.False(options.EnableDeepLinking);
        Assert.True(options.DisplayOperationId);
        Assert.False(options.DisplayRequestDuration);
        Assert.Equal(5, options.DefaultModelsExpandDepth);
        Assert.False(options.EnableFilter);
        Assert.True(options.EnableTryItOutByDefault);
        Assert.False(options.PersistAuthorization);
        Assert.Equal("full", options.DocExpansion);
        Assert.Equal("model", options.DefaultModelRendering);
        Assert.Equal("https://example.com/custom.css", options.CustomCssUrl);
        Assert.Equal(".swagger-ui { color: red; }", options.InlineCustomCss);
        Assert.Equal("https://example.com/custom.js", options.CustomJavaScriptUrl);
    }
}

/// <summary>
/// Tests for DocumentationOptions configuration.
/// </summary>
public class DocumentationOptionsTests
{
    [Fact]
    public void Default_Values_Should_Be_Set_Correctly()
    {
        // Arrange & Act
        var options = new DocumentationOptions();

        // Assert
        Assert.True(options.IgnoreObsoleteActions);
        Assert.True(options.IgnoreObsoleteProperties);
        Assert.True(options.ShowEnumDescriptions);
        Assert.True(options.ShowDefaultValues);
        Assert.False(options.UseAllOfForRequired);
        Assert.True(options.UseOneOfForPolymorphism);
        Assert.False(options.UseInlineDefinitionsForEnums);
        Assert.Empty(options.TagDescriptions);
    }

    [Fact]
    public void Properties_Can_Be_Set_And_Retrieved()
    {
        // Arrange
        var options = new DocumentationOptions
        {
            IgnoreObsoleteActions = false,
            IgnoreObsoleteProperties = false,
            ShowEnumDescriptions = false,
            ShowDefaultValues = false,
            UseAllOfForRequired = true,
            UseOneOfForPolymorphism = false,
            UseInlineDefinitionsForEnums = true,
            TagDescriptions = new Dictionary<string, string>
            {
                ["Users"] = "User management endpoints",
                ["Products"] = "Product management endpoints"
            }
        };

        // Assert
        Assert.False(options.IgnoreObsoleteActions);
        Assert.False(options.IgnoreObsoleteProperties);
        Assert.False(options.ShowEnumDescriptions);
        Assert.False(options.ShowDefaultValues);
        Assert.True(options.UseAllOfForRequired);
        Assert.False(options.UseOneOfForPolymorphism);
        Assert.True(options.UseInlineDefinitionsForEnums);
        Assert.Equal(2, options.TagDescriptions.Count);
        Assert.Equal("User management endpoints", options.TagDescriptions["Users"]);
        Assert.Equal("Product management endpoints", options.TagDescriptions["Products"]);
    }
}
