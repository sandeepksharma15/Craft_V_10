using Craft.UiComponents;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents a generic div element container.
/// </summary>
public partial class CraftDiv : CraftComponent
{
    /// <summary>
    /// Returns the base CSS class for this component.
    /// </summary>
    protected override string? GetComponentCssClass() => "craft-div";
}
