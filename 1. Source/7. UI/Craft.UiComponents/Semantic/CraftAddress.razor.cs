namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents an HTML address element for contact information.
/// The address element should be used for contact information related to the author or owner of the document or article.
/// </summary>
/// <remarks>
/// The address element provides semantic meaning for contact information such as email addresses,
/// physical addresses, phone numbers, social media links, etc.
/// </remarks>
/// <example>
/// <code>
/// &lt;CraftAddress&gt;
///     Contact us at:&lt;br/&gt;
///     &lt;a href="mailto:info@example.com"&gt;info@example.com&lt;/a&gt;&lt;br/&gt;
///     123 Main Street&lt;br/&gt;
///     City, State 12345
/// &lt;/CraftAddress&gt;
/// </code>
/// </example>
public partial class CraftAddress : CraftComponent
{
    /// <summary>
    /// Gets the component-specific CSS class.
    /// </summary>
    /// <returns>The CSS class "craft-address".</returns>
    protected override string? GetComponentCssClass() => "craft-address";
}
