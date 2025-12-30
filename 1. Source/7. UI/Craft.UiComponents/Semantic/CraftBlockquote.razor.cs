using Microsoft.AspNetCore.Components;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents an HTML blockquote element for extended quotations.
/// </summary>
/// <remarks>
/// The blockquote element should be used for block-level quotations from external sources.
/// Use the cite attribute to provide the source URL of the quotation.
/// </remarks>
/// <example>
/// <code>
/// &lt;CraftBlockquote Cite="https://www.example.com/source"&gt;
///     This is a quotation from an external source.
///     It can span multiple lines and paragraphs.
/// &lt;/CraftBlockquote&gt;
/// </code>
/// </example>
public partial class CraftBlockquote : CraftComponent
{
    /// <summary>
    /// Gets or sets the URL of the source of the quotation.
    /// </summary>
    [Parameter] public string? Cite { get; set; }

    /// <summary>
    /// Gets the component-specific CSS class.
    /// </summary>
    /// <returns>The CSS class "craft-blockquote".</returns>
    protected override string? GetComponentCssClass() => "craft-blockquote";
}
