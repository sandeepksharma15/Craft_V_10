using Craft.UiComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>Gets or sets the icon displayed on the left side of the card.</summary>
    [Parameter] public string Icon { get; set; } = string.Empty;

    /// <summary>Gets or sets the route for the "More Info" navigation link.</summary>
    [Parameter] public string Href { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether to open <see cref="Href"/> in a new browser tab.</summary>
    [Parameter] public bool OpenInNewTab { get; set; }

    /// <summary>Gets or sets the title label displayed at the top of the card.</summary>
    [Parameter] public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the card responds to hover with an elevated shadow.</summary>
    [Parameter] public bool IsHoverable { get; set; } = true;

    /// <summary>Gets or sets the label text for the "More Info" navigation link.</summary>
    [Parameter] public string MoreInfoText { get; set; } = "More Info";

    /// <summary>
    /// Gets or sets the icon for the "More Info" link.
    /// Defaults to <see cref="Icons.Material.Filled.ArrowForward"/>.
    /// </summary>
    [Parameter] public string MoreInfoIcon { get; set; } = Icons.Material.Filled.ArrowForward;

    /// <summary>Gets or sets the color applied to the card background. Defaults to <see cref="Color.Default"/>.</summary>
    [Parameter] public Color Color { get; set; } = Color.Default;

    /// <summary>
    /// Gets the MudBlazor theme CSS class that corresponds to <see cref="Color"/>.
    /// Returns an empty string for <see cref="Color.Default"/> and <see cref="Color.Inherit"/>,
    /// which leaves the card with its default surface appearance.
    /// </summary>
    private string ColorClass => Color switch
    {
        Color.Primary   => "mud-theme-primary",
        Color.Secondary => "mud-theme-secondary",
        Color.Tertiary  => "mud-theme-tertiary",
        Color.Info      => "mud-theme-info",
        Color.Success   => "mud-theme-success",
        Color.Warning   => "mud-theme-warning",
        Color.Error     => "mud-theme-error",
        Color.Dark      => "mud-theme-dark",
        _               => string.Empty,
    };

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

    private async Task OnCardClickAsync(MouseEventArgs args)
    {
        if (!string.IsNullOrEmpty(Href))
            NavigationManager.NavigateTo(Href, forceLoad: OpenInNewTab);
        else if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(args);
    }

    private async Task OnMoreInfoClickAsync(MouseEventArgs args)
    {
        if (!string.IsNullOrEmpty(Href))
            NavigationManager.NavigateTo(Href, forceLoad: OpenInNewTab);
        else if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(args);
    }
}

