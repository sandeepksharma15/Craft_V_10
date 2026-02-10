using Craft.Core;
using System.Linq.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// Represents an include expression for eagerly loading related entities.
/// </summary>
public class IncludeExpression
{
    /// <summary>
    /// Gets or sets the lambda expression that specifies the navigation property to include.
    /// </summary>
    public LambdaExpression Expression { get; set; }

    /// <summary>
    /// Gets or sets the type of the entity containing the navigation property.
    /// </summary>
    public Type EntityType { get; set; }

    /// <summary>
    /// Gets or sets the type of the navigation property.
    /// </summary>
    public Type PropertyType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a ThenInclude expression.
    /// </summary>
    public bool IsThenInclude { get; set; }

    /// <summary>
    /// Gets or sets the previous include expression (for ThenInclude chaining).
    /// </summary>
    public IncludeExpression? PreviousInclude { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IncludeExpression"/> class.
    /// </summary>
    /// <param name="expression">The lambda expression specifying the navigation property.</param>
    /// <param name="entityType">The type of the entity.</param>
    /// <param name="propertyType">The type of the navigation property.</param>
    /// <param name="isThenInclude">Whether this is a ThenInclude expression.</param>
    /// <param name="previousInclude">The previous include expression for chaining.</param>
    public IncludeExpression(
        LambdaExpression expression,
        Type entityType,
        Type propertyType,
        bool isThenInclude = false,
        IncludeExpression? previousInclude = null)
    {
        Expression = expression;
        EntityType = entityType;
        PropertyType = propertyType;
        IsThenInclude = isThenInclude;
        PreviousInclude = previousInclude;
    }
}

