namespace Craft.Domain;

/// <summary>
/// Defines a contract for entities that support optimistic concurrency control.
/// </summary>
public interface IHasConcurrency
{
    /// <summary>
    /// The name of the database column for the ConcurrencyStamp property.
    /// </summary>
    public const string ColumnName = "ConcurrencyStamp";

    /// <summary>
    /// The maximum length of the concurrency stamp.
    /// </summary>
    public const int MaxLength = 40;

    /// <summary>
    /// Gets or sets the concurrency stamp for optimistic concurrency control.
    /// </summary>
    string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// Gets the current concurrency stamp.
    /// </summary>
    /// <returns>The concurrency stamp value.</returns>
    public string? GetConcurrencyStamp() => ConcurrencyStamp;

    /// <summary>
    /// Determines whether the entity has a concurrency stamp set.
    /// </summary>
    /// <returns>True if a concurrency stamp is set; otherwise, false.</returns>
    public bool HasConcurrencyStamp() => !string.IsNullOrWhiteSpace(ConcurrencyStamp);

    /// <summary>
    /// Sets the concurrency stamp. If no stamp is provided, generates a new GUID.
    /// </summary>
    /// <param name="stamp">The concurrency stamp to set. If null, a new GUID is generated.</param>
    public void SetConcurrencyStamp(string? stamp = null)
        => ConcurrencyStamp = stamp ?? Guid.NewGuid().ToString();

    /// <summary>
    /// Clears the concurrency stamp by setting it to null.
    /// </summary>
    public void ClearConcurrencyStamp() => ConcurrencyStamp = null;
}
