using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Infrastructure.RequestMiddleware;

/// <summary>
/// Extension methods for configuring API controllers with custom model validation behavior.
/// Integrates with the Request Middleware system for consistent error handling.
/// </summary>
public static class ApiControllersExtension
{
    /// <summary>
    /// Adds API controllers with custom model validation response formatting.
    /// Uses RFC 7807 ProblemDetails format for validation errors.
    /// Integrates with CraftProblemDetailsFactory for automatic enrichment.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Optional configuration to customize validation status code. If not provided, defaults to 422.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This extension configures ASP.NET Core MVC controllers to use the CraftProblemDetailsFactory
    /// for all model validation errors, ensuring consistent error responses across the application.
    /// </para>
    /// <para>
    /// The ProblemDetails response is automatically enriched with:
    /// <list type="bullet">
    /// <item>correlationId - Request correlation identifier</item>
    /// <item>traceId - ASP.NET Core trace identifier</item>
    /// <item>timestamp - UTC timestamp when error occurred</item>
    /// <item>path - Request path</item>
    /// <item>method - HTTP method</item>
    /// <item>userId, userEmail, tenant - User context (if authenticated)</item>
    /// </list>
    /// </para>
    /// <para>
    /// The validation status code is determined by <c>RequestMiddlewareSettings:ModelValidationStatusCode</c>:
    /// <list type="bullet">
    /// <item>422 (Unprocessable Entity) - Semantic, RFC 4918 compliant (default)</item>
    /// <item>400 (Bad Request) - Traditional approach</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Usage:</strong>
    /// <code>
    /// builder.Services.AddApiControllers(builder.Configuration);
    /// </code>
    /// </para>
    /// </remarks>
    /// <example>
    /// Configuration in appsettings.json:
    /// <code>
    /// {
    ///   "RequestMiddlewareSettings": {
    ///     "ModelValidationStatusCode": 422
    ///   }
    /// }
    /// </code>
    /// 
    /// Sample validation error response:
    /// <code>
    /// {
    ///   "type": "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2",
    ///   "title": "One or more validation errors occurred",
    ///   "status": 422,
    ///   "detail": "One or more validation errors occurred. See the errors property for details.",
    ///   "instance": "/api/users",
    ///   "errors": {
    ///     "Email": ["Email is required", "Email format is invalid"],
    ///     "Age": ["Age must be between 18 and 100"]
    ///   },
    ///   "correlationId": "abc-123-xyz",
    ///   "traceId": "0HN1FKQNVQO5M:00000001",
    ///   "timestamp": "2024-01-15T10:30:00.000Z",
    ///   "path": "/api/users",
    ///   "method": "POST"
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddApiControllers(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    // Get the factory - it will handle status code configuration and enrichment
                    var problemDetailsFactory = context.HttpContext.RequestServices
                        .GetRequiredService<ProblemDetailsFactory>();

                    // Create ValidationProblemDetails
                    // The CraftProblemDetailsFactory automatically:
                    // - Sets status code from RequestMiddlewareSettings.ModelValidationStatusCode
                    // - Sets appropriate title based on status code
                    // - Sets RFC-compliant type URL
                    // - Enriches with diagnostic extensions (correlationId, timestamp, user context, etc.)
                    var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                        context.HttpContext,
                        context.ModelState);

                    // Set detail message if not already set by custom validation
                    if (string.IsNullOrEmpty(problemDetails.Detail))
                        problemDetails.Detail = "One or more validation errors occurred. See the errors property for details.";

                    // Return appropriate result based on status code
                    // The status was set by CraftProblemDetailsFactory based on configuration
                    ObjectResult result = problemDetails.Status == 422
                        ? new UnprocessableEntityObjectResult(problemDetails)
                        : new BadRequestObjectResult(problemDetails);

                    result.ContentTypes.Add("application/problem+json");

                    return result;
                };
            });
        
        return services;
    }
}
