using System.Linq.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// Represents information for an SQL LIKE search functionality.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
[Serializable]
public sealed class SqlLikeSearchInfo<T> where T : class
{
    /// Gets the group number for the search (useful for grouping multiple LIKE conditions).
    public int SearchGroup { get; internal set; }

    /// Gets the lambda expression representing the property to search.
    public LambdaExpression? SearchItem { get; internal set; }

    /// Gets the search string for the LIKE operation.
    public string? SearchString { get; internal set; }

    /// Initializes a new instance of <see cref="SqlLikeSearchInfo{T}"/>.
    public SqlLikeSearchInfo(Expression<Func<T, object>> searchItem, string searchString, int searchGroup = 1)
    {
        ArgumentNullException.ThrowIfNull(searchItem, nameof(searchItem));
        ArgumentException.ThrowIfNullOrWhiteSpace(searchString, nameof(searchString));

        SearchItem = searchItem;
        SearchString = searchString;
        SearchGroup = searchGroup;
    }

    /// Initializes a new instance of <see cref="SqlLikeSearchInfo{T}"/>.
    public SqlLikeSearchInfo(LambdaExpression searchItem, string searchString, int searchGroup = 1)
    {
        ArgumentNullException.ThrowIfNull(searchItem, nameof(searchItem));
        ArgumentException.ThrowIfNullOrWhiteSpace(searchString, nameof(searchString));

        SearchItem = searchItem;
        SearchString = searchString;
        SearchGroup = searchGroup;
    }

    /// Internal parameterless constructor for serialization.
    internal SqlLikeSearchInfo() { }
}
