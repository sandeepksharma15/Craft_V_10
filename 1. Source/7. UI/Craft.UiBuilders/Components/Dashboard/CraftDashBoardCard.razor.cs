using Craft.UiComponents;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Components.Dashboard;

/// <summary>
/// A reusable dashboard card with hover elevation, an icon, a title, child content area, and a configurable "More Info" link.
/// </summary>
public partial class CraftDashBoardCard : CraftComponent
{
    private const int DefaultElevation = 2;
    private const int HoverElevation = 8;

    private int _currentElevation = DefaultElevation;

    /// <summary>Gets or sets the icon displayed on the left side of the card.</summary>
    [Parameter] public string Icon { get; set; } = string.Empty;

    /// <summary>Gets or sets the route for the "More Info" navigation link.</summary>
    [Parameter] public string Href { get; set; } = string.Empty;

    /// <summary>Gets or sets the title label displayed at the top of the card.</summary>
    [Parameter] public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the card responds to hover with an elevated shadow.</summary>
    [Parameter] public bool IsHoverable { get; set; } = true;

    /// <summary>
    /// Gets or sets the icon for the "More Info" link.
    /// Defaults to <see cref="Icons.Material.Filled.ArrowForward"/>.
    /// </summary>
    [Parameter] public string MoreInfoIcon { get; set; } = Icons.Material.Filled.ArrowForward;

    private void OnMouseEnter()
    {
        if (IsHoverable)
            _currentElevation = HoverElevation;
    }

    private void OnMouseLeave()
    {
        if (IsHoverable)
            _currentElevation = DefaultElevation;
    }
}
