using System.ComponentModel.DataAnnotations;

namespace Craft.Security.Tests.Options;

public class GoogleAuthOptionsTest
{
    [Fact]
    public void Validate_AllPropertiesValid_NoValidationErrors()
    {
        var options = new GoogleAuthOptions
        {
            ClientId = "client-id",
            ProjectId = "project-id",
            AuthUri = "https://auth.uri",
            TokenUri = "https://token.uri",
            AuthProviderX509CertUrl = "https://cert.url",
            ClientSecret = "secret",
            RedirectUris = ["https://redirect1", "https://redirect2"],
            JavascriptOrigins = ["https://origin1", "https://origin2"]
        };
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ClientId_Invalid(string? value)
    {
        var options = ValidOptions();
        options.ClientId = value;
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.ClientId)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ProjectId_Invalid(string? value)
    {
        var options = ValidOptions();
        options.ProjectId = value;
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.ProjectId)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_AuthUri_Invalid(string? value)
    {
        var options = ValidOptions();
        options.AuthUri = value;
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.AuthUri)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_TokenUri_Invalid(string? value)
    {
        var options = ValidOptions();
        options.TokenUri = value;
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.TokenUri)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_AuthProviderX509CertUrl_Invalid(string? value)
    {
        var options = ValidOptions();
        options.AuthProviderX509CertUrl = value ?? string.Empty;
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.AuthProviderX509CertUrl)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ClientSecret_Invalid(string? value)
    {
        var options = ValidOptions();
        options.ClientSecret = value;
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.ClientSecret)));
    }

    [Fact]
    public void Validate_RedirectUris_NullOrEmpty()
    {
        var options = ValidOptions();
        options.RedirectUris = null!;
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.RedirectUris)));

        options.RedirectUris = [];
        results = [.. options.Validate(new ValidationContext(options))];
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.RedirectUris)));
    }

    [Fact]
    public void Validate_RedirectUris_ContainsEmpty()
    {
        var options = ValidOptions();
        options.RedirectUris = ["valid", ""];
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.RedirectUris)));
    }

    [Fact]
    public void Validate_JavascriptOrigins_NullOrEmpty()
    {
        var options = ValidOptions();
        options.JavascriptOrigins = null!;
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.JavascriptOrigins)));

        options.JavascriptOrigins = [];
        results = [.. options.Validate(new ValidationContext(options))];
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.JavascriptOrigins)));
    }

    [Fact]
    public void Validate_JavascriptOrigins_ContainsEmpty()
    {
        var options = ValidOptions();
        options.JavascriptOrigins = ["valid", ""];
        var results = new List<ValidationResult>(options.Validate(new ValidationContext(options)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(GoogleAuthOptions.JavascriptOrigins)));
    }

    private static GoogleAuthOptions ValidOptions() => new()
    {
        ClientId = "client-id",
        ProjectId = "project-id",
        AuthUri = "https://auth.uri",
        TokenUri = "https://token.uri",
        AuthProviderX509CertUrl = "https://cert.url",
        ClientSecret = "secret",
        RedirectUris = ["https://redirect1"],
        JavascriptOrigins = ["https://origin1"]
    };
}

