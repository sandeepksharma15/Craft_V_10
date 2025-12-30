using Craft.UiComponents;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents a semantic header element for introductory content.
/// </summary>
public partial class CraftHeader : CraftComponent
{
    /// <summary>
    /// Returns the base CSS class for this component.
    /// </summary>
    protected override string? GetComponentCssClass() => "craft-header";
}
