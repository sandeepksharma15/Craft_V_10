using System.Text.Json;

namespace Craft.Auditing;

public static class AuditTrailExtensions
{
    public static Dictionary<string, JsonElement>? GetOldValuesAsDictionary(this AuditTrail audit)
    {
        ArgumentNullException.ThrowIfNull(audit);

        if (string.IsNullOrWhiteSpace(audit.OldValues))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                audit.OldValues, 
                AuditTrail.SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static Dictionary<string, JsonElement>? GetNewValuesAsDictionary(this AuditTrail audit)
    {
        ArgumentNullException.ThrowIfNull(audit);

        if (string.IsNullOrWhiteSpace(audit.NewValues))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                audit.NewValues, 
                AuditTrail.SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static Dictionary<string, JsonElement>? GetKeyValuesAsDictionary(this AuditTrail audit)
    {
        ArgumentNullException.ThrowIfNull(audit);

        if (string.IsNullOrWhiteSpace(audit.KeyValues))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                audit.KeyValues, 
                AuditTrail.SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static List<string>? GetChangedColumnsAsList(this AuditTrail audit)
    {
        ArgumentNullException.ThrowIfNull(audit);

        if (string.IsNullOrWhiteSpace(audit.ChangedColumns))
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<string>>(
                audit.ChangedColumns, 
                AuditTrail.SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static T? DeserializeOldValues<T>(this AuditTrail audit) where T : class
    {
        ArgumentNullException.ThrowIfNull(audit);

        if (string.IsNullOrWhiteSpace(audit.OldValues))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(audit.OldValues, AuditTrail.SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static T? DeserializeNewValues<T>(this AuditTrail audit) where T : class
    {
        ArgumentNullException.ThrowIfNull(audit);

        if (string.IsNullOrWhiteSpace(audit.NewValues))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(audit.NewValues, AuditTrail.SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static object? GetOldValue(this AuditTrail audit, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var values = audit.GetOldValuesAsDictionary();
        if (values == null || !values.ContainsKey(propertyName))
            return null;

        return values[propertyName];
    }

    public static object? GetNewValue(this AuditTrail audit, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var values = audit.GetNewValuesAsDictionary();
        if (values == null || !values.ContainsKey(propertyName))
            return null;

        return values[propertyName];
    }

    public static bool HasPropertyChanged(this AuditTrail audit, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var changedColumns = audit.GetChangedColumnsAsList();
        return changedColumns?.Contains(propertyName) ?? false;
    }

    public static TValue? GetOldValue<TValue>(this AuditTrail audit, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var values = audit.GetOldValuesAsDictionary();
        if (values == null || !values.TryGetValue(propertyName, out var element))
            return default;

        try
        {
            return element.Deserialize<TValue>(AuditTrail.SerializerOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public static TValue? GetNewValue<TValue>(this AuditTrail audit, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var values = audit.GetNewValuesAsDictionary();
        if (values == null || !values.TryGetValue(propertyName, out var element))
            return default;

        try
        {
            return element.Deserialize<TValue>(AuditTrail.SerializerOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
