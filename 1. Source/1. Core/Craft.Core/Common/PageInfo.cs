namespace Craft.Core;

/// <summary>
/// Represents pagination information for a paged result set.
/// </summary>
public class PageInfo
{
    private int _currentPage = 1;
    private int _pageSize = 10;

    /// <summary>
    /// Gets or sets the current page number (1-based). Defaults to 1.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is less than 1.</exception>
    public int CurrentPage
    {
        get => _currentPage;
        set => _currentPage = value < 1
            ? throw new ArgumentOutOfRangeException(nameof(CurrentPage), "Current page must be at least 1.")
            : value;
    }

    /// <summary>
    /// Gets or sets the number of items per page. Defaults to 10.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is less than 1.</exception>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1
            ? throw new ArgumentOutOfRangeException(nameof(PageSize), "Page size must be at least 1.")
            : value;
    }

    /// <summary>
    /// Gets or sets the total number of items across all pages.
    /// </summary>
    public long TotalCount { get; set; } = 0;

    /// <summary>
    /// Calculates the starting index of the current page (1-based indexing).
    /// </summary>
    public int From => ((CurrentPage - 1) * PageSize) + 1;

    /// <summary>
    /// Indicates whether there is a next page available.
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    /// <summary>
    /// Indicates whether there is a previous page available.
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Calculates the ending index for the current page, considering available items.
    /// </summary>
    public int To => (int)(From + Math.Min(PageSize - 1, TotalCount - From));

    /// <summary>
    /// Calculates the total number of pages based on the total count and page size.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
