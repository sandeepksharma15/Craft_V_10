using Craft.UiComponents;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents a semantic section element for thematic grouping of content.
/// </summary>
public partial class CraftSection : CraftComponent
{
    /// <summary>
    /// Returns the base CSS class for this component.
    /// </summary>
    protected override string? GetComponentCssClass() => "craft-section";
}
