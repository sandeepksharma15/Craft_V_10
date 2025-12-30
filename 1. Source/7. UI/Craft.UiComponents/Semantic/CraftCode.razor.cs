using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents an HTML code element for displaying inline code or computer code.
/// </summary>
/// <remarks>
/// The code element is used to display a fragment of computer code.
/// By default, it is displayed in a monospace font. Use with pre element for multi-line code blocks.
/// </remarks>
/// <example>
/// <code>
/// &lt;CraftCode&gt;var x = 10;&lt;/CraftCode&gt;
/// &lt;CraftCode Content="Console.WriteLine();" Language="csharp" /&gt;
/// </code>
/// </example>
public partial class CraftCode : CraftComponent
{
    /// <summary>
    /// Gets or sets the code content as a string.
    /// When set, this takes precedence over ChildContent.
    /// </summary>
    [Parameter] public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the programming language for syntax highlighting (used by CSS classes).
    /// </summary>
    [Parameter] public string? Language { get; set; }

    /// <summary>
    /// Gets the component-specific CSS class.
    /// </summary>
    /// <returns>The CSS class "craft-code" with optional language class.</returns>
    protected override string? GetComponentCssClass()
    {
        var baseClass = "craft-code";
        return string.IsNullOrEmpty(Language) ? baseClass : $"{baseClass} language-{Language}";
    }
}
