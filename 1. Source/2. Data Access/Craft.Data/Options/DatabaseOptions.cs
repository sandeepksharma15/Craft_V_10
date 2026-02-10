using System.ComponentModel.DataAnnotations;

namespace Craft.Data;

public class DatabaseOptions : IValidatableObject
{
    public const string SectionName = "DatabaseOptions";

    public int CommandTimeout { get; set; } = 30;
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Optional read-only connection string for read replicas or read-only operations.
    /// If not specified, ConnectionString is used for both read and write operations.
    /// </summary>
    public string? ReadOnlyConnectionString { get; set; }

    public string DbProvider { get; set; } = string.Empty;
    public bool EnableDetailedErrors { get; set; } = false;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnablePerformanceLogging { get; set; } = false;
    public int MaxRetryCount { get; set; } = 3;
    public int MaxRetryDelay { get; set; } = 15;

    /// <summary>
    /// Assembly name containing EF Core migrations. If not specified, uses the entry assembly.
    /// </summary>
    public string? MigrationAssembly { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DbProvider.IsNullOrEmpty())
            yield return new ValidationResult(
                $"{nameof(DatabaseOptions)}.{nameof(DbProvider)} is not configured",
                [nameof(DbProvider)]);

        if (ConnectionString.IsNullOrEmpty())
            yield return new ValidationResult(
                $"{nameof(DatabaseOptions)}.{nameof(ConnectionString)} is not configured",
                [nameof(ConnectionString)]);
    }
}

