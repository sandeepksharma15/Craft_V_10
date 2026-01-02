using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that renders its child content without adding any wrapper elements to the DOM.
/// Similar to React.Fragment, useful for grouping elements without introducing extra HTML.
/// </summary>
public partial class Fragment : ComponentBase
{
    /// <summary>
    /// Gets or sets the child content to be rendered.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
