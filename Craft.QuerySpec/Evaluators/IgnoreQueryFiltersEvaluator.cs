﻿using Craft.QuerySpec.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Craft.QuerySpec.Evaluators;

public sealed class IgnoreQueryFiltersEvaluator : IEvaluator
{
    public static IgnoreQueryFiltersEvaluator Instance { get; } = new IgnoreQueryFiltersEvaluator();

    private IgnoreQueryFiltersEvaluator()
    { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T> query) where T : class
    {
        if (query.IgnoreQueryFilters)
            queryable = queryable.IgnoreQueryFilters();

        return queryable;
    }
}
