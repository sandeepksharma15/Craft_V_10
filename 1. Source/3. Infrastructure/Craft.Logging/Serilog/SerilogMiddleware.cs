using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Craft.Logging.Serilog;

public class SerilogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CraftSerilogOptions _options;

    public SerilogMiddleware(RequestDelegate next)
    {
        _next = next;
        _options = new CraftSerilogOptions(); // Optionally inject via DI if needed
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract values from headers, claims, or other sources
        var tenantId = context.Request.Headers[_options.EnricherNames.TenantId].ToString();
        var userId = context.Request.Headers[_options.EnricherNames.UserId].ToString();
        var clientId = context.Request.Headers[_options.EnricherNames.ClientId].ToString();

        using (LogContext.PushProperty(_options.EnricherNames.TenantId, tenantId ?? string.Empty))
        using (LogContext.PushProperty(_options.EnricherNames.UserId, userId ?? string.Empty))
        using (LogContext.PushProperty(_options.EnricherNames.ClientId, clientId ?? string.Empty))
        {
            await _next(context);
        }
    }
}
