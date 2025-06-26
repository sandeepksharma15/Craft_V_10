using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.Extensions.Expressions;
using Craft.QuerySpec.Helpers;

namespace Craft.QuerySpec.Builders;

public class SqlLikeSearchCriteriaBuilder<T> where T : class
{
    public List<SqlLikeSearchInfo<T>> SqlLikeSearchCriteriaList { get; }

    public SqlLikeSearchCriteriaBuilder() => SqlLikeSearchCriteriaList = [];

    public long Count => SqlLikeSearchCriteriaList.Count;

    public SqlLikeSearchCriteriaBuilder<T> Add(SqlLikeSearchInfo<T> searchInfo)
    {
        ArgumentNullException.ThrowIfNull(searchInfo);

        SqlLikeSearchCriteriaList.Add(searchInfo);
        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Add(Expression<Func<T, object>> member, string searchString, int searchGroup = 1)
    {
        SqlLikeSearchCriteriaList.Add(new SqlLikeSearchInfo<T>(member, searchString, searchGroup));
        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Add(string memberName, string searchString, int searchGroup = 1)
    {
        var member = ExpressionBuilder.GetPropertyExpression<T>(memberName);
        SqlLikeSearchCriteriaList.Add(new SqlLikeSearchInfo<T>(member, searchString, searchGroup));
        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Clear()
    {
        SqlLikeSearchCriteriaList.Clear();
        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Remove(SqlLikeSearchInfo<T> searchInfo)
    {
        ArgumentNullException.ThrowIfNull(searchInfo);

        SqlLikeSearchCriteriaList.Remove(searchInfo);
        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Remove(Expression<Func<T, object>> member)
    {
        ArgumentNullException.ThrowIfNull(member);

        var comparer = new ExpressionSemanticEqualityComparer();
        var searchInfo = SqlLikeSearchCriteriaList.Find(x => comparer.Equals(x.SearchItem, member));

        if (searchInfo != null)
            SqlLikeSearchCriteriaList.Remove(searchInfo);

        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Remove(string memberName)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(memberName));

        Remove(ExpressionBuilder.GetPropertyExpression<T>(memberName));

        return this;
    }
}

public class SqlSearchCriteriaBuilderJsonConverter<T> : JsonConverter<SqlLikeSearchCriteriaBuilder<T>> where T : class
{
    public override bool CanConvert(Type objectType)
        => objectType == typeof(SqlLikeSearchCriteriaBuilder<T>);

    public override SqlLikeSearchCriteriaBuilder<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var searchBuilder = new SqlLikeSearchCriteriaBuilder<T>();

        // We Want To Clone The Options To Add The SqlLikeSearchInfoJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new SqlLikeSearchInfoJsonConverter<T>());

        // Check for array start
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Invalid format for SqlSearchCriteriaBuilder: expected array of SqlLikeSearchInfo");

        // Read each order expression
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            // Read the individual SqlLikeSearchInfo object
            var searchInfo = JsonSerializer.Deserialize<SqlLikeSearchInfo<T>>(ref reader, localOptions);

            // Validate and add the order expression
            if (searchInfo != null)
                searchBuilder.Add(searchInfo);
            else
                throw new JsonException("Invalid SqlLikeSearchInfo encountered in SqlSearchCriteriaBuilder array");
        }

        return searchBuilder;
    }

    public override void Write(Utf8JsonWriter writer, SqlLikeSearchCriteriaBuilder<T> value, JsonSerializerOptions options)
    {
        // Start The Array
        writer.WriteStartArray();

        // We Want To Clone The Options To Add The SqlLikeSearchInfoJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new SqlLikeSearchInfoJsonConverter<T>());

        foreach (var searchInfo in value.SqlLikeSearchCriteriaList)
        {
            var json = JsonSerializer.Serialize(searchInfo, localOptions);
            writer.WriteRawValue(json);
        }

        // End the array
        writer.WriteEndArray();
    }
}
