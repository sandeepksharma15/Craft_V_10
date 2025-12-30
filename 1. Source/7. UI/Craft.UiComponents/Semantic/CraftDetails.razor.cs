using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents a semantic details element for disclosure widget.
/// </summary>
public partial class CraftDetails : CraftComponent
{
    /// <summary>
    /// Gets or sets whether the details are open.
    /// </summary>
    [Parameter] public bool IsOpen { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when open state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when details are toggled.
    /// </summary>
    [Parameter] public EventCallback<bool> OnToggle { get; set; }
    
    /// <summary>
    /// Handles the toggle event.
    /// </summary>
    private async Task HandleToggleAsync(EventArgs args)
    {
        IsOpen = !IsOpen;
        await IsOpenChanged.InvokeAsync(IsOpen);
        
        if (OnToggle.HasDelegate)
        {
            await OnToggle.InvokeAsync(IsOpen);
        }
    }
    
    /// <summary>
    /// Returns the base CSS class for this component.
    /// </summary>
    protected override string? GetComponentCssClass() => "craft-details";
}
