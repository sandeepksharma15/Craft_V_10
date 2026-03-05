using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components.Dashboard;

/// <summary>
/// A display-only dashboard card that renders an animated entity count with loading and error states.
/// Pair with a data-fetching wrapper that supplies <see cref="Count"/>, <see cref="IsLoading"/>, and <see cref="HasError"/>.
/// </summary>
public partial class CraftEntityCountCard : CraftComponent
{
    /// <summary>Gets or sets the icon displayed on the card.</summary>
    [Parameter, EditorRequired] public required string Icon { get; set; }

    /// <summary>Gets or sets the title/label for the card.</summary>
    [Parameter, EditorRequired] public required string Title { get; set; }

    /// <summary>Gets or sets the route to navigate to when clicking "More Info".</summary>
    [Parameter] public string? Href { get; set; }

    /// <summary>Gets or sets the entity count to display.</summary>
    [Parameter] public long Count { get; set; }

    /// <summary>Gets or sets a value indicating whether data is currently loading.</summary>
    [Parameter] public bool IsLoading { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether an error occurred while loading.</summary>
    [Parameter] public bool HasError { get; set; }

    /// <summary>
    /// Gets or sets the animation duration in milliseconds for the running number effect.
    /// Defaults to 500ms.
    /// </summary>
    [Parameter] public int AnimationDuration { get; set; } = 500;
}
