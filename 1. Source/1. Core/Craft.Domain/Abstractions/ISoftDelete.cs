namespace Craft.Domain;

/// <summary>
/// Defines a contract for entities that support soft deletion.
/// Soft-deleted entities are marked as deleted but not physically removed from the database.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// The name of the database column for the IsDeleted property.
    /// </summary>
    public const string ColumnName = "IsDeleted";

    /// <summary>
    /// Gets or sets a value indicating whether the entity is soft-deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Marks the entity as deleted by setting IsDeleted to true.
    /// </summary>
    public void Delete() => IsDeleted = true;

    /// <summary>
    /// Restores a soft-deleted entity by setting IsDeleted to false.
    /// </summary>
    public void Restore() => IsDeleted = false;
}
