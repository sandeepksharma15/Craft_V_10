#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Collections.Generic;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class CollectionExtensions
{
    /// <summary>
    /// Determines whether the specified collection is null or empty. Offers a convenient way to check for both conditions in a single expression, improving readability and reducing code verbosity.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to check for null or emptiness.</param>
    /// <returns>True if the collection is null or contains no elements, false otherwise.</returns>
    public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        => source == null || source.Count == 0;

    /// <summary>
    /// Adds the specified item to the collection if it does not already exist in the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to which the item will be added. Cannot be <see langword="null"/>.</param>
    /// <param name="item">The item to add to the collection.</param>
    /// <returns><see langword="true"/> if the item was added to the collection; otherwise, <see langword="false"/> if the item
    /// already exists in the collection.</returns>
    public static bool AddIfNotContains<T>(this ICollection<T> source, T item)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        if (source.Contains(item)) return false;

        source.Add(item);

        return true;
    }

    /// <summary>
    /// Adds the specified items to the collection if they are not already present.
    /// </summary>
    /// <remarks>This method iterates through the <paramref name="items"/> and adds each item to the <paramref
    /// name="source"/>  collection only if it is not already present. The method returns a collection of the items that
    /// were added,  allowing the caller to determine which items were newly included.</remarks>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to which items will be added. Cannot be <see langword="null"/>.</param>
    /// <param name="items">The items to add to the collection. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the items that were successfully added to the collection. If no items
    /// were added, the returned collection will be empty.</returns>
    public static IEnumerable<T> AddIfNotContains<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        List<T> addedItems = [];

        foreach (T item in items)
        {
            if (source.Contains(item))
                continue;

            source.Add(item);
            addedItems.Add(item);
        }

        return addedItems;
    }

    /// <summary>
    /// Adds an item to the collection if no existing item satisfies the specified predicate.
    /// </summary>
    /// <remarks>This method checks the collection for an existing item that satisfies the specified
    /// predicate. If no such item exists, it invokes the <paramref name="itemFactory"/> function to create a new item
    /// and adds it to the collection. The method returns <see langword="false"/> if an item already exists that
    /// satisfies the predicate, and no new item is added.</remarks>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to which the item will be added. Cannot be <see langword="null"/>.</param>
    /// <param name="predicate">A function to test each element for a condition. The item is added only if no element in the collection
    /// satisfies this predicate. Cannot be <see langword="null"/>.</param>
    /// <param name="itemFactory">A function that creates the item to be added to the collection. This function is invoked only if the predicate
    /// is not satisfied by any existing element. Cannot be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the item was added to the collection; otherwise, <see langword="false"/>.</returns>
    public static bool AddIfNotContains<T>(this ICollection<T> source, Func<T, bool> predicate, Func<T> itemFactory)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(itemFactory, nameof(itemFactory));

        if (source.Any(predicate)) return false;

        source.Add(itemFactory());

        return true;
    }

    /// <summary>
    /// Removes all elements in the specified collection from the source collection.
    /// </summary>
    /// <remarks>This method iterates through the <paramref name="items"/> collection and removes each element
    /// from the <paramref name="source"/> collection. If an element in <paramref name="items"/> does not exist in
    /// <paramref name="source"/>, it is ignored.</remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The collection from which elements will be removed. Cannot be <see langword="null"/>.</param>
    /// <param name="items">The collection of elements to remove from the source collection. Cannot be <see langword="null"/>.</param>
    public static void RemoveAll<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        foreach (var item in items)
            source.Remove(item);
    }
}
