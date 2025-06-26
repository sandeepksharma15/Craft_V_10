using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.Extensions.Expressions;
using Craft.QuerySpec.Enums;
using Craft.QuerySpec.Helpers;

namespace Craft.QuerySpec.Builders;

[Serializable]
public class EntityFilterBuilder<T> where T : class
{
    public EntityFilterBuilder() => EntityFilterList = [];

    public List<EntityFilterCriteria<T>> EntityFilterList { get; }

    public long Count => EntityFilterList.Count;

    public EntityFilterBuilder<T> Add(Expression<Func<T, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (expression.CanReduce)
            expression = (Expression<Func<T, bool>>)expression.ReduceAndCheck();

        EntityFilterList.Add(new EntityFilterCriteria<T>(expression));

        return this;
    }

    public EntityFilterBuilder<T> Add(Expression<Func<T, object>> propExpr, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        var expression = GetExpression(propExpr, compareWith, comparisonType);

        EntityFilterList.Add(new EntityFilterCriteria<T>(expression));

        return this;
    }

    public EntityFilterBuilder<T> Add(string propName, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        var expression = GetExpression(propName, compareWith, comparisonType);

        EntityFilterList.Add(new EntityFilterCriteria<T>(expression));

        return this;
    }

    public EntityFilterBuilder<T> Clear()
    {
        EntityFilterList.Clear();

        return this;
    }

    public EntityFilterBuilder<T> Remove(Expression<Func<T, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(nameof(expression));

        if (expression.CanReduce)
            expression = (Expression<Func<T, bool>>)expression.ReduceAndCheck();

        var comparer = new ExpressionSemanticEqualityComparer();

        var whereInfo = EntityFilterList.Find(x => comparer.Equals(x.Filter, expression));

        if (whereInfo is not null)
            EntityFilterList.Remove(whereInfo);

        return this;
    }

    public EntityFilterBuilder<T> Remove(Expression<Func<T, object>> propExpr, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        var expression = GetExpression(propExpr, compareWith, comparisonType);

        Remove(expression);

        return this;
    }

    public EntityFilterBuilder<T> Remove(string propName, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        var expression = GetExpression(propName, compareWith, comparisonType);

        Remove(expression);

        return this;
    }

    private static Expression<Func<T, bool>> GetExpression(Expression<Func<T, object>> propExpr, object compareWith, ComparisonType comparisonType)
    {
        ArgumentNullException.ThrowIfNull(nameof(propExpr));

        var filterInfo = FilterCriteria.GetFilterInfo(propExpr, compareWith, comparisonType);

        return ExpressionBuilder.CreateWhereExpression<T>(filterInfo);
    }

    private static Expression<Func<T, bool>> GetExpression(string propName, object compareWith, ComparisonType comparisonType)
    {
        // Check if the property exists
        var propExpr = ExpressionBuilder.GetPropertyExpression<T>(propName);
        var filterInfo = FilterCriteria.GetFilterInfo(propExpr, compareWith, comparisonType);

        return ExpressionBuilder.CreateWhereExpression<T>(filterInfo);
    }
}

public class EntityFilterBuilderJsonConverter<T> : JsonConverter<EntityFilterBuilder<T>> where T : class
{
    public override bool CanConvert(Type objectType)
        => objectType == typeof(EntityFilterBuilder<T>);

    public override EntityFilterBuilder<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var entityFilterBuilder = new EntityFilterBuilder<T>();

        // We Want To Clone The Options To Add The EntityFilterCriteriaJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new EntityFilterCriteriaJsonConverter<T>());

        // Check for array start
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Invalid format for SortOrderBuilder: expected array of OrderDescriptor");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            var entityFilterCriteria = JsonSerializer.Deserialize<EntityFilterCriteria<T>>(ref reader, localOptions);

            if (entityFilterCriteria != null)
                entityFilterBuilder.EntityFilterList.Add(entityFilterCriteria);
            else
                throw new JsonException("Invalid format for EntityFilterBuilder: expected array of EntityFilterCriteria");
        }

        return entityFilterBuilder;
    }

    public override void Write(Utf8JsonWriter writer, EntityFilterBuilder<T> value, JsonSerializerOptions options)
    {
        // Start The Array
        writer.WriteStartArray();

        // We Want To Clone The Options To Add The EntityFilterCriteriaJsonConverter
        var localOptions = options.GetClone();
        localOptions.Converters.Add(new EntityFilterCriteriaJsonConverter<T>());

        foreach (var entityFilter in value.EntityFilterList)
        {
            var json = JsonSerializer.Serialize(entityFilter, localOptions);
            writer.WriteRawValue(json);
        }

        // End the array
        writer.WriteEndArray();
    }
}
