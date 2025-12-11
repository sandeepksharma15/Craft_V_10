using Craft.Domain;

namespace Craft.Auditing;

/// <summary>
/// Defines the contract for audit trail entities.
/// </summary>
public interface IAuditTrail : ISoftDelete, IHasUser
{
    /// <summary>
    /// Gets or sets the JSON string containing the names of changed columns.
    /// Maximum length: <see cref="AuditTrail.MaxChangedColumnsLength"/>.
    /// </summary>
    string? ChangedColumns { get; set; }

    /// <summary>
    /// Gets or sets the type of entity change (Created, Updated, Deleted, None).
    /// </summary>
    EntityChangeType ChangeType { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the change occurred.
    /// </summary>
    DateTime DateTimeUTC { get; set; }

    /// <summary>
    /// Gets or sets the JSON string containing the primary key values.
    /// Maximum length: <see cref="AuditTrail.MaxKeyValuesLength"/>.
    /// </summary>
    string? KeyValues { get; set; }

    /// <summary>
    /// Gets or sets the JSON string containing the new property values.
    /// Stored as nvarchar(max) to accommodate large entities.
    /// </summary>
    string? NewValues { get; set; }

    /// <summary>
    /// Gets or sets the JSON string containing the old property values.
    /// Stored as nvarchar(max) to accommodate large entities.
    /// </summary>
    string? OldValues { get; set; }

    /// <summary>
    /// Gets or sets the name of the table/entity being audited.
    /// Maximum length: <see cref="AuditTrail.MaxTableNameLength"/>.
    /// </summary>
    string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time after which this audit trail entry should be archived.
    /// Null indicates no specific archival date.
    /// </summary>
    DateTime? ArchiveAfter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this audit trail entry has been archived.
    /// </summary>
    bool IsArchived { get; set; }
}
