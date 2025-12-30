namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents an HTML paragraph element.
/// </summary>
/// <remarks>
/// The p element represents a paragraph of text. Paragraphs are block-level elements
/// and browsers automatically add space before and after each paragraph.
/// </remarks>
/// <example>
/// <code>
/// &lt;CraftP&gt;
///     This is a paragraph of text. It can contain multiple sentences
///     and will be displayed as a block-level element.
/// &lt;/CraftP&gt;
/// </code>
/// </example>
public partial class CraftP : CraftComponent
{
    /// <summary>
    /// Gets the component-specific CSS class.
    /// </summary>
    /// <returns>The CSS class "craft-p".</returns>
    protected override string? GetComponentCssClass() => "craft-p";
}
