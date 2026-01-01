using System.ComponentModel.DataAnnotations;
using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components;

public partial class DarkModeToggle : CraftComponent
{
    [Parameter, Required] public bool DarkMode { get; set; }
    [Parameter, Required] public EventCallback<bool>? DarkModeChanged { get; set; }

    public void ToggleLightMode(bool changed)
    {
        DarkMode = changed;
        DarkModeChanged?.InvokeAsync(DarkMode);
    }
}
