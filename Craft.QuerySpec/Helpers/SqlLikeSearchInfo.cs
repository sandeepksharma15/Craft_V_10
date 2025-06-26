using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec.Helpers;

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
    public LambdaExpression SearchItem { get; internal set; }
    public string SearchString { get; internal set; }
}

public class SqlLikeSearchInfoJsonConverter<T> : JsonConverter<SqlLikeSearchInfo<T>> where T : class
{
    public override SqlLikeSearchInfo<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        SqlLikeSearchInfo<T> searchInfo = new();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                if (propertyName == nameof(SqlLikeSearchInfo<>.SearchItem))
                    searchInfo.SearchItem = typeof(T).CreateMemberExpression(reader.GetString());

                if (propertyName == nameof(SqlLikeSearchInfo<>.SearchString))
                    searchInfo.SearchString = reader.GetString();

                if (propertyName == nameof(SqlLikeSearchInfo<>.SearchGroup))
                    searchInfo.SearchGroup = reader.GetInt32();
            }
        }

        return searchInfo;
    }

    public override void Write(Utf8JsonWriter writer, SqlLikeSearchInfo<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var memberName = value.SearchItem.GetPropertyInfo().Name;

        writer.WriteString(nameof(SqlLikeSearchInfo<>.SearchItem), memberName);
        writer.WriteString(nameof(SqlLikeSearchInfo<>.SearchString), value.SearchString);
        writer.WriteNumber(nameof(SqlLikeSearchInfo<>.SearchGroup), value.SearchGroup);

        writer.WriteEndObject();
    }
}
