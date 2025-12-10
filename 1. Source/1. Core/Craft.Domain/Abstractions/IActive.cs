namespace Craft.Domain;

/// <summary>
/// Defines a contract for entities that can be activated or deactivated.
/// </summary>
public interface IHasActive
{
    /// <summary>
    /// The name of the database column for the IsActive property.
    /// </summary>
    public const string ColumnName = "IsActive";

    /// <summary>
    /// Gets or sets a value indicating whether the entity is active.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Activates the entity by setting IsActive to true.
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Deactivates the entity by setting IsActive to false.
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Sets the active state of the entity.
    /// </summary>
    /// <param name="isActive">The active state to set.</param>
    public void SetActive(bool isActive) => IsActive = isActive;

    /// <summary>
    /// Toggles the active state of the entity.
    /// </summary>
    public void ToggleActive() => IsActive = !IsActive;
}
