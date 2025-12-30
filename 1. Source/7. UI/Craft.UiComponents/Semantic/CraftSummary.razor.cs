using Craft.UiComponents;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents a semantic summary element for details summary/legend.
/// </summary>
public partial class CraftSummary : CraftComponent
{
    /// <summary>
    /// Returns the base CSS class for this component.
    /// </summary>
    protected override string? GetComponentCssClass() => "craft-summary";
}
