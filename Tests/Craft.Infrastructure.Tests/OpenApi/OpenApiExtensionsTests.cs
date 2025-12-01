using Craft.Infrastructure.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Infrastructure.Tests.OpenApi;

/// <summary>
/// Integration tests for OpenApiExtensions.
/// </summary>
public class OpenApiExtensionsTests
{
    [Fact]
    public void AddOpenApiDocumentation_WithConfiguration_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(enabled: true);

        // Act
        services.AddOpenApiDocumentation(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<SwaggerOptions>>();
        Assert.NotNull(options);
        Assert.NotNull(options.Value);
    }

    [Fact]
    public void AddOpenApiDocumentation_WithConfigurationSection_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(enabled: true);
        var section = configuration.GetSection(SwaggerOptions.SectionName);

        // Act
        services.AddOpenApiDocumentation(section);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<SwaggerOptions>>();
        Assert.NotNull(options);
        Assert.Equal("Test API", options.Value.Title);
    }

    [Fact]
    public void AddOpenApiDocumentation_WithAction_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenApiDocumentation(options =>
        {
            options.Enable = true;
            options.Title = "Action API";
            options.Version = "v2";
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<SwaggerOptions>>();
        Assert.NotNull(options);
        Assert.Equal("Action API", options.Value.Title);
        Assert.Equal("v2", options.Value.Version);
    }

    [Fact]
    public void AddOpenApiDocumentation_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;
        var configuration = CreateConfiguration(enabled: true);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services!.AddOpenApiDocumentation(configuration));
    }

    [Fact]
    public void AddOpenApiDocumentation_ThrowsArgumentNullException_WhenConfigurationIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration? configuration = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddOpenApiDocumentation(configuration!));
    }

    [Fact]
    public void AddOpenApiDocumentation_ThrowsArgumentNullException_WhenConfigurationSectionIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfigurationSection? section = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddOpenApiDocumentation(section!));
    }

    [Fact]
    public void AddOpenApiDocumentation_ThrowsArgumentNullException_WhenActionIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<SwaggerOptions>? action = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddOpenApiDocumentation(action!));
    }

    [Fact]
    public void AddOpenApiDocumentation_WhenDisabled_DoesNotRegisterSwaggerServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(enabled: false);

        // Act
        services.AddOpenApiDocumentation(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetService<IOptions<SwaggerOptions>>();
        Assert.NotNull(options);
        Assert.False(options.Value.Enable);
    }

    [Fact]
    public void AddOpenApiDocumentation_WithValidConfiguration_BindsAllProperties()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateFullConfiguration();

        // Act
        services.AddOpenApiDocumentation(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SwaggerOptions>>().Value;

        // Assert
        Assert.True(options.Enable);
        Assert.Equal("Full Test API", options.Title);
        Assert.Equal("v1", options.Version);
        Assert.Equal("Full API Description", options.Description);
        Assert.Equal("John Doe", options.ContactName);
        Assert.Equal("john@example.com", options.ContactEmail);
        Assert.Equal("https://example.com/contact", options.ContactUrl);
        Assert.Equal("MIT", options.LicenseName);
        Assert.Equal("https://opensource.org/licenses/MIT", options.LicenseUrl);
        Assert.Equal("api-docs", options.RoutePrefix);
        Assert.True(options.EnableInProduction);
    }

    [Fact]
    public void AddOpenApiDocumentation_WithSecurityOptions_ConfiguresSecuritySchemes()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfigurationWithSecurity();

        // Act
        services.AddOpenApiDocumentation(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SwaggerOptions>>().Value;

        // Assert
        Assert.True(options.Security.EnableJwtBearer);
        Assert.True(options.Security.EnableApiKey);
        Assert.Equal("X-Custom-Key", options.Security.ApiKeyHeaderName);
    }

    [Fact]
    public void AddOpenApiDocumentation_WithServerUrls_ConfiguresServers()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfigurationWithServers();

        // Act
        services.AddOpenApiDocumentation(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SwaggerOptions>>().Value;

        // Assert
        Assert.Equal(2, options.Servers.Count);
        Assert.Equal("https://api.example.com", options.Servers[0].Url);
        Assert.Equal("Production", options.Servers[0].Description);
        Assert.Equal("https://api-staging.example.com", options.Servers[1].Url);
        Assert.Equal("Staging", options.Servers[1].Description);
    }

    [Fact]
    public void UseOpenApiDocumentation_ThrowsArgumentNullException_WhenAppIsNull()
    {
        // Arrange
        WebApplication? app = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            app!.UseOpenApiDocumentation());
    }

    [Fact]
    public void AddOpenApiDocumentation_WithUIOptions_ConfiguresUI()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfigurationWithUI();

        // Act
        services.AddOpenApiDocumentation(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SwaggerOptions>>().Value;

        // Assert
        Assert.Equal("Custom API Docs", options.UI.DocumentTitle);
        Assert.True(options.UI.DisplayOperationId);
        Assert.False(options.UI.EnableDeepLinking);
        Assert.Equal("full", options.UI.DocExpansion);
    }

    [Fact]
    public void AddOpenApiDocumentation_WithDocumentationOptions_ConfiguresDocumentation()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfigurationWithDocumentation();

        // Act
        services.AddOpenApiDocumentation(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SwaggerOptions>>().Value;

        // Assert
        Assert.False(options.Documentation.IgnoreObsoleteActions);
        Assert.True(options.Documentation.UseAllOfForRequired);
        Assert.Equal(2, options.Documentation.TagDescriptions.Count);
    }

    [Fact]
    public void AddOpenApiDocumentation_WithXmlComments_ConfiguresXmlPaths()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfigurationWithXml();

        // Act
        services.AddOpenApiDocumentation(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SwaggerOptions>>().Value;

        // Assert
        Assert.True(options.IncludeXmlComments);
        Assert.Equal(2, options.XmlDocumentationFiles.Count);
        Assert.Contains("Api.xml", options.XmlDocumentationFiles);
        Assert.Contains("Models.xml", options.XmlDocumentationFiles);
    }

    [Fact]
    public void AddOpenApiDocumentation_ValidatesOnStart()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateInvalidConfiguration();

        // Act
        services.AddOpenApiDocumentation(configuration);

        // Assert - Should throw on BuildServiceProvider because ValidateOnStart is enabled
        Assert.Throws<OptionsValidationException>(() =>
            services.BuildServiceProvider(validateScopes: true).GetRequiredService<IOptions<SwaggerOptions>>().Value);
    }

    [Fact]
    public void AddOpenApiDocumentation_SupportsMultipleSecuritySchemes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOpenApiDocumentation(options =>
        {
            options.Enable = true;
            options.Title = "Multi-Auth API";
            options.Version = "v1";
            options.Security.EnableJwtBearer = true;
            options.Security.EnableApiKey = true;
            options.Security.EnableOAuth2 = true;
            options.Security.OAuth2 = new OAuth2Options
            {
                AuthorizationUrl = "https://auth.example.com/authorize",
                TokenUrl = "https://auth.example.com/token",
                Scopes = new Dictionary<string, string>
                {
                    ["read"] = "Read access",
                    ["write"] = "Write access"
                }
            };
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var swaggerOptions = serviceProvider.GetRequiredService<IOptions<SwaggerOptions>>().Value;

        // Assert
        Assert.True(swaggerOptions.Security.EnableJwtBearer);
        Assert.True(swaggerOptions.Security.EnableApiKey);
        Assert.True(swaggerOptions.Security.EnableOAuth2);
        Assert.NotNull(swaggerOptions.Security.OAuth2);
        Assert.Equal(2, swaggerOptions.Security.OAuth2.Scopes.Count);
    }

    #region Helper Methods

    private static IConfiguration CreateConfiguration(bool enabled)
    {
        var configData = new Dictionary<string, string?>
        {
            [$"{SwaggerOptions.SectionName}:Enable"] = enabled.ToString(),
            [$"{SwaggerOptions.SectionName}:Title"] = "Test API",
            [$"{SwaggerOptions.SectionName}:Version"] = "v1",
            [$"{SwaggerOptions.SectionName}:Description"] = "Test Description"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    private static IConfiguration CreateFullConfiguration()
    {
        var configData = new Dictionary<string, string?>
        {
            [$"{SwaggerOptions.SectionName}:Enable"] = "true",
            [$"{SwaggerOptions.SectionName}:Title"] = "Full Test API",
            [$"{SwaggerOptions.SectionName}:Version"] = "v1",
            [$"{SwaggerOptions.SectionName}:Description"] = "Full API Description",
            [$"{SwaggerOptions.SectionName}:ContactName"] = "John Doe",
            [$"{SwaggerOptions.SectionName}:ContactEmail"] = "john@example.com",
            [$"{SwaggerOptions.SectionName}:ContactUrl"] = "https://example.com/contact",
            [$"{SwaggerOptions.SectionName}:LicenseName"] = "MIT",
            [$"{SwaggerOptions.SectionName}:LicenseUrl"] = "https://opensource.org/licenses/MIT",
            [$"{SwaggerOptions.SectionName}:RoutePrefix"] = "api-docs",
            [$"{SwaggerOptions.SectionName}:EnableInProduction"] = "true"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    private static IConfiguration CreateConfigurationWithSecurity()
    {
        var configData = new Dictionary<string, string?>
        {
            [$"{SwaggerOptions.SectionName}:Enable"] = "true",
            [$"{SwaggerOptions.SectionName}:Title"] = "Test API",
            [$"{SwaggerOptions.SectionName}:Version"] = "v1",
            [$"{SwaggerOptions.SectionName}:Security:EnableJwtBearer"] = "true",
            [$"{SwaggerOptions.SectionName}:Security:EnableApiKey"] = "true",
            [$"{SwaggerOptions.SectionName}:Security:ApiKeyHeaderName"] = "X-Custom-Key"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    private static IConfiguration CreateConfigurationWithServers()
    {
        var configData = new Dictionary<string, string?>
        {
            [$"{SwaggerOptions.SectionName}:Enable"] = "true",
            [$"{SwaggerOptions.SectionName}:Title"] = "Test API",
            [$"{SwaggerOptions.SectionName}:Version"] = "v1",
            [$"{SwaggerOptions.SectionName}:Servers:0:Url"] = "https://api.example.com",
            [$"{SwaggerOptions.SectionName}:Servers:0:Description"] = "Production",
            [$"{SwaggerOptions.SectionName}:Servers:1:Url"] = "https://api-staging.example.com",
            [$"{SwaggerOptions.SectionName}:Servers:1:Description"] = "Staging"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    private static IConfiguration CreateConfigurationWithUI()
    {
        var configData = new Dictionary<string, string?>
        {
            [$"{SwaggerOptions.SectionName}:Enable"] = "true",
            [$"{SwaggerOptions.SectionName}:Title"] = "Test API",
            [$"{SwaggerOptions.SectionName}:Version"] = "v1",
            [$"{SwaggerOptions.SectionName}:UI:DocumentTitle"] = "Custom API Docs",
            [$"{SwaggerOptions.SectionName}:UI:DisplayOperationId"] = "true",
            [$"{SwaggerOptions.SectionName}:UI:EnableDeepLinking"] = "false",
            [$"{SwaggerOptions.SectionName}:UI:DocExpansion"] = "full"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    private static IConfiguration CreateConfigurationWithDocumentation()
    {
        var configData = new Dictionary<string, string?>
        {
            [$"{SwaggerOptions.SectionName}:Enable"] = "true",
            [$"{SwaggerOptions.SectionName}:Title"] = "Test API",
            [$"{SwaggerOptions.SectionName}:Version"] = "v1",
            [$"{SwaggerOptions.SectionName}:Documentation:IgnoreObsoleteActions"] = "false",
            [$"{SwaggerOptions.SectionName}:Documentation:UseAllOfForRequired"] = "true",
            [$"{SwaggerOptions.SectionName}:Documentation:TagDescriptions:Users"] = "User endpoints",
            [$"{SwaggerOptions.SectionName}:Documentation:TagDescriptions:Products"] = "Product endpoints"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    private static IConfiguration CreateConfigurationWithXml()
    {
        var configData = new Dictionary<string, string?>
        {
            [$"{SwaggerOptions.SectionName}:Enable"] = "true",
            [$"{SwaggerOptions.SectionName}:Title"] = "Test API",
            [$"{SwaggerOptions.SectionName}:Version"] = "v1",
            [$"{SwaggerOptions.SectionName}:IncludeXmlComments"] = "true",
            [$"{SwaggerOptions.SectionName}:XmlDocumentationFiles:0"] = "Api.xml",
            [$"{SwaggerOptions.SectionName}:XmlDocumentationFiles:1"] = "Models.xml"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    private static IConfiguration CreateInvalidConfiguration()
    {
        var configData = new Dictionary<string, string?>
        {
            [$"{SwaggerOptions.SectionName}:Enable"] = "true",
            [$"{SwaggerOptions.SectionName}:Title"] = "", // Invalid - required when enabled
            [$"{SwaggerOptions.SectionName}:Version"] = "v1"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    #endregion
}
