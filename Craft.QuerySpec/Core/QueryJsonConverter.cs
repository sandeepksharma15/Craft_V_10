using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.QuerySpec.Contracts;

namespace Craft.QuerySpec;

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
