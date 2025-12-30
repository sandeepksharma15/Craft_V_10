using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents a dynamic HTML heading element (h1-h6).
/// </summary>
/// <remarks>
/// The CraftHeading component allows you to dynamically render heading elements from h1 to h6
/// based on the Level parameter. This is useful when the heading level needs to be determined at runtime.
/// Headings should be used to define the structure and hierarchy of content.
/// </remarks>
/// <example>
/// <code>
/// &lt;CraftHeading Level="1"&gt;Main Title&lt;/CraftHeading&gt;
/// &lt;CraftHeading Level="2"&gt;Subtitle&lt;/CraftHeading&gt;
/// &lt;CraftHeading Level="@GetDynamicLevel()"&gt;Dynamic Heading&lt;/CraftHeading&gt;
/// </code>
/// </example>
public partial class CraftHeading : CraftComponent
{
    private int _level = 2;

    /// <summary>
    /// Gets or sets the heading level (1-6). Defaults to 2 (h2).
    /// Values outside the range 1-6 will default to h2.
    /// </summary>
    [Parameter]
    public int Level
    {
        get => _level;
        set => _level = value is >= 1 and <= 6 ? value : 2;
    }

    /// <summary>
    /// Gets the component-specific CSS class.
    /// </summary>
    /// <returns>The CSS class "craft-heading" with level-specific class.</returns>
    protected override string? GetComponentCssClass() => $"craft-heading craft-heading-{Level}";
}
