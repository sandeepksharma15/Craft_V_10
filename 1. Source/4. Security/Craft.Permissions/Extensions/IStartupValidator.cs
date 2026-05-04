namespace Craft.Permissions;

/// <summary>
/// Marker interface for services that must be validated at application startup.
/// </summary>
public interface IStartupValidator
{
    void Validate();
}
