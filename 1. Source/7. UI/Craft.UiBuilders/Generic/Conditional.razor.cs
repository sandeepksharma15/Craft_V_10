using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

public partial class Conditional : CraftComponent
{
    [Parameter]
    [EditorRequired]
    public bool Condition { get; set; }

    [Parameter]
    public RenderFragment? False { get; set; }

    [Parameter]
    public RenderFragment? True { get; set; }
}
