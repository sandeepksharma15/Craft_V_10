using Microsoft.EntityFrameworkCore;

namespace Craft.Extensions.Collections;

public static class QueriableExtensions
{
    /// <summary>
    /// Determines whether the specified queryable source supports asynchronous enumeration.
    /// </summary>
    /// <typeparam name="T">The type of elements in the queryable source.</typeparam>
    /// <param name="queryable">The queryable source to check for asynchronous support.</param>
    /// <returns><see langword="true"/> if the queryable source implements <see
    /// cref="System.Collections.Generic.IAsyncEnumerable{T}"/>; otherwise, <see langword="false"/>.</returns>
    public static bool SupportsAsync<T>(this IQueryable<T> queryable)
    {
        return queryable is IAsyncEnumerable<T>;
    }

    /// <summary>
    /// Asynchronously converts an <see cref="IQueryable{T}"/> to a <see cref="List{T}"/> in a safe manner.
    /// </summary>
    /// <remarks>This method checks if the source queryable supports asynchronous operations and uses <see
    /// cref="EntityFrameworkQueryableExtensions.ToListAsync{TSource}(IQueryable{TSource}, CancellationToken)"/> if
    /// available. If the queryable is in-memory, it falls back to a synchronous conversion using <see
    /// cref="Enumerable.ToList{TSource}(IEnumerable{TSource})"/>.</remarks>
    /// <typeparam name="T">The type of the elements in the source queryable.</typeparam>
    /// <param name="queryable">The source queryable to convert to a list.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of elements from the source
    /// queryable.</returns>
    public static async Task<List<T>> ToListSafeAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        // Defensive Code to avoid IndexOutOfRangeException
        if (!queryable.Any()) return [];

        if (queryable.SupportsAsync())
            // For async queryables, use ToListAsync
            return await queryable.ToListAsync(cancellationToken).ConfigureAwait(false);
        else
            // For in-memory queryables, use synchronous ToList
            return await Task.FromResult(queryable.ToList()).ConfigureAwait(false);
    }
}
