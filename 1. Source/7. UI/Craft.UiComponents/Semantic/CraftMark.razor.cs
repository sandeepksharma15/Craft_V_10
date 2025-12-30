using Craft.UiComponents;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents a semantic mark element for highlighted text.
/// </summary>
public partial class CraftMark : CraftComponent
{
    /// <summary>
    /// Returns the base CSS class for this component.
    /// </summary>
    protected override string? GetComponentCssClass() => "craft-mark";
}
