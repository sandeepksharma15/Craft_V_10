using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec;

// Summary: JSON converter for SelectInfo<T, TResult>.
public class SelectDescriptorJsonConverter<T, TResult> : JsonConverter<SelectDescriptor<T, TResult>>
    where T : class
    where TResult : class
{
    // Summary: Reads JSON and converts it to a SelectInfo<T, TResult> instance.
    public override SelectDescriptor<T, TResult>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        SelectDescriptor<T, TResult> selectInfo = new();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                if (propertyName == nameof(SelectDescriptor<,>.Assignor))
                    selectInfo.Assignor = typeof(T).CreateMemberExpression(reader.GetString());

                if (propertyName == nameof(SelectDescriptor<,>.Assignee))
                    selectInfo.Assignee = typeof(TResult).CreateMemberExpression(reader.GetString());
            }
        }

        return selectInfo;
    }

    // Summary: Writes a SelectInfo<T, TResult> instance to JSON.
    public override void Write(Utf8JsonWriter writer, SelectDescriptor<T, TResult>? value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var assignor = value?.Assignor?.Body as MemberExpression;
        var assignee = value?.Assignee?.Body as MemberExpression;

        writer.WriteString(nameof(SelectDescriptor<,>.Assignor), assignor?.Member.Name);
        writer.WriteString(nameof(SelectDescriptor<,>.Assignee), assignee?.Member.Name);

        writer.WriteEndObject();
    }
}
