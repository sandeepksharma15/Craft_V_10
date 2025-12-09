using System.ComponentModel.DataAnnotations;

namespace Craft.Security.Tokens;

public class JwtSettings : IValidatableObject
{
    public int ClockSkew { get; set; } = 1;
    public bool IncludeErrorDetails { get; set; } = true;
    public string IssuerSigningKey { get; set; } = string.Empty;
    public int RefreshTokenExpirationInDays { get; set; }
    public bool RequireExpirationTime { get; set; } = true;
    public bool RequireHttpsMetaData { get; set; } = true;
    public bool RequireSignedTokens { get; set; } = true;
    public bool SaveToken { get; set; } = true;
    public int TokenExpirationInMinutes { get; set; }
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public string[]? ValidAudiences { get; set; }

    public string? ValidIssuer { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IssuerSigningKey.IsNullOrEmpty())
            yield return new ValidationResult("No Key defined in JwtSettings config", [nameof(IssuerSigningKey)]);
        if (ValidIssuer.IsNullOrEmpty())
            yield return new ValidationResult("No ValidIssuer defined in JwtSettings config", [nameof(ValidIssuer)]);
        if (ValidAudiences!.IsNullOrEmpty())
            yield return new ValidationResult("No ValidAudiences defined in JwtSettings config", [nameof(ValidAudiences)]);
        if (TokenExpirationInMinutes <= 0)
            yield return new ValidationResult("TokenExpirationInMinutes must be greater than 0", [nameof(TokenExpirationInMinutes)]);
        if (RefreshTokenExpirationInDays <= 0)
            yield return new ValidationResult("RefreshTokenExpirationInDays must be greater than 0", [nameof(RefreshTokenExpirationInDays)]);
    }
}
