using Craft.UiComponents.Enums;
using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Abstractions;

/// <summary>
/// Interface for components that support animation capabilities.
/// </summary>
public interface IAnimatable
{
    /// <summary>
    /// Gets or sets the animation type for this component.
    /// </summary>
    [Parameter]
    AnimationType Animation { get; set; }

    /// <summary>
    /// Gets or sets the animation duration.
    /// </summary>
    [Parameter]
    AnimationDuration AnimationDuration { get; set; }

    /// <summary>
    /// Gets or sets a custom animation duration in milliseconds.
    /// Overrides <see cref="AnimationDuration"/> when set to a positive value.
    /// </summary>
    [Parameter]
    int? CustomAnimationDurationMs { get; set; }

    /// <summary>
    /// Gets the CSS class for the current animation setting.
    /// </summary>
    /// <returns>The CSS class string for the animation, or null if no animation class should be applied.</returns>
    string? GetAnimationCssClass() => Animation switch
    {
        AnimationType.None => null,
        AnimationType.Fade => "craft-animate-fade",
        AnimationType.Slide => "craft-animate-slide",
        AnimationType.Scale => "craft-animate-scale",
        AnimationType.Collapse => "craft-animate-collapse",
        AnimationType.Bounce => "craft-animate-bounce",
        AnimationType.Shake => "craft-animate-shake",
        AnimationType.Pulse => "craft-animate-pulse",
        AnimationType.Flip => "craft-animate-flip",
        AnimationType.Rotate => "craft-animate-rotate",
        _ => null
    };

    /// <summary>
    /// Gets the inline style string for animation duration.
    /// </summary>
    /// <returns>The style string for animation duration, or null if no animation is set.</returns>
    string? GetAnimationStyle()
    {
        if (Animation == AnimationType.None)
            return null;

        var duration = CustomAnimationDurationMs ?? (int)AnimationDuration;
        return duration > 0 ? $"--craft-animation-duration: {duration}ms" : null;
    }
}
