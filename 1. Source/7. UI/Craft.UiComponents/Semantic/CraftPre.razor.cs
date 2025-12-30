using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents an HTML pre element for preformatted text.
/// </summary>
/// <remarks>
/// The pre element displays text in a fixed-width font and preserves both spaces and line breaks.
/// Commonly used with code element for displaying code blocks.
/// </remarks>
/// <example>
/// <code>
/// &lt;CraftPre&gt;
///     Line 1
///     Line 2
///         Indented line
/// &lt;/CraftPre&gt;
/// 
/// &lt;CraftPre Content="@codeBlock" /&gt;
/// </code>
/// </example>
public partial class CraftPre : CraftComponent
{
    /// <summary>
    /// Gets or sets the preformatted content as a string.
    /// When set, this takes precedence over ChildContent.
    /// </summary>
    [Parameter] public string? Content { get; set; }

    /// <summary>
    /// Gets or sets whether to wrap long lines.
    /// </summary>
    [Parameter] public bool WrapLines { get; set; }

    /// <summary>
    /// Gets the component-specific CSS class.
    /// </summary>
    /// <returns>The CSS class "craft-pre" with optional wrap class.</returns>
    protected override string? GetComponentCssClass()
    {
        var baseClass = "craft-pre";
        return WrapLines ? $"{baseClass} craft-pre-wrap" : baseClass;
    }
}
