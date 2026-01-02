using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that conditionally renders different content based on a boolean condition.
/// Renders the <see cref="True"/> content when <see cref="Condition"/> is true,
/// otherwise renders the <see cref="False"/> content.
/// </summary>
public partial class If : CraftComponent
{
    [Parameter]
    [EditorRequired]
    public bool Condition { get; set; }

    [Parameter]
    public RenderFragment? False { get; set; }

    [Parameter]
    public RenderFragment? True { get; set; }
}
