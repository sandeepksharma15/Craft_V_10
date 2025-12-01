using System.ComponentModel.DataAnnotations;
using Craft.Security.Tokens;

namespace Craft.Security.Tests.Tokens;

public class JwtSettingsTests
{
    [Fact]
    public void Validate_Should_Pass_WithValidSettings()
    {
        // Arrange
        var settings = new JwtSettings
        {
            IssuerSigningKey = "ValidKey123",
            ValidIssuer = "TestIssuer",
            ValidAudiences = ["Audience1"],
            TokenExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };

        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_Should_Fail_WhenIssuerSigningKeyIsNull()
    {
        // Arrange
        var settings = new JwtSettings
        {
            IssuerSigningKey = null!,
            ValidIssuer = "TestIssuer",
            ValidAudiences = ["Audience1"],
            TokenExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };

        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("No Key defined in JwtSettings config", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_Should_Fail_WhenIssuerSigningKeyIsEmpty()
    {
        // Arrange
        var settings = new JwtSettings
        {
            IssuerSigningKey = string.Empty,
            ValidIssuer = "TestIssuer",
            ValidAudiences = ["Audience1"],
            TokenExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };

        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("No Key defined in JwtSettings config", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_Should_Fail_WhenValidIssuerIsNull()
    {
        // Arrange
        var settings = new JwtSettings
        {
            IssuerSigningKey = "ValidKey123",
            ValidIssuer = null!,
            ValidAudiences = ["Audience1"],
            TokenExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };

        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("No ValidIssuer defined in JwtSettings config", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_Should_Fail_WhenValidAudiencesIsNull()
    {
        // Arrange
        var settings = new JwtSettings
        {
            IssuerSigningKey = "ValidKey123",
            ValidIssuer = "TestIssuer",
            ValidAudiences = null!,
            TokenExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };

        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("No ValidAudiences defined in JwtSettings config", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_Should_Fail_WhenValidAudiencesIsEmpty()
    {
        // Arrange
        var settings = new JwtSettings
        {
            IssuerSigningKey = "ValidKey123",
            ValidIssuer = "TestIssuer",
            ValidAudiences = [],
            TokenExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };

        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("No ValidAudiences defined in JwtSettings config", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_Should_Fail_WhenTokenExpirationIsZero()
    {
        // Arrange
        var settings = new JwtSettings
        {
            IssuerSigningKey = "ValidKey123",
            ValidIssuer = "TestIssuer",
            ValidAudiences = ["Audience1"],
            TokenExpirationInMinutes = 0,
            RefreshTokenExpirationInDays = 7
        };

        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("TokenExpirationInMinutes must be greater than 0", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_Should_Fail_WhenTokenExpirationIsNegative()
    {
        // Arrange
        var settings = new JwtSettings
        {
            IssuerSigningKey = "ValidKey123",
            ValidIssuer = "TestIssuer",
            ValidAudiences = ["Audience1"],
            TokenExpirationInMinutes = -10,
            RefreshTokenExpirationInDays = 7
        };

        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("TokenExpirationInMinutes must be greater than 0", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_Should_Fail_WhenRefreshTokenExpirationIsZero()
    {
        // Arrange
        var settings = new JwtSettings
        {
            IssuerSigningKey = "ValidKey123",
            ValidIssuer = "TestIssuer",
            ValidAudiences = ["Audience1"],
            TokenExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 0
        };

        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("RefreshTokenExpirationInDays must be greater than 0", results[0].ErrorMessage);
    }

    [Fact]
    public void Validate_Should_ReturnMultipleErrors_WhenMultipleFieldsInvalid()
    {
        // Arrange
        var settings = new JwtSettings
        {
            IssuerSigningKey = string.Empty,
            ValidIssuer = null!,
            ValidAudiences = [],
            TokenExpirationInMinutes = 0,
            RefreshTokenExpirationInDays = -1
        };

        var context = new ValidationContext(settings);

        // Act
        var results = settings.Validate(context).ToList();

        // Assert
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void DefaultValues_Should_BeSetCorrectly()
    {
        // Arrange & Act
        var settings = new JwtSettings();

        // Assert
        Assert.Equal(1, settings.ClockSkew);
        Assert.True(settings.IncludeErrorDetails);
        Assert.True(settings.RequireExpirationTime);
        Assert.True(settings.RequireHttpsMetaData);
        Assert.True(settings.RequireSignedTokens);
        Assert.True(settings.SaveToken);
        Assert.True(settings.ValidateAudience);
        Assert.True(settings.ValidateIssuer);
        Assert.True(settings.ValidateIssuerSigningKey);
        Assert.True(settings.ValidateLifetime);
    }
}
