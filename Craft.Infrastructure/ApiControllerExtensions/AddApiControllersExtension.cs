using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Infrastructure.ApiControllerExtensions;

/// <summary>
/// Extension methods for configuring API controllers with custom model validation behavior.
/// </summary>
public static class AddApiControllersExtension
{
    /// <summary>
    /// Adds API controllers with custom model validation response formatting.
    /// Uses RFC 7807 ProblemDetails format for validation errors.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Optional configuration to customize validation status code. If not provided, defaults to 422.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApiControllers(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var statusCode = configuration?
            .GetSection("RequestMiddlewareSettings:ModelValidationStatusCode")
            .Get<int?>() ?? 422;

        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                    var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext,
                        context.ModelState);

                    problemDetails.Detail = "One or more validation errors occurred. See the errors property for details.";
                    problemDetails.Instance = context.HttpContext.Request.Path;
                    problemDetails.Status = statusCode;
                    problemDetails.Title = statusCode == 422 
                        ? "One or more validation errors occurred" 
                        : "Bad Request";

                    ObjectResult result = statusCode == 422
                        ? new UnprocessableEntityObjectResult(problemDetails)
                        : new BadRequestObjectResult(problemDetails);

                    result.ContentTypes.Add("application/problem+json");

                    return result;
                };
            });
        
        return services;
    }
}
