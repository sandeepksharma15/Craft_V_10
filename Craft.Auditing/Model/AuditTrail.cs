using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Craft.Auditing;

[Table("HK_AuditTrail")]
public class AuditTrail : BaseEntity, IAuditTrail
{
    public string? ChangedColumns { get; set; }
    public EntityChangeType ChangeType { get; set; }
    public DateTime DateTimeUTC { get; set; } = DateTime.UtcNow;

    public string? KeyValues { get; set; }
    public string? NewValues { get; set; }
    public string? OldValues { get; set; }
    public bool ShowDetails { get; set; }
    public string? TableName { get; set; }
    public KeyType UserId { get; set; }

    public AuditTrail() { }

    public AuditTrail(EntityEntry entity, KeyType userId)
    {
        // Validate the entity entry is not null
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        // Set UserId
        UserId = userId;

        // Populate the audit trail properties based on the entity entry state
        PopulateFromEntityEntry(entity);
    }

    private void PopulateFromEntityEntry(EntityEntry entity)
    {
        var keyValues = new Dictionary<string, object>();
        var oldValues = new Dictionary<string, object>();
        var newValues = new Dictionary<string, object>();
        var changedColumns = new List<string>();

        switch (entity.State)
        {
            case EntityState.Added:
                ChangeType = EntityChangeType.Created;
                ExtractAddedValues(entity, keyValues, newValues);
                break;

            case EntityState.Deleted:
                if (entity.Entity is ISoftDelete)
                    entity.State = EntityState.Modified;
                ChangeType = EntityChangeType.Deleted;
                ExtractDeletedValues(entity, keyValues, oldValues);
                break;

            case EntityState.Modified:
                if (IsItASoftDeleteUpdate(entity))
                {
                    ChangeType = EntityChangeType.Deleted;
                    ExtractDeletedValues(entity, keyValues, oldValues);
                }
                else
                {
                    ChangeType = EntityChangeType.Updated;
                    ExtractUpdatedValues(entity, keyValues, oldValues, newValues, changedColumns);
                }
                break;

            default:
                ChangeType = EntityChangeType.None;
                break;
        }

        TableName = entity.Metadata.DisplayName();
        DateTimeUTC = DateTime.UtcNow;
        KeyValues = SerializeOrNull(keyValues);
        ChangedColumns = SerializeOrNull(changedColumns);
        OldValues = SerializeOrNull(oldValues);
        NewValues = SerializeOrNull(newValues);
    }

    private static void ExtractAddedValues(EntityEntry entity, Dictionary<string, object> keyValues, Dictionary<string, object> newValues)
    {
        foreach (var property in entity.Properties)
        {
            var propertyName = property.Metadata.Name;

            if (property.Metadata.IsPrimaryKey())
                keyValues[propertyName] = property.CurrentValue ?? string.Empty;

            newValues[propertyName] = property.CurrentValue ?? string.Empty;
        }
    }

    private static void ExtractDeletedValues(EntityEntry entity, Dictionary<string, object> keyValues, Dictionary<string, object> oldValues)
    {
        foreach (var property in entity.Properties)
        {
            var propertyName = property.Metadata.Name;

            if (property.Metadata.IsPrimaryKey())
                keyValues[propertyName] = property.CurrentValue ?? string.Empty;

            oldValues[propertyName] = property.OriginalValue ?? string.Empty;
        }
    }

    private static void ExtractUpdatedValues(EntityEntry entity, Dictionary<string, object> keyValues,
        Dictionary<string, object> oldValues, Dictionary<string, object> newValues, List<string> changedColumns)
    {
        foreach (var property in entity.Properties)
        {
            var propertyName = property.Metadata.Name;
            var dbColumnName = property.Metadata.GetFieldName();

            if (property.Metadata.IsPrimaryKey())
                keyValues[propertyName] = property.CurrentValue ?? string.Empty;

            if (property.IsModified && !Equals(property.CurrentValue ?? string.Empty, property.OriginalValue ?? string.Empty))
            {
                changedColumns.Add(dbColumnName ?? string.Empty);
                oldValues[propertyName] = property.OriginalValue ?? string.Empty;
                newValues[propertyName] = property.CurrentValue ?? string.Empty;
            }
        }
    }

    private static bool IsItASoftDeleteUpdate(EntityEntry entity)
    {
        foreach (var property in entity.Properties)
            if (property.Metadata.Name == ISoftDelete.ColumnName && property.CurrentValue is true)
                return true;

        return false;
    }

    private static string? SerializeOrNull<T>(T value)
    {
        if (value is ICollection<object> collection && collection.Count == 0)
            return null;

        if (value is IDictionary<string, object> dict && dict.Count == 0)
            return null;

        if (value is null)
            return null;

        return JsonSerializer.Serialize(value);
    }
}
