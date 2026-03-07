using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Craft.Security;

/// <summary>
/// Extension methods for registering cookie-based sign-in and sign-out endpoints.
/// </summary>
public static class CookieAuthEndpointExtensions
{
    private const string BearerTokenCookie = "BearerToken";

    /// <summary>
    /// Maps minimal API endpoints for cookie-based auth:
    /// <list type="bullet">
    /// <item><c>GET /auth/sign-in?token={jwt}&amp;returnUrl={url}</c> — writes the HTTP-only bearer cookie and redirects.</item>
    /// <item><c>GET /auth/sign-out?returnUrl={url}</c> — deletes the bearer cookie and redirects.</item>
    /// </list>
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapAuthCookieEndpoints(this WebApplication app)
    {
        app.MapGet("/auth/sign-in", (string token, string returnUrl, HttpResponse response) =>
        {
            response.Cookies.Append(BearerTokenCookie, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Results.Redirect(returnUrl);
        }).AllowAnonymous();

        app.MapGet("/auth/sign-out", (string returnUrl, HttpResponse response) =>
        {
            response.Cookies.Delete(BearerTokenCookie);

            return Results.Redirect(returnUrl);
        }).AllowAnonymous();

        return app;
    }
}
