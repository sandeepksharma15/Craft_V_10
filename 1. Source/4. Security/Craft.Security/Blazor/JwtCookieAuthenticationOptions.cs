using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Craft.Security;

/// <summary>
/// Options for the <see cref="JwtCookieAuthenticationHandler"/>.
/// </summary>
public class JwtCookieAuthenticationOptions : AuthenticationSchemeOptions, IValidatableObject
{
    public const string SchemeName = "JwtCookie";

    public bool HttpOnly { get; set; } = true;
    public string CookieName { get; set; } = "BearerToken";

    public PathString LoginPath { get; set; } = "/login";
    public PathString AccessDeniedPath { get; set; } = "/access-denied";
    public PathString LogoutPath { get; set; } = "/logout";

    public int ExpireTimeSpan { get; set; } = 60;
    public bool SlidingExpiration { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(CookieName))
            yield return new ValidationResult($"{nameof(JwtCookieAuthenticationOptions)}.{nameof(CookieName)} is not configured", [nameof(CookieName)]);

        if (!LoginPath.HasValue)
            yield return new ValidationResult($"{nameof(JwtCookieAuthenticationOptions)}.{nameof(LoginPath)} is not configured", [nameof(LoginPath)]);

        if (!AccessDeniedPath.HasValue)
            yield return new ValidationResult($"{nameof(JwtCookieAuthenticationOptions)}.{nameof(AccessDeniedPath)} is not configured", [nameof(AccessDeniedPath)]);

        if (!LogoutPath.HasValue)
            yield return new ValidationResult($"{nameof(JwtCookieAuthenticationOptions)}.{nameof(LogoutPath)} is not configured", [nameof(LogoutPath)]);

        if (ExpireTimeSpan <= 0)
            yield return new ValidationResult($"{nameof(JwtCookieAuthenticationOptions)}.{nameof(ExpireTimeSpan)} must be greater than 0", [nameof(ExpireTimeSpan)]);
    }
}
