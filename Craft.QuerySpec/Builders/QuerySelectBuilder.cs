using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.QuerySpec.Contracts;
using Craft.QuerySpec.Helpers;

namespace Craft.QuerySpec.Builders;

[Serializable]
public class QuerySelectBuilder<T> : QuerySelectBuilder<T, T>, IQuerySelectBuilder<T> where T : class;

/// <summary>
/// A builder class for constructing select expressions.
/// </summary>
[Serializable]
public class QuerySelectBuilder<T, TResult> : IQuerySelectBuilder<T, TResult>
    where T : class
    where TResult : class
{
    public List<SelectDescriptor<T, TResult>> SelectDescriptorList { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySelectBuilder{T, TResult}"/> class.
    /// </summary>
    public QuerySelectBuilder() => SelectDescriptorList = [];

    /// <summary>
    /// Gets the count of select expressions.
    /// </summary>
    public long Count => SelectDescriptorList.Count;

    public QuerySelectBuilder<T, TResult> Add(Expression<Func<T, object>> assignor, Expression<Func<TResult, object>> assignee)
    {
        var assignorName = assignor.GetMemberName();
        var assigneeName = assignee.GetMemberName();

        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(assignorName, assigneeName));

        return this;
    }

    public QuerySelectBuilder<T, TResult> Add(SelectDescriptor<T, TResult> selectDescriptor)
    {
        SelectDescriptorList.Add(selectDescriptor);
        return this;
    }

    /// <summary>
    /// Adds a select expression.
    /// </summary>
    public QuerySelectBuilder<T, TResult> Add(LambdaExpression assignor, LambdaExpression assignee)
    {
        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(assignor, assignee));

        return this;
    }

    public QuerySelectBuilder<T, TResult> Add(Expression<Func<T, object>> column)
    {
        var columnName = column.GetMemberName();

        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(columnName));

        return this;
    }

    /// <summary>
    /// Adds a select expression with a single column.
    /// </summary>
    public QuerySelectBuilder<T, TResult> Add(LambdaExpression column)
    {
        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(column));

        return this;
    }

    /// <summary>
    /// Adds a select expression with a single column identified by its name.
    /// </summary>
    public QuerySelectBuilder<T, TResult> Add(string assignorPropName)
    {
        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(assignorPropName));

        return this;
    }

    /// <summary>
    /// Adds a select expression with a mapping between two properties.
    /// </summary>
    public QuerySelectBuilder<T, TResult> Add(string assignorPropName, string assigneePropName)
    {
        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(assignorPropName, assigneePropName));

        return this;
    }

    /// <summary>
    /// Builds the select expression.
    /// </summary>
    public Expression<Func<T, TResult>> Build()
        => typeof(TResult) == typeof(object) ? BuildAnnonymousSelect() : BuildSelect();

    /// <summary>
    /// Clears the select expressions.
    /// </summary>
    public void Clear()
        => SelectDescriptorList.Clear();

    private Expression<Func<T, TResult>> BuildAnnonymousSelect()
    {
        var sourceParam = Expression.Parameter(typeof(T), "x");

        var selectExpressions = SelectDescriptorList.Select(item =>
        {
            var columnInvoke = Expression.Invoke(item.Assignor, sourceParam);

            return Expression.Convert(columnInvoke, typeof(object));
        });

        var selectorBody = Expression.NewArrayInit(typeof(TResult), selectExpressions);

        return Expression.Lambda<Func<T, TResult>>(selectorBody, sourceParam);
    }

    private Expression<Func<T, TResult>> BuildSelect()
    {
        var selectParam = Expression.Parameter(typeof(T));
        var memberBindings = new List<MemberBinding>();

        foreach (var item in SelectDescriptorList)
        {
            var columnInvoke = Expression.Invoke(item.Assignor, selectParam);
            var propertyInfo = item.Assignee.GetPropertyInfo();
            var propertyType = propertyInfo.PropertyType;

            var convertedValue = Expression.Convert(columnInvoke, propertyType);

            var memberBinding = Expression.Bind(propertyInfo, convertedValue);
            memberBindings.Add(memberBinding);
        }

        var memberInit = Expression.MemberInit(Expression.New(typeof(TResult)), memberBindings);
        var selectorLambda = Expression.Lambda<Func<T, TResult>>(memberInit, selectParam);

        return Expression.Lambda<Func<T, TResult>>(selectorLambda.Body, selectParam);
    }
}

public class QuerySelectBuilderJsonConverter<T, TResult> : JsonConverter<QuerySelectBuilder<T, TResult>>
    where T : class
    where TResult : class
{
    public override bool CanConvert(Type objectType)
        => objectType == typeof(QuerySelectBuilder<T, TResult>);

    public override QuerySelectBuilder<T, TResult> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var querySelectBuilder = new QuerySelectBuilder<T, TResult>();

        // We Want To Clone The Options To Add The SelectDescriptorJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new SelectDescriptorJsonConverter<T, TResult>());

        // Check for array start
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Invalid format for QuerySelectBuilder: expected array of SelectDescriptor");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            // Read the individual Select Descriptor object
            var selectDescriptor = JsonSerializer.Deserialize<SelectDescriptor<T, TResult>>(ref reader, localOptions);

            // Validate and add the SelectDescriptor
            if (selectDescriptor != null)
                querySelectBuilder.Add(selectDescriptor);
            else
                throw new JsonException("Invalid select descriptor encountered in QuerySelectBuilder array");
        }

        return querySelectBuilder;
    }

    public override void Write(Utf8JsonWriter writer, QuerySelectBuilder<T, TResult> value, JsonSerializerOptions options)
    {
        // Start The Array
        writer.WriteStartArray();

        // We Want To Clone The Options To Add The SelectDescriptorJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new SelectDescriptorJsonConverter<T, TResult>());

        foreach (var selectDescriptor in value.SelectDescriptorList)
        {
            var json = JsonSerializer.Serialize(selectDescriptor, localOptions);
            writer.WriteRawValue(json);
        }

        // End the array
        writer.WriteEndArray();
    }
}
