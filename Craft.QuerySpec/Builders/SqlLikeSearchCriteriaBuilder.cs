﻿using System.Linq.Expressions;
using Craft.Extensions.Expressions;

namespace Craft.QuerySpec;

public class SqlLikeSearchCriteriaBuilder<T> where T : class
{
    public List<SqlLikeSearchInfo<T>> SqlLikeSearchCriteriaList { get; }

    public SqlLikeSearchCriteriaBuilder() => SqlLikeSearchCriteriaList = [];

    public long Count => SqlLikeSearchCriteriaList.Count;

    public SqlLikeSearchCriteriaBuilder<T> Add(SqlLikeSearchInfo<T> searchInfo)
    {
        ArgumentNullException.ThrowIfNull(searchInfo);

        SqlLikeSearchCriteriaList.Add(searchInfo);
        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Add(Expression<Func<T, object>> member, string searchString, int searchGroup = 1)
    {
        SqlLikeSearchCriteriaList.Add(new SqlLikeSearchInfo<T>(member, searchString, searchGroup));
        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Add(string memberName, string searchString, int searchGroup = 1)
    {
        var member = ExpressionBuilder.GetPropertyExpression<T>(memberName);
        SqlLikeSearchCriteriaList.Add(new SqlLikeSearchInfo<T>(member!, searchString, searchGroup));

        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Clear()
    {
        SqlLikeSearchCriteriaList.Clear();

        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Remove(SqlLikeSearchInfo<T> searchInfo)
    {
        ArgumentNullException.ThrowIfNull(searchInfo);

        SqlLikeSearchCriteriaList.Remove(searchInfo);

        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Remove(Expression<Func<T, object>> member)
    {
        ArgumentNullException.ThrowIfNull(member);

        var comparer = new ExpressionSemanticEqualityComparer();
        var searchInfo = SqlLikeSearchCriteriaList.Find(x => comparer.Equals(x.SearchItem, member));

        if (searchInfo != null)
            SqlLikeSearchCriteriaList.Remove(searchInfo);

        return this;
    }

    public SqlLikeSearchCriteriaBuilder<T> Remove(string memberName)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(memberName));

        Remove(ExpressionBuilder.GetPropertyExpression<T>(memberName)!);

        return this;
    }
}
