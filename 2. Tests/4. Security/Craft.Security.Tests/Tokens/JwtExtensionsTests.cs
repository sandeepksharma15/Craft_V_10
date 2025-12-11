using Craft.Security.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Security.Tests.Tokens;

public class JwtExtensionsTests
{
    [Fact]
    public void ConfigureJwt_Should_AddJwtAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.ConfigureJwt(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtSettings = serviceProvider.GetService<IOptions<JwtSettings>>();
        Assert.NotNull(jwtSettings);
        Assert.NotNull(jwtSettings.Value);
        Assert.Equal("TestIssuer", jwtSettings.Value.ValidIssuer);
    }

    [Fact]
    public void ConfigureJwt_Should_ValidateJwtSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.ConfigureJwt(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtSettings = serviceProvider.GetService<IOptions<JwtSettings>>();
        Assert.NotNull(jwtSettings?.Value);
        
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(jwtSettings.Value);
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            jwtSettings.Value, validationContext, validationResults, true);
        
        Assert.True(isValid);
    }

    [Fact]
    public void ConfigureJwt_Should_ThrowException_WhenSettingsAreMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => services.ConfigureJwt(configuration));
    }

    [Fact]
    public void ConfigureJwt_Should_ConfigureAuthenticationScheme()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.ConfigureJwt(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var authSchemes = serviceProvider.GetServices<IConfigureOptions<JwtBearerOptions>>();
        Assert.NotEmpty(authSchemes);
    }

    [Fact]
    public void ConfigureJwt_Should_ConfigureTokenValidationParameters()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.ConfigureJwt(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
        Assert.Equal("TestIssuer", jwtSettings.ValidIssuer);
        Assert.Contains("TestAudience", jwtSettings.ValidAudiences!);
        Assert.True(jwtSettings.ValidateIssuer);
        Assert.True(jwtSettings.ValidateAudience);
        Assert.True(jwtSettings.ValidateIssuerSigningKey);
    }

    [Fact]
    public void AddTokenManagement_Should_AddTimeProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTokenManagement();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var timeProvider = serviceProvider.GetService<TimeProvider>();
        Assert.NotNull(timeProvider);
    }

    [Fact]
    public void AddTokenManagement_Should_AddTokenBlacklist()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTokenManagement();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var blacklist = serviceProvider.GetService<ITokenBlacklist>();
        Assert.NotNull(blacklist);
        Assert.IsType<InMemoryTokenBlacklist>(blacklist);
    }

    [Fact]
    public void AddTokenManagement_Should_AddCleanupService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTokenManagement();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var hostedServices = serviceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
        Assert.Contains(hostedServices, s => s is TokenBlacklistCleanupService);
    }

    [Fact]
    public void AddTokenManagement_Should_RegisterTimeProviderAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTokenManagement();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var timeProvider1 = serviceProvider.GetService<TimeProvider>();
        var timeProvider2 = serviceProvider.GetService<TimeProvider>();
        Assert.Same(timeProvider1, timeProvider2);
    }

    [Fact]
    public void AddTokenManagement_Should_RegisterBlacklistAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddTokenManagement();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var blacklist1 = serviceProvider.GetService<ITokenBlacklist>();
        var blacklist2 = serviceProvider.GetService<ITokenBlacklist>();
        Assert.Same(blacklist1, blacklist2);
    }

    [Fact]
    public void ConfigureJwt_Should_SetClockSkew()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.ConfigureJwt(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
        Assert.Equal(5, jwtSettings.ClockSkew);
    }

    [Fact]
    public void ConfigureJwt_Should_SetTokenExpirationInMinutes()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.ConfigureJwt(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
        Assert.Equal(60, jwtSettings.TokenExpirationInMinutes);
    }

    [Fact]
    public void ConfigureJwt_Should_SetRefreshTokenExpirationInDays()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.ConfigureJwt(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
        Assert.Equal(7, jwtSettings.RefreshTokenExpirationInDays);
    }

    [Fact]
    public void ConfigureJwt_Should_EnableSaveToken()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.ConfigureJwt(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
        Assert.True(jwtSettings.SaveToken);
    }

    private static IConfiguration CreateValidConfiguration()
    {
        var configValues = new Dictionary<string, string>
        {
            { "SecuritySettings:JwtSettings:IssuerSigningKey", "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456" },
            { "SecuritySettings:JwtSettings:ValidIssuer", "TestIssuer" },
            { "SecuritySettings:JwtSettings:ValidAudiences:0", "TestAudience" },
            { "SecuritySettings:JwtSettings:TokenExpirationInMinutes", "60" },
            { "SecuritySettings:JwtSettings:RefreshTokenExpirationInDays", "7" },
            { "SecuritySettings:JwtSettings:ValidateAudience", "true" },
            { "SecuritySettings:JwtSettings:ValidateIssuer", "true" },
            { "SecuritySettings:JwtSettings:ValidateIssuerSigningKey", "true" },
            { "SecuritySettings:JwtSettings:ValidateLifetime", "true" },
            { "SecuritySettings:JwtSettings:RequireExpirationTime", "true" },
            { "SecuritySettings:JwtSettings:RequireSignedTokens", "true" },
            { "SecuritySettings:JwtSettings:RequireHttpsMetaData", "true" },
            { "SecuritySettings:JwtSettings:SaveToken", "true" },
            { "SecuritySettings:JwtSettings:IncludeErrorDetails", "true" },
            { "SecuritySettings:JwtSettings:ClockSkew", "5" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();
    }
}
