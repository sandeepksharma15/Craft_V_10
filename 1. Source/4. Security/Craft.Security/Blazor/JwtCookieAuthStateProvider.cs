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
public class JwtCookieAuthStateProvider(IHttpContextAccessor httpContextAccessor) : AuthenticationStateProvider
{
    private static readonly AuthenticationState _anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var context = httpContextAccessor.HttpContext;

            if (context?.User?.Identity?.IsAuthenticated != true)
                return Task.FromResult(_anonymous);

            return Task.FromResult(new AuthenticationState(context.User));
        }
        catch
        {
            return Task.FromResult(_anonymous);
        }
    }

    public void NotifyAuthChanged(ClaimsPrincipal user)
    {
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(user)));
    }
}
