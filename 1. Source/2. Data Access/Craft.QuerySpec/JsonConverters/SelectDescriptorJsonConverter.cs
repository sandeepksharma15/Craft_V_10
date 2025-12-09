using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// JSON converter for <see cref="SelectDescriptor{T, TResult}"/>.
/// </summary>
/// <typeparam name="T">The source type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public class SelectDescriptorJsonConverter<T, TResult> : JsonConverter<SelectDescriptor<T, TResult>>
    where T : class
    where TResult : class
{
    /// <inheritdoc />
    public override SelectDescriptor<T, TResult>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        // Use internal parameterless constructor for deserialization
        var ctor = typeof(SelectDescriptor<T, TResult>).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            Type.EmptyTypes,
            modifiers: null) ?? throw new InvalidOperationException($"No internal parameterless constructor found for {nameof(SelectDescriptor<,>)}.");

        var selectInfo = (SelectDescriptor<T, TResult>)ctor.Invoke(null);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                var value = reader.GetString();

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (propertyName == nameof(SelectDescriptor<,>.Assignor))
                {
                    var assignorExpr = value.CreateMemberExpression<T>();

                    SetProperty(selectInfo, nameof(SelectDescriptor<,>.Assignor), assignorExpr);
                }
                else if (propertyName == nameof(SelectDescriptor<,>.Assignee))
                {
                    var assigneeExpr = value.CreateMemberExpression<TResult>();

                    SetProperty(selectInfo, nameof(SelectDescriptor<,>.Assignee), assigneeExpr);
                }
            }
        }

        return selectInfo;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SelectDescriptor<T, TResult>? value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var assignor = value?.Assignor?.Body as MemberExpression;
        var assignee = value?.Assignee?.Body as MemberExpression;

        writer.WriteString(nameof(SelectDescriptor<,>.Assignor), assignor?.Member.Name);
        writer.WriteString(nameof(SelectDescriptor<,>.Assignee), assignee?.Member.Name);

        writer.WriteEndObject();
    }

    /// <summary>
    /// Sets a property value using reflection, including private/internal setters.
    /// </summary>
    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{obj.GetType().Name}'.");

        prop.SetValue(obj, value);
    }
}
