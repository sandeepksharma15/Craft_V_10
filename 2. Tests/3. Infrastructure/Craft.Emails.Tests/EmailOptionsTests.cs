using System.ComponentModel.DataAnnotations;
using Craft.Emails;

namespace Craft.Tests.Emails;

public class EmailOptionsTests
{
    [Fact]
    public void Validate_WithValidSmtpConfiguration_ReturnsNoErrors()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "smtp",
            From = "test@example.com",
            Smtp = new SmtpSettings
            {
                Host = "smtp.example.com",
                Port = 587
            }
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_WithMissingFrom_ReturnsError()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "smtp",
            From = null
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EmailOptions.From)));
    }

    [Fact]
    public void Validate_WithInvalidEmailAddress_ReturnsError()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "smtp",
            From = "invalid-email"
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EmailOptions.From)));
    }

    [Fact]
    public void Validate_WithSmtpProviderAndNoSmtpSettings_ReturnsError()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "smtp",
            From = "test@example.com",
            Smtp = null
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Validate_WithInvalidRetryAttempts_ReturnsError()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "mock",
            From = "test@example.com",
            MaxRetryAttempts = 11 // > 10
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EmailOptions.MaxRetryAttempts)));
    }

    [Fact]
    public void Validate_WithMockProvider_SucceedsWithoutSmtpSettings()
    {
        // Arrange
        var options = new EmailOptions
        {
            Provider = "mock",
            From = "test@example.com"
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, true);

        // Assert
        Assert.True(isValid);
    }
}

public class SmtpSettingsTests
{
    [Fact]
    public void Validate_WithValidConfiguration_ReturnsNoErrors()
    {
        // Arrange
        var settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587
        };

        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_WithMissingHost_ReturnsError()
    {
        // Arrange
        var settings = new SmtpSettings
        {
            Host = null,
            Port = 587
        };

        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Validate_WithInvalidPort_ReturnsError()
    {
        // Arrange
        var settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 0
        };

        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(SmtpSettings.Port)));
    }

    [Fact]
    public void Validate_WithPortTooHigh_ReturnsError()
    {
        // Arrange
        var settings = new SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 70000
        };

        var context = new ValidationContext(settings);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(settings, context, results, true);

        // Assert
        Assert.False(isValid);
    }
}
