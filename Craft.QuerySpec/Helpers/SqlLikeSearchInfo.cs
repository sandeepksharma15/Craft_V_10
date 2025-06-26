using System.Linq.Expressions;

namespace Craft.QuerySpec;

// Used For an SQL LIKE Search Functionality
[Serializable]
public class SqlLikeSearchInfo<T> where T : class
{
    public SqlLikeSearchInfo(LambdaExpression searchItem, string searchString, int searchGroup = 1)
    {
        ArgumentNullException.ThrowIfNull(searchItem);

        if (searchString.IsNullOrEmpty())
            throw new ArgumentException("{0} cannot be null or empty", nameof(searchString));

        SearchItem = searchItem;
        SearchString = searchString;
        SearchGroup = searchGroup;
    }

    public SqlLikeSearchInfo(Expression<Func<T, object>> searchItem, string searchString, int searchGroup = 1)
    {
        ArgumentNullException.ThrowIfNull(searchItem);

        if (searchString.IsNullOrEmpty())
            throw new ArgumentException("{0} cannot be null or empty", nameof(searchString));

        SearchItem = searchItem;
        SearchString = searchString;
        SearchGroup = searchGroup;
    }

    internal SqlLikeSearchInfo() { }

    public int SearchGroup { get; internal set; }
    public LambdaExpression? SearchItem { get; internal set; }
    public string? SearchString { get; internal set; }
}
