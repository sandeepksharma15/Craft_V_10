using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents a semantic nav element for navigation links.
/// </summary>
public partial class CraftNav : CraftComponent
{
    /// <summary>
    /// Gets or sets the ARIA label for the navigation.
    /// </summary>
    [Parameter] public string? AriaLabel { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of element that labels this navigation.
    /// </summary>
    [Parameter] public string? AriaLabelledBy { get; set; }
    
    /// <summary>
    /// Returns the base CSS class for this component.
    /// </summary>
    protected override string? GetComponentCssClass() => "craft-nav";
}
