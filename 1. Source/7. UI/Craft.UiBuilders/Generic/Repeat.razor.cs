using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that renders a collection of items with support for item templates and empty state.
/// Simpler alternative to <see cref="ForEach{T}"/> when you don't need index tracking or separators.
/// </summary>
/// <typeparam name="TItem">The type of items in the collection.</typeparam>
public partial class Repeat<TItem> : CraftComponent
{
    /// <summary>
    /// Gets or sets the collection of items to render.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// Gets or sets the template for rendering each item.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the content to display when the collection is empty or null.
    /// </summary>
    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }
}
