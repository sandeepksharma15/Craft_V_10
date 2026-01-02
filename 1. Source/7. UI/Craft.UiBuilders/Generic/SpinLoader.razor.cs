using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

public partial class SpinLoader : CraftComponent
{
    [Parameter] public RenderFragment Content { get; set; }

    [Parameter] public bool IsFaulted { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter] public RenderFragment Loading { get; set; }
}
