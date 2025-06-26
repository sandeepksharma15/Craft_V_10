using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec.Helpers;

// Summary: Represents information about a select operation.
//          This class is designed to store assignor and assignee LambdaExpressions.
[Serializable]
public class SelectDescriptor<T, TResult>
    where T : class
    where TResult : class
{
    public SelectDescriptor(LambdaExpression assignor)
        => Initialize(assignor);

    public SelectDescriptor(LambdaExpression assignor, LambdaExpression assignee)
        => Initialize(assignor, assignee);

    public SelectDescriptor(string assignorPropName)
        => Initialize(assignorPropName.CreateMemberExpression<T>());

    public SelectDescriptor(string assignorPropName, string assigneePropName)
        => Initialize(assignorPropName.CreateMemberExpression<T>(), assigneePropName.CreateMemberExpression<TResult>());

    internal SelectDescriptor()
    { }

    public LambdaExpression Assignee { get; internal set; }
    public LambdaExpression Assignor { get; internal set; }

    private static LambdaExpression GetAssignee(LambdaExpression assignor)
    {
        var memberExpression = assignor.Body as MemberExpression;
        var assignorPropName = memberExpression.Member.Name;

        _ = typeof(TResult).GetProperty(assignorPropName)
            ?? throw new ArgumentException($"You should pass a lambda for the {assignorPropName} if TResult is not T");

        return assignorPropName.CreateMemberExpression<TResult>();
    }

    private void Initialize(LambdaExpression assignor)
    {
        ArgumentNullException.ThrowIfNull(assignor);

        Assignor = assignor;

        if (typeof(TResult) != typeof(object))
            Assignee = GetAssignee(assignor);
    }

    private void Initialize(LambdaExpression assignor, LambdaExpression assignee)
    {
        if (typeof(TResult) == typeof(T))
            throw new ArgumentException($"You must call constructor without {nameof(assignee)} if TResult is T");

        if (typeof(TResult) == typeof(object))
            throw new ArgumentException($"You must call constructor without {nameof(assignee)} if TResult is object");

        Assignor = assignor ?? throw new ArgumentException($"You must pass a lambda for the {nameof(assignor)}");
        Assignee = assignee ?? throw new ArgumentException($"You must pass a lambda for the {nameof(assignee)}");
    }
}

// Summary: JSON converter for SelectInfo<T, TResult>.
public class SelectDescriptorJsonConverter<T, TResult> : JsonConverter<SelectDescriptor<T, TResult>>
    where T : class
    where TResult : class
{
    // Summary: Reads JSON and converts it to a SelectInfo<T, TResult> instance.
    public override SelectDescriptor<T, TResult> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
    public override void Write(Utf8JsonWriter writer, SelectDescriptor<T, TResult> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var assignor = value.Assignor.Body as MemberExpression;
        var assignee = value.Assignee.Body as MemberExpression;

        writer.WriteString(nameof(SelectDescriptor<,>.Assignor), assignor.Member.Name);
        writer.WriteString(nameof(SelectDescriptor<,>.Assignee), assignee.Member.Name);

        writer.WriteEndObject();
    }
}
