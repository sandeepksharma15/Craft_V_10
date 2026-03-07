using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Security;

/// <summary>
/// ASP.NET Core authentication handler that reads a JWT from the <c>BearerToken</c> HTTP-only
/// cookie and produces a <see cref="ClaimsPrincipal"/>. This runs at the middleware level so
/// that <c>[Authorize]</c> and <c>FallbackPolicy</c> work before Blazor components load.
/// Pairs with <see cref="JwtCookieAuthStateProvider"/> which provides the same identity to the
/// Blazor <see cref="Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider"/>.
/// </summary>
public class JwtCookieAuthenticationHandler(
    IOptionsMonitor<JwtCookieAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<JwtCookieAuthenticationOptions>(options, logger, encoder)
{
    /// <summary>
    /// The default authentication scheme name used by this handler.
    /// </summary>
    public const string SchemeName = "JwtCookie";

    private const string BearerTokenCookie = "BearerToken";

    private static readonly JwtSecurityTokenHandler _tokenHandler = new();

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Request.Cookies[BearerTokenCookie];

        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(AuthenticateResult.NoResult());

        try
        {
            var jwt = _tokenHandler.ReadJwtToken(token);

            // Treat expired tokens as unauthenticated
            if (jwt.ValidTo != DateTime.MinValue && jwt.ValidTo < DateTime.UtcNow)
                return Task.FromResult(AuthenticateResult.NoResult());

            var identity = new ClaimsIdentity(jwt.Claims, JwtBearerDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            return Task.FromResult(AuthenticateResult.Fail(ex));
        }
    }

    /// <inheritdoc />
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var loginPath = Options.LoginPath;
        var returnUrl = Uri.EscapeDataString(Request.Path + Request.QueryString);

        Response.Redirect($"{loginPath}?returnUrl={returnUrl}");

        return Task.CompletedTask;
    }
}
