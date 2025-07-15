using System.Linq.Expressions;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// Represents information about a select operation, storing assignor and assignee LambdaExpressions.
/// </summary>
/// <typeparam name="T">The source type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
[Serializable]
public sealed class SelectDescriptor<T, TResult> where T : class where TResult : class
{
    /// Gets the assignor lambda expression (source property selector).
    public LambdaExpression? Assignor { get; private set; }

    /// Gets the assignee lambda expression (destination property selector).
    public LambdaExpression? Assignee { get; private set; }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectDescriptor{T, TResult}"/> with an assignor expression.
    /// </summary>
    /// <param name="assignor">The assignor lambda expression.</param>
    /// <exception cref="ArgumentNullException">Thrown if assignor is null.</exception>
    /// <exception cref="ArgumentException">Thrown if assignor is not a member expression.</exception>
    public SelectDescriptor(LambdaExpression assignor)
    {
        ArgumentNullException.ThrowIfNull(assignor, nameof(assignor));

        if (assignor.Body is not MemberExpression)
            throw new ArgumentException("Assignor must be a member expression (e.g., x => x.Property).", nameof(assignor));

        Assignor = assignor;

        if (typeof(TResult) != typeof(object))
            Assignee = GetAssignee(assignor);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectDescriptor{T, TResult}"/> with assignor and assignee expressions.
    /// </summary>
    /// <param name="assignor">The assignor lambda expression.</param>
    /// <param name="assignee">The assignee lambda expression.</param>
    /// <exception cref="ArgumentNullException">Thrown if assignor or assignee is null.</exception>
    /// <exception cref="ArgumentException">Thrown if TResult is T or object, or if assignor/assignee are not member expressions.</exception>
    public SelectDescriptor(LambdaExpression assignor, LambdaExpression assignee)
    {
        if (typeof(TResult) == typeof(T))
            throw new ArgumentException("You must call constructor without assignee if TResult is T");

        if (typeof(TResult) == typeof(object))
            throw new ArgumentException("You must call constructor without assignee if TResult is object");

        ArgumentNullException.ThrowIfNull(assignor, nameof(assignor));
        ArgumentNullException.ThrowIfNull(assignee, nameof(assignee));

        if (assignor.Body is not MemberExpression)
            throw new ArgumentException("Assignor must be a member expression (e.g., x => x.Property).", nameof(assignor));

        if (assignee.Body is not MemberExpression)
            throw new ArgumentException("Assignee must be a member expression (e.g., x => x.Property).", nameof(assignee));

        Assignor = assignor;
        Assignee = assignee;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectDescriptor{T, TResult}"/> with an assignor property name.
    /// </summary>
    /// <param name="assignorPropName">The assignor property name.</param>
    /// <exception cref="ArgumentNullException">Thrown if assignorPropName is null or whitespace.</exception>
    public SelectDescriptor(string assignorPropName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assignorPropName, nameof(assignorPropName));

        var assignor = assignorPropName.CreateMemberExpression<T>();

        Assignor = assignor;

        if (typeof(TResult) != typeof(object))
            Assignee = GetAssignee(assignor);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SelectDescriptor{T, TResult}"/> with assignor and assignee property names.
    /// </summary>
    /// <param name="assignorPropName">The assignor property name.</param>
    /// <param name="assigneePropName">The assignee property name.</param>
    /// <exception cref="ArgumentNullException">Thrown if any property name is null or whitespace.</exception>
    public SelectDescriptor(string assignorPropName, string assigneePropName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assignorPropName, nameof(assignorPropName));
        ArgumentException.ThrowIfNullOrWhiteSpace(assigneePropName, nameof(assigneePropName));  

        Assignor = assignorPropName.CreateMemberExpression<T>(); ;
        Assignee = assigneePropName.CreateMemberExpression<TResult>();
    }

    /// <summary>
    /// Internal parameterless constructor for serialization.
    /// </summary>
    internal SelectDescriptor() { }

    /// <summary>
    /// Gets the assignee lambda expression for a given assignor.
    /// </summary>
    /// <param name="assignor">The assignor lambda expression.</param>
    /// <returns>The assignee lambda expression.</returns>
    /// <exception cref="ArgumentNullException">Thrown if assignor is null.</exception>
    /// <exception cref="ArgumentException">Thrown if assignor is not a member expression or property does not exist on TResult.</exception>
    private static LambdaExpression GetAssignee(LambdaExpression assignor)
    {
        ArgumentNullException.ThrowIfNull(assignor, nameof(assignor));

        if (assignor.Body is not MemberExpression memberExpression)
            throw new ArgumentException("Assignor must be a member expression (e.g., x => x.Property).", nameof(assignor));

        var assignorPropName = memberExpression.Member.Name;

        _ = typeof(TResult).GetProperty(assignorPropName)
            ?? throw new ArgumentException($"Property '{assignorPropName}' does not exist on type '{typeof(TResult).Name}'. You should pass a lambda for the property if TResult is not T.");

        return assignorPropName.CreateMemberExpression<TResult>();
    }
}
