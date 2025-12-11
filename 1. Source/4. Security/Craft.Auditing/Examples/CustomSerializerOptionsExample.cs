using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.Auditing.Examples;

/// <summary>
/// Examples demonstrating how to configure custom JsonSerializerOptions for audit trail serialization.
/// </summary>
public static class CustomSerializerOptionsExample
{
    /// <summary>
    /// Example 1: Configure audit trail to use indented JSON for better readability in development.
    /// </summary>
    public static void ConfigureIndentedJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,  // Make JSON readable for debugging
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 32
        };

        AuditTrail.ConfigureSerializerOptions(options);
    }

    /// <summary>
    /// Example 2: Configure audit trail with custom converters for specific types.
    /// </summary>
    public static void ConfigureWithCustomConverters()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 32,
            Converters =
            {
                new JsonStringEnumConverter(),  // Serialize enums as strings instead of numbers
            }
        };

        AuditTrail.ConfigureSerializerOptions(options);
    }

    /// <summary>
    /// Example 3: Configure audit trail with camelCase property names.
    /// </summary>
    public static void ConfigureCamelCaseNaming()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 32,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        AuditTrail.ConfigureSerializerOptions(options);
    }

    /// <summary>
    /// Example 4: Configure audit trail with snake_case property names.
    /// </summary>
    public static void ConfigureSnakeCaseNaming()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 32,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        AuditTrail.ConfigureSerializerOptions(options);
    }

    /// <summary>
    /// Example 5: Configure audit trail to include all properties (even nulls).
    /// </summary>
    public static void ConfigureIncludeNulls()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,  // Include null values
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 32
        };

        AuditTrail.ConfigureSerializerOptions(options);
    }

    /// <summary>
    /// Example 6: Reset to default options.
    /// </summary>
    public static void ResetToDefaultOptions()
    {
        AuditTrail.ConfigureSerializerOptions(null);
    }

    /// <summary>
    /// Example 7: Configure in application startup (ASP.NET Core).
    /// </summary>
    public class StartupConfiguration
    {
        public void ConfigureAuditTrailSerialization()
        {
            // Configure once at application startup
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                MaxDepth = 32,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            AuditTrail.ConfigureSerializerOptions(options);
        }
    }

    /// <summary>
    /// Example 8: Environment-specific configuration.
    /// </summary>
    public static void ConfigureByEnvironment(string environment)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = environment == "Development",  // Only indent in dev
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 32
        };

        AuditTrail.ConfigureSerializerOptions(options);
    }

    /// <summary>
    /// Example 9: Configure with increased max depth for complex object graphs.
    /// </summary>
    public static void ConfigureDeepObjectGraphs()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 64  // Double the default for very deep structures
        };

        AuditTrail.ConfigureSerializerOptions(options);
    }

    /// <summary>
    /// Example 10: Get current serializer options to inspect configuration.
    /// </summary>
    public static void InspectCurrentOptions()
    {
        var currentOptions = AuditTrail.SerializerOptions;
        
        Console.WriteLine($"WriteIndented: {currentOptions.WriteIndented}");
        Console.WriteLine($"MaxDepth: {currentOptions.MaxDepth}");
        Console.WriteLine($"PropertyNamingPolicy: {currentOptions.PropertyNamingPolicy?.GetType().Name ?? "None"}");
    }
}
