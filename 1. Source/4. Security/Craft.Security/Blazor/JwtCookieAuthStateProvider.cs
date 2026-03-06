using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace Craft.Security;

/// <summary>
/// Blazor Server <see cref="AuthenticationStateProvider"/> that reads auth state from the
/// <c>BearerToken</c> HTTP-only cookie written by <see cref="CookieAuthEndpointExtensions"/>.
/// The JWT is read but not re-validated; cryptographic validation is the responsibility of
/// the API that issued the token.
/// </summary>
public class JwtCookieAuthStateProvider(IHttpContextAccessor httpContextAccessor)
    : AuthenticationStateProvider
{
    private static readonly AuthenticationState _anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private static readonly JwtSecurityTokenHandler _tokenHandler = new();

    /// <inheritdoc />
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = httpContextAccessor.HttpContext?.Request.Cookies["BearerToken"];

            if (string.IsNullOrWhiteSpace(token))
                return Task.FromResult(_anonymous);

            var jwt = _tokenHandler.ReadJwtToken(token);

            // Treat expired tokens as anonymous — no need to surface a stale identity
            if (jwt.ValidTo != DateTime.MinValue && jwt.ValidTo < DateTime.UtcNow)
                return Task.FromResult(_anonymous);

            var identity = new ClaimsIdentity(jwt.Claims, JwtBearerDefaults.AuthenticationScheme);
            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(user));
        }
        catch (Exception)
        {
            // Covers malformed tokens and ObjectDisposedException when HttpContext
            // is disposed during Blazor Server reconnection — treat both as anonymous.
            return Task.FromResult(_anonymous);
        }
    }

    /// <summary>
    /// Notifies Blazor that the authentication state has changed.
    /// Call this after a programmatic sign-in or sign-out to immediately refresh
    /// all cascading <see cref="AuthenticationState"/> consumers.
    /// </summary>
    /// <param name="user">The updated <see cref="ClaimsPrincipal"/>.</param>
    public void NotifyAuthChanged(ClaimsPrincipal user)
    {
        var authState = Task.FromResult(new AuthenticationState(user));

        NotifyAuthenticationStateChanged(authState);
    }
}
