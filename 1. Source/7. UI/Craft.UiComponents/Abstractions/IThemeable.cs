using Craft.UiComponents.Enums;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Abstractions;

/// <summary>
/// Interface for components that support theming capabilities including size and variant styling.
/// </summary>
public interface IThemeable
{
    /// <summary>
    /// Gets or sets the size variant for this component.
    /// </summary>
    [Parameter]
    ComponentSize Size { get; set; }

    /// <summary>
    /// Gets or sets the visual style variant for this component.
    /// </summary>
    [Parameter]
    ComponentVariant Variant { get; set; }

    /// <summary>
    /// Gets the CSS class for the current size setting.
    /// </summary>
    /// <returns>The CSS class string for the size, or null if no size class should be applied.</returns>
    string? GetSizeCssClass() => Size switch
    {
        ComponentSize.ExtraSmall => "craft-size-xs",
        ComponentSize.Small => "craft-size-sm",
        ComponentSize.Medium => null,
        ComponentSize.Large => "craft-size-lg",
        ComponentSize.ExtraLarge => "craft-size-xl",
        _ => null
    };

    /// <summary>
    /// Gets the CSS class for the current variant setting.
    /// </summary>
    /// <returns>The CSS class string for the variant, or null if no variant class should be applied.</returns>
    string? GetVariantCssClass() => Variant switch
    {
        ComponentVariant.Default => null,
        ComponentVariant.Primary => "craft-variant-primary",
        ComponentVariant.Secondary => "craft-variant-secondary",
        ComponentVariant.Success => "craft-variant-success",
        ComponentVariant.Warning => "craft-variant-warning",
        ComponentVariant.Danger => "craft-variant-danger",
        ComponentVariant.Info => "craft-variant-info",
        ComponentVariant.Light => "craft-variant-light",
        ComponentVariant.Dark => "craft-variant-dark",
        ComponentVariant.Outlined => "craft-variant-outlined",
        ComponentVariant.Text => "craft-variant-text",
        _ => null
    };
}
