using System.Text.Json;
using System.Text.Json.Serialization;

namespace Craft.QuerySpec;

/// <summary>
/// JSON converter for <see cref="Query{T}"/>. Handles serialization and deserialization of query specifications.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class QueryJsonConverter<T> : JsonConverter<Query<T>>, IQueryJsonConverter<T> where T : class
{
    /// <inheritdoc/>
    public override Query<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        try
        {
            var query = new Query<T>();
            var localOptions = IQueryJsonConverter<T>.GetLocalSerializerOptions(options);

            return IQueryJsonConverter<T>.Read(ref reader, localOptions, query);
        }
        catch (Exception ex)
        {
            throw new JsonException($"Error deserializing Query<{typeof(T).Name}>.", ex);
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Query<T> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);
        try
        {
            var localOptions = IQueryJsonConverter<T>.GetLocalSerializerOptions(options);

            writer.WriteStartObject();

            IQueryJsonConverter<T>.WriteCommonProperties(ref writer, value, localOptions);

            writer.WriteEndObject();
        }
        catch (Exception ex)
        {
            throw new JsonException($"Error serializing Query<{typeof(T).Name}>.", ex);
        }
    }
}

/// <summary>
/// JSON converter for <see cref="Query{T, TResult}"/>. Handles serialization and deserialization of query specifications with projection.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public class QueryJsonConverter<T, TResult> : JsonConverter<Query<T, TResult>>, IQueryJsonConverter<T>
    where T : class
    where TResult : class
{
    /// <inheritdoc/>
    public override Query<T, TResult> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        try
        {
            var query = new Query<T, TResult>();
            var localOptions = IQueryJsonConverter<T>.GetLocalSerializerOptions(options);

            localOptions.Converters.Add(new QuerySelectBuilderJsonConverter<T, TResult>());

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Expected StartObject token, but got {reader.TokenType}.");

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();

                    if (propertyName == nameof(query.QuerySelectBuilder))
                        query.QuerySelectBuilder = JsonSerializer.Deserialize<QuerySelectBuilder<T, TResult>>(ref reader, localOptions);
                    else
                    {
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
                                query.SortOrderBuilder = JsonSerializer.Deserialize<SortOrderBuilder<T>>(ref reader, localOptions);
                                break;
                            case nameof(query.SqlLikeSearchCriteriaBuilder):
                                query.SqlLikeSearchCriteriaBuilder = JsonSerializer.Deserialize<SqlLikeSearchCriteriaBuilder<T>>(ref reader, localOptions);
                                break;
                            case nameof(query.EntityFilterBuilder):
                                query.EntityFilterBuilder = JsonSerializer.Deserialize<EntityFilterBuilder<T>>(ref reader, localOptions);
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                }
            }

            return query;
        }
        catch (Exception ex)
        {
            throw new JsonException($"Error deserializing Query<{typeof(T).Name}, {typeof(TResult).Name}>.", ex);
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Query<T, TResult> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);
        try
        {
            var localOptions = IQueryJsonConverter<T>.GetLocalSerializerOptions(options);

            localOptions.Converters.Add(new QuerySelectBuilderJsonConverter<T, TResult>());

            writer.WriteStartObject();

            IQueryJsonConverter<T>.WriteCommonProperties(ref writer, value, localOptions);

            if (value.QuerySelectBuilder is not null)
            {
                writer.WritePropertyName(nameof(value.QuerySelectBuilder));
                JsonSerializer.Serialize(writer, value.QuerySelectBuilder, localOptions);
            }
            writer.WriteEndObject();
        }
        catch (Exception ex)
        {
            throw new JsonException($"Error serializing Query<{typeof(T).Name}, {typeof(TResult).Name}>.", ex);
        }
    }
}
