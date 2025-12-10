namespace Craft.Expressions;

/// <summary>
/// Exception thrown when an expression cannot be evaluated against a type.
/// </summary>
public class ExpressionEvaluationException : Exception
{
    /// <summary>
    /// Gets the member path that could not be resolved.
    /// </summary>
    public string MemberPath { get; }

    /// <summary>
    /// Gets the type being evaluated against.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionEvaluationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="targetType">The type being evaluated against.</param>
    /// <param name="memberPath">The member path that could not be resolved.</param>
    public ExpressionEvaluationException(string message, Type targetType, string memberPath)
        : base($"{message} for type '{targetType.Name}': {memberPath}")
    {
        TargetType = targetType;
        MemberPath = memberPath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionEvaluationException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="targetType">The type being evaluated against.</param>
    /// <param name="memberPath">The member path that could not be resolved.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    public ExpressionEvaluationException(string message, Type targetType, string memberPath, Exception innerException)
        : base($"{message} for type '{targetType.Name}': {memberPath}", innerException)
    {
        TargetType = targetType;
        MemberPath = memberPath;
    }
}
