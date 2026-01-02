using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that displays its content only when a collection is empty or null.
/// Useful for showing empty state messages.
/// </summary>
/// <typeparam name="TItem">The type of items in the collection.</typeparam>
public partial class Empty<TItem> : CraftComponent
{
    /// <summary>
    /// Gets or sets the collection to check for emptiness.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }
}
