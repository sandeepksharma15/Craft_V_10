using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that iterates over a collection and renders content for each item.
/// Provides support for item context (item + index), empty state, and separators.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public partial class ForEach<T> : CraftComponent
{
    /// <summary>
    /// Gets or sets the collection of items to iterate over.
    /// </summary>
    [Parameter, EditorRequired] public IEnumerable<T>? Collection { get; set; }

    /// <summary>
    /// Gets or sets the template for rendering each item with its index.
    /// Provides access to both the item and its zero-based index in the collection.
    /// </summary>
    [Parameter] public RenderFragment<ItemContext>? ItemContent { get; set; }

    /// <summary>
    /// Gets or sets the template to render when the collection is null or empty.
    /// If not provided, nothing will be rendered for empty collections.
    /// </summary>
    [Parameter] public RenderFragment? Empty { get; set; }

    /// <summary>
    /// Gets or sets the template to render between items.
    /// This separator will not be rendered before the first item or after the last item.
    /// </summary>
    [Parameter] public RenderFragment? Separator { get; set; }

    /// <summary>
    /// Represents the context for each item in the collection, including the item itself and its index.
    /// </summary>
    /// <param name="Item">The current item from the collection.</param>
    /// <param name="Index">The zero-based index of the item in the collection.</param>
    public record ItemContext(T Item, int Index)
    {
        /// <summary>
        /// Gets a value indicating whether this is the first item (index == 0).
        /// </summary>
        public bool IsFirst => Index == 0;
    }
}
