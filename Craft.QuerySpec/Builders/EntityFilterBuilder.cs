using System.Linq.Expressions;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec;

[Serializable]
public class EntityFilterBuilder<T> where T : class
{
    public EntityFilterBuilder() => EntityFilterList = [];

    public List<EntityFilterCriteria<T>> EntityFilterList { get; }

    public long Count => EntityFilterList.Count;

    public EntityFilterBuilder<T> Add(Expression<Func<T, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (expression.CanReduce)
            expression = (Expression<Func<T, bool>>)expression.ReduceAndCheck();

        EntityFilterList.Add(new EntityFilterCriteria<T>(expression));

        return this;
    }

    public EntityFilterBuilder<T> Add(Expression<Func<T, object>> propExpr, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        var expression = GetExpression(propExpr, compareWith, comparisonType);

        EntityFilterList.Add(new EntityFilterCriteria<T>(expression));

        return this;
    }

    public EntityFilterBuilder<T> Add(string propName, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        var expression = GetExpression(propName, compareWith, comparisonType);

        EntityFilterList.Add(new EntityFilterCriteria<T>(expression));

        return this;
    }

    public EntityFilterBuilder<T> Clear()
    {
        EntityFilterList.Clear();

        return this;
    }

    public EntityFilterBuilder<T> Remove(Expression<Func<T, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        if (expression.CanReduce)
            expression = (Expression<Func<T, bool>>)expression.ReduceAndCheck();

        var comparer = new ExpressionSemanticEqualityComparer();

        var whereInfo = EntityFilterList.Find(x => comparer.Equals(x.Filter, expression));

        if (whereInfo is not null)
            EntityFilterList.Remove(whereInfo);

        return this;
    }

    public EntityFilterBuilder<T> Remove(Expression<Func<T, object>> propExpr, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        var expression = GetExpression(propExpr, compareWith, comparisonType);

        Remove(expression);

        return this;
    }

    public EntityFilterBuilder<T> Remove(string propName, object compareWith, ComparisonType comparisonType = ComparisonType.EqualTo)
    {
        var expression = GetExpression(propName, compareWith, comparisonType);

        Remove(expression);

        return this;
    }

    private static Expression<Func<T, bool>> GetExpression(Expression<Func<T, object>> propExpr, object compareWith, ComparisonType comparisonType)
    {
        ArgumentNullException.ThrowIfNull(propExpr, nameof(propExpr));

        var filterInfo = FilterCriteria.GetFilterInfo(propExpr, compareWith, comparisonType);

        return ExpressionBuilder.CreateWhereExpression<T>(filterInfo);
    }

    private static Expression<Func<T, bool>> GetExpression(string propName, object compareWith, ComparisonType comparisonType)
    {
        // Check if the property exists
        var propExpr = ExpressionBuilder.GetPropertyExpression<T>(propName);

        ArgumentNullException.ThrowIfNull(propExpr, nameof(propExpr));
        
        var filterInfo = FilterCriteria.GetFilterInfo(propExpr, compareWith, comparisonType);

        return ExpressionBuilder.CreateWhereExpression<T>(filterInfo);
    }
}
