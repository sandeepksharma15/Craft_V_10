using System.Text.Json;

namespace Craft.Auditing;

public static class AuditTrailValidator
{
    public static AuditValidationResult Validate(AuditTrail audit)
    {
        ArgumentNullException.ThrowIfNull(audit);

        var result = new AuditValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(audit.TableName))
        {
            result.IsValid = false;
            result.Errors.Add("TableName is required");
        }
        else if (audit.TableName.Length > AuditTrail.MaxTableNameLength)
        {
            result.IsValid = false;
            result.Errors.Add($"TableName exceeds maximum length of {AuditTrail.MaxTableNameLength} characters");
        }

        if (audit.KeyValues != null && audit.KeyValues.Length > AuditTrail.MaxKeyValuesLength)
        {
            result.IsValid = false;
            result.Errors.Add($"KeyValues exceeds maximum length of {AuditTrail.MaxKeyValuesLength} characters");
        }

        if (audit.ChangedColumns != null && audit.ChangedColumns.Length > AuditTrail.MaxChangedColumnsLength)
        {
            result.IsValid = false;
            result.Errors.Add($"ChangedColumns exceeds maximum length of {AuditTrail.MaxChangedColumnsLength} characters");
        }

        if (audit.DateTimeUTC == default)
        {
            result.IsValid = false;
            result.Errors.Add("DateTimeUTC must be set");
        }

        if (audit.DateTimeUTC.Kind != DateTimeKind.Utc)
        {
            result.Warnings.Add("DateTimeUTC should be in UTC format");
        }

        if (audit.ArchiveAfter.HasValue && audit.ArchiveAfter.Value.Kind != DateTimeKind.Utc)
        {
            result.Warnings.Add("ArchiveAfter should be in UTC format");
        }

        if (audit.ArchiveAfter.HasValue && audit.ArchiveAfter.Value < audit.DateTimeUTC)
        {
            result.IsValid = false;
            result.Errors.Add("ArchiveAfter must be greater than or equal to DateTimeUTC");
        }

        ValidateJsonField(audit.KeyValues, "KeyValues", result);
        ValidateJsonField(audit.ChangedColumns, "ChangedColumns", result);
        ValidateJsonField(audit.OldValues, "OldValues", result);
        ValidateJsonField(audit.NewValues, "NewValues", result);

        return result;
    }

    private static void ValidateJsonField(string? jsonField, string fieldName, AuditValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(jsonField))
            return;

        try
        {
            JsonDocument.Parse(jsonField);
        }
        catch (JsonException ex)
        {
            result.IsValid = false;
            result.Errors.Add($"{fieldName} contains invalid JSON: {ex.Message}");
        }
    }

    public static bool WillExceedMaxLength(string? value, int maxLength, out int actualLength)
    {
        actualLength = value?.Length ?? 0;
        return actualLength > maxLength;
    }

    public static string TruncateIfNeeded(string? value, int maxLength, string truncationSuffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value ?? string.Empty;

        var availableLength = maxLength - truncationSuffix.Length;
        if (availableLength <= 0)
            return truncationSuffix[..maxLength];

        return value[..availableLength] + truncationSuffix;
    }
}

public class AuditValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public bool HasWarnings => Warnings.Count > 0;
    public bool HasErrors => Errors.Count > 0;
}
