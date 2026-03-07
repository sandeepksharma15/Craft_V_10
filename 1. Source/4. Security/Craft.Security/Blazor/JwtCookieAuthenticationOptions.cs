using Microsoft.AspNetCore.Authentication;

namespace Craft.Security;

/// <summary>
/// Options for the <see cref="JwtCookieAuthenticationHandler"/>.
/// </summary>
public class JwtCookieAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// The path to redirect unauthenticated users to. Defaults to <c>/login</c>.
    /// </summary>
    public string LoginPath { get; set; } = "/login";
}
