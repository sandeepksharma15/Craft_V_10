using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that conditionally renders its child content when the condition is true.
/// Simpler alternative to <see cref="If"/> when only the "if true" case is needed.
/// </summary>
public partial class Show : CraftComponent
{
    /// <summary>
    /// Gets or sets the condition that determines whether the content should be displayed.
    /// When true, the child content is rendered; when false, nothing is rendered.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public bool When { get; set; }
}
