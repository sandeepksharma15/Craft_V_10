using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that conditionally renders its child content when the condition is false.
/// Simpler alternative to <see cref="Conditional"/> when only the "if false" case is needed.
/// </summary>
public partial class Hide : CraftComponent
{
    /// <summary>
    /// Gets or sets the condition that determines whether the content should be hidden.
    /// When false, the child content is rendered; when true, nothing is rendered.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public bool When { get; set; }
}
