namespace Craft.Core;

public class PageInfo
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
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
