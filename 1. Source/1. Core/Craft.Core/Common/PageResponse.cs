using System.Text.Json.Serialization;

namespace Craft.Core;

/// <summary>
/// Represents a paginated response containing a collection of items and pagination information.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
[Serializable]
public class PageResponse<T> : PageInfo where T : class
{
    /// <summary>
    /// The collection of items in the current page.
    /// </summary>
    public IEnumerable<T> Items { get; }

    [JsonConstructor]
    public PageResponse(IEnumerable<T> items, long totalCount, int currentPage, int pageSize)
    {
        Items = items ?? [];
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
