using System.Text.Json;

namespace Craft.Auditing;

public class AuditDifference
{
    public string PropertyName { get; set; } = string.Empty;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public bool HasChanged { get; set; }
}

public class AuditTrailComparison
{
    public AuditTrail? CurrentAudit { get; set; }
    public AuditTrail? PreviousAudit { get; set; }
    public List<AuditDifference> Differences { get; set; } = [];
    public bool HasDifferences => Differences.Count > 0;
}

public static class AuditTrailComparer
{
    public static AuditTrailComparison CompareTo(this AuditTrail current, AuditTrail? previous)
    {
        ArgumentNullException.ThrowIfNull(current);

        var comparison = new AuditTrailComparison
        {
            CurrentAudit = current,
            PreviousAudit = previous
        };

        if (previous == null)
        {
            return comparison;
        }

        if (current.TableName != previous.TableName)
        {
            throw new ArgumentException("Cannot compare audit trails from different tables");
        }

        var currentNewValues = current.GetNewValuesAsDictionary();
        var previousNewValues = previous.GetNewValuesAsDictionary();

        if (currentNewValues == null && previousNewValues == null)
        {
            return comparison;
        }

        if (currentNewValues == null || previousNewValues == null)
        {
            return comparison;
        }

        var allProperties = currentNewValues.Keys.Union(previousNewValues.Keys).ToList();

        foreach (var propertyName in allProperties)
        {
            var currentHasProperty = currentNewValues.TryGetValue(propertyName, out var currentValue);
            var previousHasProperty = previousNewValues.TryGetValue(propertyName, out var previousValue);

            if (!currentHasProperty && !previousHasProperty)
                continue;

            var currentValueString = currentHasProperty ? currentValue.ToString() : null;
            var previousValueString = previousHasProperty ? previousValue.ToString() : null;

            var hasChanged = currentValueString != previousValueString;

            comparison.Differences.Add(new AuditDifference
            {
                PropertyName = propertyName,
                OldValue = previousHasProperty ? previousValue : null,
                NewValue = currentHasProperty ? currentValue : null,
                HasChanged = hasChanged
            });
        }

        return comparison;
    }

    public static bool PropertyWasModified(this AuditTrail audit, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (audit.ChangeType != EntityChangeType.Updated)
            return false;

        var changedColumns = audit.GetChangedColumnsAsList();
        return changedColumns?.Contains(propertyName) ?? false;
    }

    public static List<AuditDifference> GetPropertyChanges(this AuditTrail audit)
    {
        ArgumentNullException.ThrowIfNull(audit);

        var differences = new List<AuditDifference>();

        var oldValues = audit.GetOldValuesAsDictionary();
        var newValues = audit.GetNewValuesAsDictionary();

        if (oldValues == null && newValues == null)
            return differences;

        var allProperties = (oldValues?.Keys ?? Enumerable.Empty<string>())
            .Union(newValues?.Keys ?? Enumerable.Empty<string>())
            .ToList();

        foreach (var propertyName in allProperties)
        {
            JsonElement oldValue = default;
            JsonElement newValue = default;
            
            var hasOldValue = oldValues?.TryGetValue(propertyName, out oldValue) ?? false;
            var hasNewValue = newValues?.TryGetValue(propertyName, out newValue) ?? false;

            if (!hasOldValue && !hasNewValue)
                continue;

            var oldValueString = hasOldValue ? oldValue.ToString() : null;
            var newValueString = hasNewValue ? newValue.ToString() : null;

            differences.Add(new AuditDifference
            {
                PropertyName = propertyName,
                OldValue = hasOldValue ? (object)oldValue : null,
                NewValue = hasNewValue ? (object)newValue : null,
                HasChanged = oldValueString != newValueString
            });
        }

        return differences;
    }

    public static object? GetPropertyValue(this AuditTrail audit, string propertyName, bool useNewValue = true)
    {
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        return useNewValue 
            ? audit.GetNewValue(propertyName) 
            : audit.GetOldValue(propertyName);
    }

    public static Dictionary<string, (object? OldValue, object? NewValue)> GetChangedProperties(this AuditTrail audit)
    {
        ArgumentNullException.ThrowIfNull(audit);

        var result = new Dictionary<string, (object? OldValue, object? NewValue)>();

        if (audit.ChangeType != EntityChangeType.Updated)
            return result;

        var changedColumns = audit.GetChangedColumnsAsList();
        if (changedColumns == null || changedColumns.Count == 0)
            return result;

        var oldValues = audit.GetOldValuesAsDictionary();
        var newValues = audit.GetNewValuesAsDictionary();

        foreach (var columnName in changedColumns)
        {
            object? oldValue = null;
            object? newValue = null;

            if (oldValues != null && oldValues.TryGetValue(columnName, out var oldElement))
                oldValue = oldElement;

            if (newValues != null && newValues.TryGetValue(columnName, out var newElement))
                newValue = newElement;

            result[columnName] = (oldValue, newValue);
        }

        return result;
    }
}
