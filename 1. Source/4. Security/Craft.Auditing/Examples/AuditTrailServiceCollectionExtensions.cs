using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Auditing.Examples;

/// <summary>
/// Example showing how to configure AuditTrail JSON serialization in a dependency injection context.
/// </summary>
public static class AuditTrailServiceCollectionExtensions
{
    /// <summary>
    /// Example extension method to configure audit trail serialization in Program.cs or Startup.cs.
    /// </summary>
    public static IServiceCollection ConfigureAuditTrailSerialization(
        this IServiceCollection services,
        Action<JsonSerializerOptions>? configure = null)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 32
        };

        configure?.Invoke(options);

        AuditTrail.ConfigureSerializerOptions(options);

        return services;
    }
}

/// <summary>
/// Example usage in Program.cs or Startup.cs
/// </summary>
public class ProgramExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Example 1: Use default audit trail serialization
        services.ConfigureAuditTrailSerialization();

        // Example 2: Customize audit trail serialization
        services.ConfigureAuditTrailSerialization(options =>
        {
            options.WriteIndented = true;  // For development
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.Converters.Add(new JsonStringEnumConverter());
        });

        // Example 3: Environment-specific configuration
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        services.ConfigureAuditTrailSerialization(options =>
        {
            options.WriteIndented = environment == "Development";
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    }
}

/// <summary>
/// Alternative: Configure using options pattern
/// </summary>
public class OptionsBasedExample
{
    public class AuditTrailOptions
    {
        public bool WriteIndented { get; set; }
        public bool UseCamelCase { get; set; }
        public bool UseEnumStrings { get; set; }
        public int MaxDepth { get; set; } = 32;
    }

    public static void ConfigureFromOptions(IServiceCollection services, AuditTrailOptions auditOptions)
    {
        services.ConfigureAuditTrailSerialization(options =>
        {
            options.WriteIndented = auditOptions.WriteIndented;
            options.MaxDepth = auditOptions.MaxDepth;

            if (auditOptions.UseCamelCase)
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            if (auditOptions.UseEnumStrings)
                options.Converters.Add(new JsonStringEnumConverter());
        });
    }
}
