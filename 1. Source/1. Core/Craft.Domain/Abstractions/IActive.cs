namespace Craft.Domain;

public interface IHasActive
{
    public const string ColumnName = "IsActive";

    bool IsActive { get; set; }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void SetActive(bool isActive) => IsActive = isActive;

    public void ToggleActive() => IsActive = !IsActive;
}
