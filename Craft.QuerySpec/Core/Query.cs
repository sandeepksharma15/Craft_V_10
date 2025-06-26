using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.QuerySpec.Builders;
using Craft.QuerySpec.Contracts;
using Craft.QuerySpec.Evaluators;

namespace Craft.QuerySpec.Core;

// Represents a query with result projection.
[Serializable]
public class Query<T, TResult> : Query<T>, IQuery<T, TResult>
    where T : class
    where TResult : class
{
    // QuerySelectBuilder for constructing select expressions.
    public QuerySelectBuilder<T, TResult> QuerySelectBuilder { get; internal set; } = new();

    // Expression for selecting many results.
    public Expression<Func<T, IEnumerable<TResult>>>? SelectorMany { get; set; }

    // Clears the query specifications including select expressions and selector for many results.
    public new void Clear()
    {
        base.Clear();
        QuerySelectBuilder.Clear();
        SelectorMany = null;
    }

    // Function for post-processing results.
    public new Func<IEnumerable<TResult>, IEnumerable<TResult>> PostProcessingAction { get; set; }
}

[Serializable]
public class Query<T> : IQuery<T> where T : class
{
    // Common query specifications.
    public bool AsNoTracking { get; set; } = true;
    public bool AsSplitQuery { get; set; }
    public bool IgnoreAutoIncludes { get; set; } = true;
    public bool IgnoreQueryFilters { get; set; }

    // Pagination specifications.
    public int? Skip { get; set; }
    public int? Take { get; set; }

    // Builders for building where and order expressions.
    public SortOrderBuilder<T> SortOrderBuilder { get; internal set; } = new();
    public SqlLikeSearchCriteriaBuilder<T> SqlLikeSearchCriteriaBuilder { get; internal set; } = new();
    public EntityFilterBuilder<T> EntityFilterBuilder { get; internal set; } = new();

    // Function for post-processing results.
    public Func<IEnumerable<T>, IEnumerable<T>> PostProcessingAction { get; set; }

    // Checks if the entity satisfies the query specifications.
    public virtual bool IsSatisfiedBy(T entity)
    {
        // Create a queryable from the entity
        var queryable = new List<T> { entity }.AsQueryable();

        queryable = QueryEvaluator.Instance.GetQuery(queryable, this);

        return queryable.Any();
    }

    // Sets pagination specifications.
    public virtual void SetPage(int page = PaginationConstant.DefaultPage, int pageSize = PaginationConstant.DefaultPageSize)
    {
        pageSize = pageSize > 0 ? pageSize : PaginationConstant.DefaultPageSize;
        page = Math.Max(page, PaginationConstant.DefaultPage);
        Take = pageSize;
        Skip = (page - 1) * pageSize;
    }

    // Clears all query specifications.
    public void Clear()
    {
        // Reset pagination specifications.
        SetPage();

        // Reset common query specifications.
        AsNoTracking = true;
        AsSplitQuery = false;
        IgnoreAutoIncludes = true;
        IgnoreQueryFilters = false;

        // Clear Builders
        SortOrderBuilder.Clear();
        SqlLikeSearchCriteriaBuilder.Clear();
        EntityFilterBuilder.Clear();
    }
}

public interface IQueryJsonConverter<T> where T : class
{
    bool CanConvert(Type objectType);

    static Query<T> Read(ref Utf8JsonReader reader, JsonSerializerOptions options, Query<T> query)
    {
        // Start the object
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Invalid format for Query: expected start object");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Invalid format for Query: expected property name");

            var propertyName = reader.GetString();

            reader.Read();

