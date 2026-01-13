using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Craft.Security;

/// <summary>
/// HTTP message handler that attaches bearer tokens to outgoing requests.
/// Handles HttpContext access safely in Blazor Server scenarios.
/// </summary>
public class BearerTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Capture token synchronously to avoid HttpContext disposal issues in async operations
        string? token = null;
        
        try
        {
            // Access HttpContext synchronously before any async operations
            token = httpContextAccessor?.HttpContext?.Request.Cookies["BearerToken"];
        }
        catch (ObjectDisposedException)
        {
            // HttpContext disposed - common in Blazor Server during navigation
            // Continue without token rather than failing the request
        }

        // Attach token if available
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Continue with the request
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
