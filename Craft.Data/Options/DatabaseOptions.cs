using System.ComponentModel.DataAnnotations;

namespace Craft.Data;

public class DatabaseOptions : IValidatableObject
{
    public const string SectionName = "DatabaseOptions";

    public int CommandTimeout { get; set; } = 30;
    public string ConnectionString { get; set; } = string.Empty;
    public string DbProvider { get; set; } = string.Empty;
    public bool EnableDetailedErrors { get; set; }
    public bool EnableSensitiveDataLogging { get; set; }
    public int MaxRetryCount { get; set; } = 3;
    public int MaxRetryDelay { get; set; } = 15;

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
