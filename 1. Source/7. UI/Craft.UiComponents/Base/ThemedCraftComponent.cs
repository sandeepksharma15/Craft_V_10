using Craft.UiComponents.Abstractions;
using Craft.UiComponents.Enums;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents;

/// <summary>
/// Extended base component that includes theming and animation support.
/// For components that need both theming and animation capabilities.
/// </summary>
public abstract class ThemedCraftComponent : CraftComponent, IThemeable, IAnimatable
{
    #region IThemeable Implementation

    /// <inheritdoc />
    [Parameter] public ComponentSize Size { get; set; } = ComponentSize.Medium;

    /// <inheritdoc />
    [Parameter] public ComponentVariant Variant { get; set; } = ComponentVariant.Default;

    #endregion

    #region IAnimatable Implementation

    /// <inheritdoc />
    [Parameter] public AnimationType Animation { get; set; } = AnimationType.None;

    /// <inheritdoc />
    [Parameter] public AnimationDuration AnimationDuration { get; set; } = AnimationDuration.Normal;

    /// <inheritdoc />
    [Parameter] public int? CustomAnimationDurationMs { get; set; }

    #endregion

    #region Overrides

    /// <inheritdoc />
    protected override string BuildCssClass()
    {
        var baseClass = base.BuildCssClass();
        var builder = new Utilities.Builders.CssBuilder(baseClass);

        var sizeClass = ((IThemeable)this).GetSizeCssClass();
        if (!string.IsNullOrEmpty(sizeClass))
            builder.AddClass(sizeClass);

        var variantClass = ((IThemeable)this).GetVariantCssClass();
        if (!string.IsNullOrEmpty(variantClass))
            builder.AddClass(variantClass);

        var animationClass = ((IAnimatable)this).GetAnimationCssClass();
        if (!string.IsNullOrEmpty(animationClass))
            builder.AddClass(animationClass);

        return builder.Build();
    }

    /// <inheritdoc />
    protected override string? BuildStyle()
    {
        var baseStyle = base.BuildStyle();
        var animationStyle = ((IAnimatable)this).GetAnimationStyle();

        if (string.IsNullOrEmpty(baseStyle) && string.IsNullOrEmpty(animationStyle))
            return null;

        if (string.IsNullOrEmpty(baseStyle))
            return animationStyle;

        if (string.IsNullOrEmpty(animationStyle))
            return baseStyle;

        return $"{baseStyle}; {animationStyle}";
    }

    #endregion
}
