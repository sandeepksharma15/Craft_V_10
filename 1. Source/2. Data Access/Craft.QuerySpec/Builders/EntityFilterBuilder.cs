using System.Linq.Expressions;
using Craft.Extensions.Expressions;
using iText.Signatures.Validation.Lotl.Criteria;

namespace Craft.QuerySpec;

/// <summary>
/// Builder for creating and managing a list of entity filter criteria for type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// This class is NOT thread-safe. Do not share instances across threads.
/// </remarks>
/// <typeparam name="T">The entity type.</typeparam>
[Serializable]
public class EntityFilterBuilder<T> where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityFilterBuilder{T}"/> class.
    /// </summary>
    public EntityFilterBuilder() => EntityFilterList = [];

    /// <summary>
    /// Gets the list of filter criteria.
    /// </summary>
    public List<EntityFilterCriteria<T>> EntityFilterList { get; }

    /// <summary>
    /// Gets the number of filter criteria.
    /// </summary>
    public int Count => EntityFilterList.Count;

    /// <summary>
    /// Adds a filter criteria based on a boolean expression.
    /// </summary>
    /// <param name="expression">The filter expression.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="expression"/> is null.</exception>
    public EntityFilterBuilder<T> Add(Expression<Func<T, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (expression.CanReduce)
            expression = (Expression<Func<T, bool>>)expression.ReduceAndCheck();

        EntityFilterList.Add(new EntityFilterCriteria<T>(expression));

        return this;
    }

    /// <summary>
    /// Adds the specified filter criteria to the builder.
    /// </summary>
    /// <param name="entityFilterCriteria">The filter criteria to add. Cannot be null.</param>
    /// <returns>The current <see cref="EntityFilterBuilder{T}"/> instance with the added filter criteria.</returns>
    public EntityFilterBuilder<T> Add(EntityFilterCriteria<T> entityFilterCriteria)
    {
        ArgumentNullException.ThrowIfNull(entityFilterCriteria);

        EntityFilterList.Add(entityFilterCriteria);

        return this;
    }

    /// <summary>
    /// Adds a filter criteria based on a FilterCriteria object.
    /// </summary>
    /// <param name="criteria">The filter criteria containing property information, value, and comparison type.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="criteria"/> is null.</exception>
    public EntityFilterBuilder<T> Add(FilterCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var expression = FilterCriteria.GetExpression<T>(criteria);

        EntityFilterList.Add(new EntityFilterCriteria<T>(expression, criteria));

        return this;
    }

    /// <summary>
    /// Adds a filter criteria based on a property expression, value, and comparison type.
    /// </summary>
    /// <param name="propExpr">The property expression.</param>
    /// <param name="compareWith">The value to compare with.</param>
    /// <param name="comparisonType">The comparison type.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propExpr"/> is null.</exception>
    public EntityFilterBuilder<T> Add(Expression<Func<T, object>> propExpr, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        ArgumentNullException.ThrowIfNull(propExpr);

        var expression = GetExpression(propExpr, compareWith, comparisonType);

        EntityFilterList.Add(new EntityFilterCriteria<T>(expression));

        return this;
    }

    /// <summary>
    /// Adds a filter criteria based on a property name, value, and comparison type.
    /// </summary>
    /// <param name="propName">The property name.</param>
    /// <param name="compareWith">The value to compare with.</param>
    /// <param name="comparisonType">The comparison type.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="propName"/> is null or whitespace, or does not exist.</exception>
    public EntityFilterBuilder<T> Add(string propName, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propName);

        var expression = GetExpression(propName, compareWith, comparisonType);

        EntityFilterList.Add(new EntityFilterCriteria<T>(expression));

        return this;
    }

    /// <summary>
    /// Removes all filter criteria.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public EntityFilterBuilder<T> Clear()
    {
        EntityFilterList.Clear();
        return this;
    }

    /// <summary>
    /// Removes a filter criteria matching the given boolean expression.
    /// </summary>
    /// <param name="expression">The filter expression to remove.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="expression"/> is null.</exception>
    public EntityFilterBuilder<T> Remove(Expression<Func<T, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (expression.CanReduce)
            expression = (Expression<Func<T, bool>>)expression.ReduceAndCheck();

        var comparer = new ExpressionSemanticEqualityComparer();
        var toRemove = EntityFilterList
            .Find(x => comparer.Equals(x.Filter, expression));

        if (toRemove is not null)
            EntityFilterList.Remove(toRemove);

        return this;
    }

    /// <summary>
    /// Removes a filter criteria matching the given property expression, value, and comparison type.
    /// </summary>
    /// <param name="propExpr">The property expression.</param>
    /// <param name="compareWith">The value to compare with.</param>
    /// <param name="comparisonType">The comparison type.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propExpr"/> is null.</exception>
    public EntityFilterBuilder<T> Remove(Expression<Func<T, object>> propExpr, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        ArgumentNullException.ThrowIfNull(propExpr);

        var expression = GetExpression(propExpr, compareWith, comparisonType);

        return Remove(expression);
    }

    /// <summary>
    /// Removes a filter criteria matching the given property name, value, and comparison type.
    /// </summary>
    /// <param name="propName">The property name.</param>
    /// <param name="compareWith">The value to compare with.</param>
    /// <param name="comparisonType">The comparison type.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="propName"/> is null or whitespace, or does not exist.</exception>
    public EntityFilterBuilder<T> Remove(string propName, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propName);

        var expression = GetExpression(propName, compareWith, comparisonType);

        return Remove(expression);
    }

    /// <summary>
    /// Builds a filter expression from a property expression, value, and comparison type.
    /// </summary>
    private static Expression<Func<T, bool>> GetExpression(Expression<Func<T, object>> propExpr, object compareWith, ComparisonType comparisonType)
    {
        ArgumentNullException.ThrowIfNull(propExpr);

        var filterInfo = FilterCriteria.GetFilterInfo(propExpr, compareWith, comparisonType);

        return ExpressionBuilder.CreateWhereExpression<T>(filterInfo);
    }

    /// <summary>
    /// Builds a filter expression from a property name, value, and comparison type.
    /// </summary>
    private static Expression<Func<T, bool>> GetExpression(string propName, object compareWith, ComparisonType comparisonType)
    {
        var propExpr = ExpressionBuilder.GetPropertyExpression<T>(propName)
            ?? throw new ArgumentException($"Property '{propName}' does not exist on type '{typeof(T).Name}'.", nameof(propName));

        var filterInfo = FilterCriteria.GetFilterInfo(propExpr, compareWith, comparisonType);

        return ExpressionBuilder.CreateWhereExpression<T>(filterInfo);
    }
}
