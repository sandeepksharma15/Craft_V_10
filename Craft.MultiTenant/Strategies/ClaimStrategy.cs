using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.MultiTenant;

public class ClaimStrategy(string tenantKey = TenantConstants.TenantToken,
                     string authenticationScheme = "Bearer") : ITenantStrategy
{
    private readonly string _authenticationScheme = authenticationScheme;
    private readonly string _tenantKey = tenantKey;

    public async Task<string?> GetIdentifierAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        try
        {
            if (context.User.Identity is { IsAuthenticated: true })
                return context.User.FindFirst(_tenantKey)?.Value;

            AuthenticationScheme? authScheme;

            var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            if (_authenticationScheme is null)
                authScheme = await schemeProvider?.GetDefaultAuthenticateSchemeAsync()!;
            else
                authScheme = (await schemeProvider?.GetAllSchemesAsync()!)?.FirstOrDefault(x => x.Name == _authenticationScheme)!;

            if (authScheme is null)
                return null;

            var handler = (IAuthenticationHandler)ActivatorUtilities
                .CreateInstance(context.RequestServices, authScheme.HandlerType);

            await handler.InitializeAsync(authScheme, context);

            context.Items[$"{TenantConstants.TenantToken}__bypass_validate_principal__"] = "true"; // Value doesn't matter.
            var handlerResult = await handler.AuthenticateAsync();
            context.Items.Remove($"{TenantConstants.TenantToken}__bypass_validate_principal__");

            return handlerResult.Principal?.FindFirst(_tenantKey)?.Value;
        }
        catch
        {
            return null;
        }
    }
}
