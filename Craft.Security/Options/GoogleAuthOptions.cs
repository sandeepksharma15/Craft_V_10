using System.ComponentModel.DataAnnotations;

namespace Craft.Security.Options;

public class GoogleAuthOptions : IValidatableObject
{
    public const string SectionName = "Authentication:Google";

    public string? ClientId { get; set; }
    public string? ProjectId { get; set; }
    public string? AuthUri { get; set; }
    public string? TokenUri { get; set; }
    public string AuthProviderX509CertUrl { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public string[] RedirectUris { get; set; } = [];
    public string[] JavascriptOrigins { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in GoogleAuthOptions.ValidateString(ClientId, nameof(ClientId))) yield return result;
        foreach (var result in GoogleAuthOptions.ValidateString(ProjectId, nameof(ProjectId))) yield return result;
        foreach (var result in GoogleAuthOptions.ValidateString(AuthUri, nameof(AuthUri))) yield return result;
        foreach (var result in GoogleAuthOptions.ValidateString(TokenUri, nameof(TokenUri))) yield return result;
        foreach (var result in GoogleAuthOptions.ValidateString(AuthProviderX509CertUrl, nameof(AuthProviderX509CertUrl))) yield return result;
        foreach (var result in GoogleAuthOptions.ValidateString(ClientSecret, nameof(ClientSecret))) yield return result;
        foreach (var result in GoogleAuthOptions.ValidateArray(RedirectUris, nameof(RedirectUris))) yield return result;
        foreach (var result in GoogleAuthOptions.ValidateArray(JavascriptOrigins, nameof(JavascriptOrigins))) yield return result;
    }

    private static IEnumerable<ValidationResult> ValidateString(string? value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
            yield return new ValidationResult($"{nameof(GoogleAuthOptions)}.{propertyName} is not configured", [propertyName]);
    }

    private static IEnumerable<ValidationResult> ValidateArray(string[]? array, string propertyName)
    {
        if (array == null || array.Length == 0 || array.Any(string.IsNullOrEmpty))
            yield return new ValidationResult($"{nameof(GoogleAuthOptions)}.{propertyName} is not configured or contains empty values", [propertyName]);
    }
}
