using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Controllers;

/// <summary>
/// Extension methods for configuring rate limiting policies for API controllers.
/// </summary>
/// <remarks>
/// <para>
/// Rate limiting requires ASP.NET Core 7.0 or later and the Microsoft.AspNetCore.RateLimiting namespace.
/// This extension provides helper methods for configuring common rate limiting scenarios.
/// </para>
/// <para>
/// <strong>Important:</strong> These extensions use reflection to call AddRateLimiter from the hosting application
/// since it's not directly available in class library projects. Alternatively, you can add rate limiting
/// directly in your Program.cs:
/// </para>
/// <code>
/// builder.Services.AddRateLimiter(options =>
/// {
///     options.AddFixedWindowLimiter("read-policy", opt =>
///     {
///         opt.PermitLimit = 100;
///         opt.Window = TimeSpan.FromMinutes(1);
///     });
/// });
/// </code>
/// </remarks>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds pre-configured rate limiting policies for CRUD operations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to customize rate limiting options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Configures the following policies:
    /// <list type="bullet">
    /// <item><b>read-policy</b>: 100 requests per minute per IP</item>
    /// <item><b>write-policy</b>: 30 requests per minute per IP</item>
    /// <item><b>delete-policy</b>: 10 requests per minute per IP</item>
    /// <item><b>bulk-policy</b>: 5 requests per minute per IP</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Usage in Program.cs:</strong>
    /// <code>
    /// builder.Services.AddControllerRateLimiting();
    /// 
    /// var app = builder.Build();
    /// app.UseRateLimiter();
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Usage in Controllers:</strong>
    /// <code>
    /// [EnableRateLimiting("read-policy")]
    /// [HttpGet]
    /// public async Task&lt;IActionResult&gt; GetAll() { }
    /// 
    /// [EnableRateLimiting("write-policy")]
    /// [HttpPost]
    /// public async Task&lt;IActionResult&gt; Create() { }
    /// </code>
    /// </para>
    /// </remarks>
    /// <example>
    /// Custom configuration:
    /// <code>
    /// builder.Services.AddControllerRateLimiting(options =>
    /// {
    ///     options.GlobalLimiter = PartitionedRateLimiter.Create&lt;HttpContext, string&gt;(context =>
    ///     {
    ///         return RateLimitPartition.GetFixedWindowLimiter(
    ///             context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
    ///             _ => new FixedWindowRateLimiterOptions
    ///             {
    ///                 PermitLimit = 200,
    ///                 Window = TimeSpan.FromMinutes(1)
    ///             });
    ///     });
    /// });
    /// </code>
    /// </example>
    /// <summary>
    /// Adds pre-configured rate limiting policies for CRUD operations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to customize rate limiting options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> Due to class library limitations, this method provides documentation and examples.
    /// To use rate limiting, add the following code directly in your Program.cs:
    /// </para>
    /// <code>
    /// builder.Services.AddRateLimiter(options =>
    /// {
    ///     // Read operations policy (GET) - More permissive
    ///     options.AddFixedWindowLimiter("read-policy", opt =>
    ///     {
    ///         opt.PermitLimit = 100;
    ///         opt.Window = TimeSpan.FromMinutes(1);
    ///         opt.QueueLimit = 10;
    ///     });
    ///     
    ///     // Write operations policy (POST, PUT) - Moderate
    ///     options.AddFixedWindowLimiter("write-policy", opt =>
    ///     {
    ///         opt.PermitLimit = 30;
    ///         opt.Window = TimeSpan.FromMinutes(1);
    ///         opt.QueueLimit = 5;
    ///     });
    ///     
    ///     // Delete operations policy - Restrictive
    ///     options.AddFixedWindowLimiter("delete-policy", opt =>
    ///     {
    ///         opt.PermitLimit = 10;
    ///         opt.Window = TimeSpan.FromMinutes(1);
    ///         opt.QueueLimit = 2;
    ///     });
    ///     
    ///     // Configure rejection behavior
    ///     options.RejectionStatusCode = 429;
    ///     options.OnRejected = async (context, cancellationToken) =>
    ///     {
    ///         context.HttpContext.Response.StatusCode = 429;
    ///         await context.HttpContext.Response.WriteAsJsonAsync(new
    ///         {
    ///             error = "Too many requests",
    ///             message = "Rate limit exceeded. Please try again later."
    ///         }, cancellationToken);
    ///     };
    /// });
    /// 
    /// // In your middleware pipeline:
    /// app.UseRateLimiter();
    /// 
    /// // In your controllers:
    /// [EnableRateLimiting("read-policy")]
    /// [HttpGet]
    /// public async Task&lt;IActionResult&gt; GetAll() { }
    /// </code>
    /// </remarks>
    [Obsolete("This method provides documentation only. Implement rate limiting directly in Program.cs as shown in the remarks.")]
    public static IServiceCollection AddControllerRateLimiting(
        this IServiceCollection services,
        Action<object>? configureOptions = null)
    {
        throw new NotImplementedException(
            "Rate limiting must be configured directly in your Program.cs using services.AddRateLimiter(). " +
            "See FEATURES.md for complete implementation examples.");
    }

    /// <summary>
    /// Adds a custom rate limiting policy.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="policyName">The name of the policy.</param>
    /// <param name="partitionedLimiter">The rate limiter configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> Due to class library limitations, this method provides documentation only.
    /// Implement rate limiting directly in your Program.cs:
    /// </para>
    /// <code>
    /// builder.Services.AddRateLimiter(options =>
    /// {
    ///     options.AddPolicy("custom-policy", context =>
    ///         RateLimitPartition.GetFixedWindowLimiter(
    ///             context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
    ///             _ => new FixedWindowRateLimiterOptions
    ///             {
    ///                 PermitLimit = 50,
    ///                 Window = TimeSpan.FromSeconds(30)
    ///             }));
    /// });
    /// </code>
    /// </remarks>
    [Obsolete("This method provides documentation only. Implement rate limiting directly in Program.cs as shown in the remarks.")]
    public static IServiceCollection AddCustomRateLimitPolicy(
        this IServiceCollection services,
        string policyName,
        Func<HttpContext, object> partitionedLimiter)
    {
        throw new NotImplementedException(
            "Rate limiting must be configured directly in your Program.cs using services.AddRateLimiter(). " +
            "See FEATURES.md for complete implementation examples.");
    }
}
