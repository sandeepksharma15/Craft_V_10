using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Security;

/// <summary>
/// ASP.NET Core authentication handler that reads a JWT from the cookie configured via
/// <see cref="JwtCookieAuthenticationOptions"/> and produces a <see cref="ClaimsPrincipal"/>.
/// This runs at the middleware level so that <c>[Authorize]</c> and fallback policies work
/// before Blazor components render. Pairs with <see cref="JwtCookieAuthStateProvider"/>.
/// </summary>
public class JwtCookieAuthenticationHandler(IOptionsMonitor<JwtCookieAuthenticationOptions> options,
    ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<JwtCookieAuthenticationOptions>(options, logger, encoder)
{
    private static readonly JwtSecurityTokenHandler _tokenHandler = new();

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue(Options.CookieName, out var token) || string.IsNullOrWhiteSpace(token))
            return Task.FromResult(AuthenticateResult.NoResult());

        try
        {
            var jwt = _tokenHandler.ReadJwtToken(token);

            // Reject tokens that carry an expiry and have already passed it
            if (jwt.ValidTo != DateTime.MinValue && jwt.ValidTo < DateTime.UtcNow)
                return Task.FromResult(AuthenticateResult.Fail("Token expired"));

            var identity = new ClaimsIdentity(jwt.Claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to parse JWT from cookie '{CookieName}'", Options.CookieName);

            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }
    }

    /// <inheritdoc />
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var returnUrl = Uri.EscapeDataString(Request.Path + Request.QueryString);

        Response.Redirect($"{Options.LoginPath}?returnUrl={returnUrl}");

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.Redirect(Options.AccessDeniedPath);

        return Task.CompletedTask;
    }
}
