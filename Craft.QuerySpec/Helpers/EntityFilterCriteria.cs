using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Craft.Extensions.System;
using Craft.QuerySpec.Builders;

namespace Craft.QuerySpec.Helpers;

/// <summary>
/// Represents a specification for filtering a collection of entities of type `T`.
/// Encapsulates a compiled predicate function for efficient evaluation.
/// </summary>
/// <typeparam name="T">The type of the entities to be filtered.</typeparam>
[Serializable]
public class EntityFilterCriteria<T> where T : class
{
    private readonly Lazy<Func<T, bool>> filterFunc;

    public EntityFilterCriteria(Expression<Func<T, bool>> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        Filter = filter;
        filterFunc = new Lazy<Func<T, bool>>(Filter.Compile);
    }

    public Expression<Func<T, bool>> Filter { get; }
    public Func<T, bool> FilterFunc => filterFunc.Value;

    /// <summary>
    /// Checks whether the specified entity matches the filter criteria.
    /// </summary>
    /// <param name="entity">The entity to be evaluated.</param>
    /// <returns>True if the entity matches the filter, false otherwise.</returns>
    public bool Matches(T entity) => FilterFunc.Invoke(entity);
}

/// <summary>
/// A custom JSON converter for the `EntityFilterCriteria<T>` class, enabling serialization and deserialization of filter expressions for entity filtering.
/// </summary>
/// <typeparam name="T">The type of the entities being filtered.</typeparam>
public class EntityFilterCriteriaJsonConverter<T> : JsonConverter<EntityFilterCriteria<T>> where T : class
{
    public override EntityFilterCriteria<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Expression<Func<T, bool>> filter = null;

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                if (propertyName == nameof(EntityFilterCriteria<>.Filter))
                {
                    var str = reader.GetString();

                    str = str.Replace('\'', '\"');
                    str = str.RemovePreFix("(");
                    str = str.RemovePostFix(")");

                    filter = ExpressionTreeBuilder.BuildBinaryTreeExpression<T>(str);
                }
                else
                {
                    throw new JsonException($"Expected a valid property value instead of {reader.GetString()}");
                }
            }
        }

        return new EntityFilterCriteria<T>(filter);
    }

    public override void Write(Utf8JsonWriter writer, EntityFilterCriteria<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(EntityFilterCriteria<>.Filter),
            RemoveAccessor(value.Filter.Body.ToString()));

        writer.WriteEndObject();
    }

    private static string RemoveAccessor(string source)
    {
        var result =  Regex.Replace(source, @"\((\w+)\.", "(");

        return result;
    }
}
