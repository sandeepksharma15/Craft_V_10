﻿using Craft.QuerySpec.Contracts;

namespace Craft.QuerySpec.Evaluators;

public sealed class WhereEvaluator : IEvaluator
{
    private WhereEvaluator() { }

    public static WhereEvaluator Instance { get; } = new WhereEvaluator();

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T> query) where T : class
    {
        foreach (var condition in query.EntityFilterBuilder.EntityFilterList)
            queryable = queryable.Where(condition.Filter);

        return queryable;
    }
}
