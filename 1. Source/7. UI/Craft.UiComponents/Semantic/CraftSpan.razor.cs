namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents an HTML span element, a generic inline container.
/// </summary>
/// <remarks>
/// The span element is used to group inline elements for styling purposes or to apply attributes.
/// It has no semantic meaning by itself and should be used when no other semantic element is appropriate.
/// </remarks>
/// <example>
/// <code>
/// &lt;CraftSpan Class="highlight"&gt;Important text&lt;/CraftSpan&gt;
/// &lt;CraftSpan Style="color: red;"&gt;Red text&lt;/CraftSpan&gt;
/// </code>
/// </example>
public partial class CraftSpan : CraftComponent
{
    /// <summary>
    /// Gets the component-specific CSS class.
    /// </summary>
    /// <returns>The CSS class "craft-span".</returns>
    protected override string? GetComponentCssClass() => "craft-span";
}
