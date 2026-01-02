using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that displays placeholder/skeleton content during loading.
/// </summary>
public partial class Placeholder : CraftComponent
{
    /// <summary>
    /// Gets or sets whether the placeholder is currently in loading state.
    /// </summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>
    /// Gets or sets the number of placeholder lines to display.
    /// </summary>
    [Parameter]
    public int Lines { get; set; } = 3;

    /// <summary>
    /// Gets or sets the variant of the placeholder.
    /// </summary>
    [Parameter]
    public PlaceholderVariant Variant { get; set; } = PlaceholderVariant.Text;

    /// <summary>
    /// Gets or sets the actual content to display when not loading.
    /// </summary>
    [Parameter]
    public RenderFragment? Content { get; set; }
}

/// <summary>
/// Defines the visual variant of the placeholder.
/// </summary>
public enum PlaceholderVariant
{
    /// <summary>
    /// Text line placeholder.
    /// </summary>
    Text,

    /// <summary>
    /// Circular placeholder for avatars.
    /// </summary>
    Circle,

    /// <summary>
    /// Rectangular placeholder for images or cards.
    /// </summary>
    Rectangle
}
