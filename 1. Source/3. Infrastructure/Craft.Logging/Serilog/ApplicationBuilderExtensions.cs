using Microsoft.AspNetCore.Builder;

namespace Craft.Logging.Serilog;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSerilogEnrichers(this IApplicationBuilder app)
        => app.UseMiddleware<SerilogMiddleware>();
}
