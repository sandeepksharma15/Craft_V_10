namespace Craft.Domain;

/// <summary>
/// Defines a contract for entities that support versioning for tracking changes.
/// </summary>
public interface IHasVersion
{
    /// <summary>
    /// The name of the database column for the Version property.
    /// </summary>
    public const string ColumnName = "Version";

    /// <summary>
    /// Gets or sets the version number of the entity.
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// Decrements the version number if it is greater than zero.
    /// </summary>
    public void DecrementVersion()
    {
        if (Version > 0)
            Version--;
    }

    /// <summary>
    /// Gets the current version number.
    /// </summary>
    /// <returns>The version number.</returns>
    public long GetVersion() => Version;

    /// <summary>
    /// Increments the version number.
    /// </summary>
    public void IncrementVersion() => Version++;

    /// <summary>
    /// Sets the version number.
    /// </summary>
    /// <param name="version">The version number to set.</param>
    public void SetVersion(long version) => Version = version;
}
