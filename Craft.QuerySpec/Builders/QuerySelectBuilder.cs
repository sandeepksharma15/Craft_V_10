using System.Linq.Expressions;
using System.Reflection;

namespace Craft.QuerySpec;

[Serializable]
public class QuerySelectBuilder<T> : QuerySelectBuilder<T, T>, IQuerySelectBuilder<T> where T : class;

/// <summary>
/// A builder class for constructing select expressions.
/// </summary>
[Serializable]
public class QuerySelectBuilder<T, TResult> : IQuerySelectBuilder<T, TResult>
    where T : class
    where TResult : class
{
    public List<SelectDescriptor<T, TResult>> SelectDescriptorList { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySelectBuilder{T, TResult}"/> class.
    /// </summary>
    public QuerySelectBuilder() => SelectDescriptorList = [];

    /// <summary>
    /// Gets the count of select expressions.
    /// </summary>
    public long Count => SelectDescriptorList.Count;

    public QuerySelectBuilder<T, TResult> Add(Expression<Func<T, object>> assignor, Expression<Func<TResult, object>> assignee)
    {
        ArgumentNullException.ThrowIfNull(assignor);
        ArgumentNullException.ThrowIfNull(assignee);

        var assignorName = assignor.GetMemberName();
        var assigneeName = assignee.GetMemberName();

        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(assignorName, assigneeName));

        return this;
    }

    public QuerySelectBuilder<T, TResult> Add(SelectDescriptor<T, TResult> selectDescriptor)
    {
        ArgumentNullException.ThrowIfNull(selectDescriptor);

        SelectDescriptorList.Add(selectDescriptor);
        return this;
    }

    /// <summary>
    /// Adds a select expression.
    /// </summary>
    public QuerySelectBuilder<T, TResult> Add(LambdaExpression assignor, LambdaExpression assignee)
    {
        ArgumentNullException.ThrowIfNull(assignor);
        ArgumentNullException.ThrowIfNull(assignee);

        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(assignor, assignee));

        return this;
    }

    public QuerySelectBuilder<T, TResult> Add(Expression<Func<T, object>> column)
    {
        ArgumentNullException.ThrowIfNull(column);

        var columnName = column.GetMemberName();

        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(columnName));

        return this;
    }

    /// <summary>
    /// Adds a select expression with a single column.
    /// </summary>
    public QuerySelectBuilder<T, TResult> Add(LambdaExpression column)
    {
        ArgumentNullException.ThrowIfNull(column);

        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(column));

        return this;
    }

    /// <summary>
    /// Adds a select expression with a single column identified by its name.
    /// </summary>
    public QuerySelectBuilder<T, TResult> Add(string assignorPropName)
    {
        ArgumentNullException.ThrowIfNull(assignorPropName);

        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(assignorPropName));

        return this;
    }

    /// <summary>
    /// Adds a select expression with a mapping between two properties.
    /// </summary>
    public QuerySelectBuilder<T, TResult> Add(string assignorPropName, string assigneePropName)
    {
        ArgumentNullException.ThrowIfNull(assignorPropName);

        SelectDescriptorList.Add(new SelectDescriptor<T, TResult>(assignorPropName, assigneePropName));

        return this;
    }

    /// <summary>
    /// Builds the select expression.
    /// </summary>
    public Expression<Func<T, TResult>>? Build()
        => typeof(TResult) == typeof(object) ? BuildAnnonymousSelect() : BuildSelect();

    /// <summary>
    /// Clears the select expressions.
    /// </summary>
    public void Clear()
        => SelectDescriptorList.Clear();

    private Expression<Func<T, TResult>>? BuildAnnonymousSelect()
    {
        var sourceParam = Expression.Parameter(typeof(T), "x");

        var selectExpressions = SelectDescriptorList.Select(item =>
        {
            var columnInvoke = Expression.Invoke(item.Assignor!, sourceParam);

            return Expression.Convert(columnInvoke, typeof(object));
        });

        var selectorBody = Expression.NewArrayInit(typeof(TResult), selectExpressions);

        return Expression.Lambda<Func<T, TResult>>(selectorBody, sourceParam);
    }

    private Expression<Func<T, TResult>> BuildSelect()
    {
        if (SelectDescriptorList.Count == 0)
            throw new InvalidOperationException("No select mappings defined in QuerySelectBuilder.");

        var selectParam = Expression.Parameter(typeof(T));
        var memberBindings = new List<MemberBinding>();

        foreach (var item in SelectDescriptorList)
        {
            var columnInvoke = Expression.Invoke(item.Assignor!, selectParam);
            var propertyInfo = item.Assignee?.GetPropertyInfo();
            var propertyType = propertyInfo?.PropertyType;

            if (propertyInfo == null || propertyType == null)
                throw new InvalidOperationException("Property info or type cannot be null.");

            var convertedValue = Expression.Convert(columnInvoke, propertyType);

            var memberBinding = Expression.Bind(propertyInfo, convertedValue);
            memberBindings.Add(memberBinding);
        }

        var memberInit = Expression.MemberInit(Expression.New(typeof(TResult)), memberBindings);
        var selectorLambda = Expression.Lambda<Func<T, TResult>>(memberInit, selectParam);

        return Expression.Lambda<Func<T, TResult>>(selectorLambda.Body, selectParam);
    }
}