            switch (propertyName)
            {
                case nameof(query.AsNoTracking):
                    query.AsNoTracking = reader.GetBoolean();
                    break;
                case nameof(query.AsSplitQuery):
                    query.AsSplitQuery = reader.GetBoolean();
                    break;
                case nameof(query.IgnoreAutoIncludes):
                    query.IgnoreAutoIncludes = reader.GetBoolean();
                    break;
                case nameof(query.IgnoreQueryFilters):
                    query.IgnoreQueryFilters = reader.GetBoolean();
                    break;
                case nameof(query.Skip):
                    query.Skip = reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();
                    break;
                case nameof(query.Take):
                    query.Take = reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();
                    break;
                case nameof(query.SortOrderBuilder):
                    query.SortOrderBuilder = JsonSerializer.Deserialize<SortOrderBuilder<T>>(ref reader, options);
                    break;
                case nameof(query.SqlLikeSearchCriteriaBuilder):
                    query.SqlLikeSearchCriteriaBuilder = JsonSerializer.Deserialize<SqlLikeSearchCriteriaBuilder<T>>(ref reader, options);
                    break;
                case nameof(query.EntityFilterBuilder):
                    query.EntityFilterBuilder = JsonSerializer.Deserialize<EntityFilterBuilder<T>>(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return query;
    }

    static void WriteCommonProperties(ref Utf8JsonWriter writer, IQuery<T> value, JsonSerializerOptions options)
    {
        writer.WriteBoolean(nameof(value.AsNoTracking), value.AsNoTracking);
        writer.WriteBoolean(nameof(value.AsSplitQuery), value.AsSplitQuery);
        writer.WriteBoolean(nameof(value.IgnoreAutoIncludes), value.IgnoreAutoIncludes);
        writer.WriteBoolean(nameof(value.IgnoreQueryFilters), value.IgnoreQueryFilters);

        writer.WritePropertyName(nameof(value.Skip));
        if (value.Skip != null)
            writer.WriteNumberValue(value.Skip.Value);
        else
            writer.WriteNullValue();

        writer.WritePropertyName("Take");
        if (value.Take != null)
            writer.WriteNumberValue(value.Take.Value);
        else
            writer.WriteNullValue();

        writer.WritePropertyName(nameof(value.SortOrderBuilder));
        JsonSerializer.Serialize(writer, value.SortOrderBuilder, options);

        writer.WritePropertyName(nameof(value.SqlLikeSearchCriteriaBuilder));
        JsonSerializer.Serialize(writer, value.SqlLikeSearchCriteriaBuilder, options);

        writer.WritePropertyName(nameof(value.EntityFilterBuilder));
        JsonSerializer.Serialize(writer, value.EntityFilterBuilder, options);
    }

    static JsonSerializerOptions GetLocalSerializerOptions(JsonSerializerOptions options)
    {
        var localOptions = options.GetClone();

        localOptions.Converters.Add(new SortOrderBuilderJsonConverter<T>());
        localOptions.Converters.Add(new SqlSearchCriteriaBuilderJsonConverter<T>());
        localOptions.Converters.Add(new EntityFilterBuilderJsonConverter<T>());

        return localOptions;
    }
}

public class QueryJsonConverter<T> : JsonConverter<Query<T>>, IQueryJsonConverter<T> where T : class
{
    public override bool CanConvert(Type objectType)
       => objectType == typeof(Query<T>);

    public override Query<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Create a new Query
        var query = new Query<T>();

        // Get Local Serializer Options
        var localOptions = IQueryJsonConverter<T>.GetLocalSerializerOptions(options);

        // Read Common Properties
        return IQueryJsonConverter<T>.Read(ref reader, localOptions, query);
    }

    public override void Write(Utf8JsonWriter writer, Query<T> value, JsonSerializerOptions options)
    {
        // Get Local Serializer Options
        var localOptions = IQueryJsonConverter<T>.GetLocalSerializerOptions(options);

        // Start the object
        writer.WriteStartObject();

        // Write Common Properties
        IQueryJsonConverter<T>.WriteCommonProperties(ref writer, value, localOptions);

        // End the object
        writer.WriteEndObject();
    }
}

public class QueryJsonConverter<T, TResult> : JsonConverter<Query<T, TResult>>, IQueryJsonConverter<T>
    where T : class
    where TResult : class
{
    public override bool CanConvert(Type objectType)
        => objectType == typeof(Query<T, TResult>);

    public override Query<T, TResult> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Create a new Query
        var query = new Query<T, TResult>();

        // Get Local Serializer Options 
        var localOptions = IQueryJsonConverter<T>.GetLocalSerializerOptions(options);
        localOptions.Converters.Add(new QuerySelectBuilderJsonConverter<T, TResult>());

        // Create A Clone Of Reader To Read Common Properties
        var readerClone = reader;

        // Read Common Properties
        query = (Query<T, TResult>)IQueryJsonConverter<T>.Read(ref readerClone, localOptions, query);

        // Read QuerySelectBuilder
        while (reader.Read())
        {
            // Read All Tokens Until We Find Property "QuerySelectBuilder"
            if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == nameof(query.QuerySelectBuilder))
            {
                reader.Read();
                query.QuerySelectBuilder = JsonSerializer.Deserialize<QuerySelectBuilder<T, TResult>>(ref reader, localOptions);
                break;
            }
        }

        // Read The Rest Of The Object
        while (reader.Read())
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

        return query;
    }

    public override void Write(Utf8JsonWriter writer, Query<T, TResult> value, JsonSerializerOptions options)
    {
        // Get Local Serializer Options 
        var localOptions = IQueryJsonConverter<T>.GetLocalSerializerOptions(options);
        localOptions.Converters.Add(new QuerySelectBuilderJsonConverter<T, TResult>());

        // Start the object
        writer.WriteStartObject();

        // Write Common Properties
        IQueryJsonConverter<T>.WriteCommonProperties(ref writer, value, localOptions);

        // Write QuerySelectBuilder
        writer.WritePropertyName(nameof(value.QuerySelectBuilder));
        JsonSerializer.Serialize(writer, value.QuerySelectBuilder, localOptions);

        // End the object
        writer.WriteEndObject();
    }
}
