using Craft.Core;
using System.Text.Json;

namespace Craft.QuerySpec;

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
                case nameof(query.AutoIncludeNavigationProperties):
                    query.AutoIncludeNavigationProperties = reader.GetBoolean();
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
        writer.WriteBoolean(nameof(value.AutoIncludeNavigationProperties), value.AutoIncludeNavigationProperties);

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

