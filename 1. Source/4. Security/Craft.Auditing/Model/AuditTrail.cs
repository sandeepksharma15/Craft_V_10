using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Craft.Auditing;

[Table("HK_AuditTrail")]
public class AuditTrail : BaseEntity, IAuditTrail
{
    /// <summary>
    /// Maximum length for ChangedColumns JSON field.
    /// </summary>
    public const int MaxChangedColumnsLength = 2000;

    /// <summary>
    /// Maximum length for KeyValues JSON field.
    /// </summary>
    public const int MaxKeyValuesLength = 1000;

    /// <summary>
    /// Maximum length for TableName field.
    /// </summary>
    public const int MaxTableNameLength = 255;

    private static JsonSerializerOptions? _customSerializerOptions;
    private static int? _defaultRetentionDays;
    private static bool _includeNavigationProperties = true;

    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        MaxDepth = 32
    };

    /// <summary>
    /// Gets the currently configured JsonSerializerOptions.
    /// Returns custom options if set, otherwise returns default options.
    /// </summary>
    public static JsonSerializerOptions SerializerOptions =>
        _customSerializerOptions ?? DefaultSerializerOptions;

    /// <summary>
    /// Gets or sets the default retention period in days for audit trail entries.
    /// When set, new audit trail entries will automatically have ArchiveAfter set to DateTimeUTC + DefaultRetentionDays.
    /// Set to null to disable automatic retention policy.
    /// </summary>
    public static int? DefaultRetentionDays
    {
        get => _defaultRetentionDays;
        set
        {
            if (value.HasValue && value.Value < 1)
                throw new ArgumentException("Default retention days must be greater than 0", nameof(value));
            _defaultRetentionDays = value;
        }
    }

    /// <summary>
    /// Gets or sets whether to include navigation property changes in audit trails.
    /// When true, foreign key changes will show both the FK value and related entity information.
    /// Default is true.
    /// </summary>
    public static bool IncludeNavigationProperties
    {
        get => _includeNavigationProperties;
        set => _includeNavigationProperties = value;
    }

    /// <summary>
    /// Configures custom JsonSerializerOptions for audit trail serialization.
    /// This affects all future audit trail entries.
    /// </summary>
    /// <param name="options">The custom JsonSerializerOptions to use. Pass null to reset to default.</param>
    public static void ConfigureSerializerOptions(JsonSerializerOptions? options)
    {
        _customSerializerOptions = options;
    }

    [MaxLength(MaxChangedColumnsLength)]
    public string? ChangedColumns { get; set; }

    [Required]
    public EntityChangeType ChangeType { get; set; }

    [Required]
    public DateTime DateTimeUTC { get; set; } = DateTime.UtcNow;

    [MaxLength(MaxKeyValuesLength)]
    public string? KeyValues { get; set; }

    // Use unlimited string without database-specific TypeName for cross-database compatibility
    // EF Core will use 'nvarchar(max)' for SQL Server and 'text' for PostgreSQL automatically
    public string? NewValues { get; set; }

    // Use unlimited string without database-specific TypeName for cross-database compatibility
    // EF Core will use 'nvarchar(max)' for SQL Server and 'text' for PostgreSQL automatically
    public string? OldValues { get; set; }

    [Required]
    [MaxLength(MaxTableNameLength)]
    public string? TableName { get; set; }

    [Required]
    public KeyType UserId { get; set; }

    public DateTime? ArchiveAfter { get; set; }
    public bool IsArchived { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditTrail"/> class.
    /// For Entity Framework use only. Use <see cref="Create"/> or <see cref="CreateAsync"/> for creating audit trail instances.
    /// </summary>
    public AuditTrail() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditTrail"/> class.
    /// Consider using <see cref="Create"/> or <see cref="CreateAsync"/> factory methods instead.
    /// </summary>
    /// <param name="entity">The entity entry to audit.</param>
    /// <param name="userId">The ID of the user making the change.</param>
    public AuditTrail(EntityEntry entity, KeyType userId)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        UserId = userId;
        PopulateFromEntityEntry(entity);
        ApplyRetentionPolicy();
    }

    /// <summary>
    /// Creates an audit trail entry synchronously from an entity entry.
    /// </summary>
    /// <param name="entity">The entity entry to audit.</param>
    /// <param name="userId">The ID of the user making the change.</param>
    /// <returns>A new <see cref="AuditTrail"/> instance.</returns>
    public static AuditTrail Create(EntityEntry entity, KeyType userId)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var auditTrail = new AuditTrail
        {
            UserId = userId
        };

        auditTrail.PopulateFromEntityEntry(entity);
        auditTrail.ApplyRetentionPolicy();
        return auditTrail;
    }

    /// <summary>
    /// Creates an audit trail entry asynchronously from an entity entry.
    /// This method allows for future async operations during audit trail creation.
    /// </summary>
    /// <param name="entity">The entity entry to audit.</param>
    /// <param name="userId">The ID of the user making the change.</param>
    /// <param name="_">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the new <see cref="AuditTrail"/> instance.</returns>
    public static Task<AuditTrail> CreateAsync(EntityEntry entity, KeyType userId, CancellationToken _ = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var auditTrail = new AuditTrail
        {
            UserId = userId
        };

        auditTrail.PopulateFromEntityEntry(entity);
        auditTrail.ApplyRetentionPolicy();
        
        return Task.FromResult(auditTrail);
    }

    /// <summary>
    /// Marks this audit trail entry as archived.
    /// </summary>
    public void Archive()
    {
        IsArchived = true;
    }

    /// <summary>
    /// Restores this audit trail entry from archived status.
    /// </summary>
    public void Unarchive()
    {
        IsArchived = false;
    }

    /// <summary>
    /// Determines whether this audit trail entry is eligible for archival based on the ArchiveAfter date.
    /// </summary>
    /// <returns>True if the entry should be archived; otherwise, false.</returns>
    public bool ShouldBeArchived()
    {
        return ArchiveAfter.HasValue && DateTime.UtcNow >= ArchiveAfter.Value && !IsArchived;
    }

    /// <summary>
    /// Sets a custom retention policy for this specific audit trail entry.
    /// </summary>
    /// <param name="retentionDays">Number of days to retain this entry before archival.</param>
    public void SetRetentionPolicy(int retentionDays)
    {
        if (retentionDays < 1)
            throw new ArgumentException("Retention days must be greater than 0", nameof(retentionDays));

        ArchiveAfter = DateTimeUTC.AddDays(retentionDays);
    }

    private void ApplyRetentionPolicy()
    {
        if (DefaultRetentionDays.HasValue && !ArchiveAfter.HasValue)
        {
            ArchiveAfter = DateTimeUTC.AddDays(DefaultRetentionDays.Value);
        }
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
            if (ShouldSkipProperty(entity.Entity, property.Metadata.Name))
                continue;

            var propertyName = property.Metadata.Name;

            if (property.Metadata.IsPrimaryKey())
                keyValues[propertyName] = property.CurrentValue ?? string.Empty;

            newValues[propertyName] = property.CurrentValue ?? string.Empty;
        }

        if (IncludeNavigationProperties)
            ExtractNavigationProperties(entity, newValues, null);
    }

    private static void ExtractDeletedValues(EntityEntry entity, Dictionary<string, object> keyValues, Dictionary<string, object> oldValues)
    {
        foreach (var property in entity.Properties)
        {
            if (ShouldSkipProperty(entity.Entity, property.Metadata.Name))
                continue;

            var propertyName = property.Metadata.Name;

            if (property.Metadata.IsPrimaryKey())
                keyValues[propertyName] = property.CurrentValue ?? string.Empty;

            oldValues[propertyName] = property.OriginalValue ?? string.Empty;
        }

        if (IncludeNavigationProperties)
            ExtractNavigationProperties(entity, null, oldValues);
    }

    private static void ExtractUpdatedValues(EntityEntry entity, Dictionary<string, object> keyValues,
        Dictionary<string, object> oldValues, Dictionary<string, object> newValues, List<string> changedColumns)
    {
        foreach (var property in entity.Properties)
        {
            var propertyName = property.Metadata.Name;

            if (property.Metadata.IsPrimaryKey())
                keyValues[propertyName] = property.CurrentValue ?? string.Empty;

            if (ShouldSkipProperty(entity.Entity, propertyName))
                continue;

            var dbColumnName = property.Metadata.GetFieldName();

            if (property.IsModified && !Equals(property.CurrentValue ?? string.Empty, property.OriginalValue ?? string.Empty))
            {
                changedColumns.Add(dbColumnName ?? propertyName);
                oldValues[propertyName] = property.OriginalValue ?? string.Empty;
                newValues[propertyName] = property.CurrentValue ?? string.Empty;
            }
        }

        if (IncludeNavigationProperties)
            ExtractNavigationProperties(entity, newValues, oldValues);
    }

    private static void ExtractNavigationProperties(EntityEntry entity, Dictionary<string, object>? newValues, Dictionary<string, object>? oldValues)
    {
        foreach (var reference in entity.References)
        {
            var navigationName = reference.Metadata.Name;

            if (ShouldSkipProperty(entity.Entity, navigationName))
                continue;

            // Cast to INavigation to access ForeignKey
            if (reference.Metadata is not Microsoft.EntityFrameworkCore.Metadata.INavigation navigation)
                continue;

            // Get the foreign key properties for this navigation
            var foreignKey = navigation.ForeignKey;
            if (foreignKey == null || !foreignKey.Properties.Any())
                continue;

            // For simplicity, use the first FK property
            var foreignKeyProperty = foreignKey.Properties[0];
            var fkPropertyEntry = entity.Property(foreignKeyProperty.Name);

            // Track new value (for Added/Modified)
            if (newValues != null && fkPropertyEntry.CurrentValue != null)
            {
                var relatedEntity = reference.CurrentValue;
                var navigationInfo = new Dictionary<string, object?>
                {
                    ["ForeignKey"] = fkPropertyEntry.CurrentValue,
                    ["RelatedEntity"] = GetNavigationDisplayValue(relatedEntity)
                };
                newValues[$"{navigationName}_Navigation"] = navigationInfo;
            }

            // Track old value (for Modified/Deleted)
            if (oldValues != null)
            {
                var oldFkValue = fkPropertyEntry.OriginalValue;
                if (oldFkValue != null && !Equals(oldFkValue, fkPropertyEntry.CurrentValue))
                {
                    var navigationInfo = new Dictionary<string, object?>
                    {
                        ["ForeignKey"] = oldFkValue,
                        ["RelatedEntity"] = "Previous value (not loaded)"
                    };
                    oldValues[$"{navigationName}_Navigation"] = navigationInfo;
                }
            }
        }
    }

    private static object? GetNavigationDisplayValue(object? entity)
    {
        if (entity == null)
            return null;

        var entityType = entity.GetType();

        var nameProperty = entityType.GetProperty("Name") ?? 
                          entityType.GetProperty("Title") ??
                          entityType.GetProperty("DisplayName");

        if (nameProperty != null)
        {
            var nameValue = nameProperty.GetValue(entity);
            if (nameValue != null)
                return nameValue;
        }

        var idProperty = entityType.GetProperty("Id");
        if (idProperty != null)
        {
            var idValue = idProperty.GetValue(entity);
            return new { Type = entityType.Name, Id = idValue };
        }

        return new { Type = entityType.Name };
    }

    private static bool ShouldSkipProperty(object entity, string propertyName)
    {
        var entityType = entity.GetType();
        var propertyInfo = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo == null)
            return false;

        return propertyInfo.IsDefined(typeof(DoNotAuditAttribute), inherit: false);
    }

    private static bool IsItASoftDeleteUpdate(EntityEntry entity)
    {
        foreach (var property in entity.Properties)
            if (property.Metadata.Name == ISoftDelete.ColumnName && property.CurrentValue is true)
                return true;

        return false;
    }

    /// <summary>
    /// Serializes the value to JSON if it contains data, otherwise returns null.
    /// Returns null for empty collections or dictionaries.
    /// </summary>
    private static string? SerializeOrNull<T>(T value)
    {
        if (value is null)
            return null;

        if (value is ICollection<object> collection && collection.Count == 0)
            return null;

        if (value is IDictionary<string, object> dict && dict.Count == 0)
            return null;

        return JsonSerializer.Serialize(value, SerializerOptions);
    }
}
