using System.ComponentModel.DataAnnotations;
using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components;

public partial class CraftDarkModeToggle : CraftComponent
{
    [Parameter, Required] public bool DarkMode { get; set; }
    [Parameter, Required] public EventCallback<bool> DarkModeChanged { get; set; }

    public async Task OnDarkModeChanged(bool changed)
    {
        DarkMode = changed;
        await DarkModeChanged.InvokeAsync(DarkMode);
    }
}

