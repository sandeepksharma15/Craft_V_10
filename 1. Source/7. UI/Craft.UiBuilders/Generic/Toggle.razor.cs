using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that switches between two content states based on a boolean flag.
/// </summary>
public partial class Toggle : CraftComponent
{
    /// <summary>
    /// Gets or sets whether the toggle is in the active state.
    /// </summary>
    [Parameter]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the content to render when active.
    /// </summary>
    [Parameter]
    public RenderFragment? Active { get; set; }

    /// <summary>
    /// Gets or sets the content to render when inactive.
    /// </summary>
    [Parameter]
    public RenderFragment? Inactive { get; set; }
}
