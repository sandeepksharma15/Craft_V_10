using Craft.Core;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Craft.Extensions.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Craft.QuerySpec;

public static class SearchExtension
{
    private static readonly MemberExpression Functions = Expression
        .Property(null, typeof(EF)
        .GetProperty(nameof(EF.Functions)) ?? throw new TargetException("The EF.Functions not found!"));

    private static readonly MethodInfo LikeMethodInfo = typeof(DbFunctionsExtensions)
            .GetMethod(nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string)])
        ?? throw new TargetException("The EF.Functions.Like not found");

    private static readonly MethodInfo StringContainsMethodInfo = typeof(string)
            .GetMethod(nameof(string.Contains), [typeof(string)])
        ?? throw new TargetException("The string.Contains method not found");

    private static readonly MethodInfo StringStartsWithMethodInfo = typeof(string)
            .GetMethod(nameof(string.StartsWith), [typeof(string)])
        ?? throw new TargetException("The string.StartsWith method not found");

    private static readonly MethodInfo StringEndsWithMethodInfo = typeof(string)
            .GetMethod(nameof(string.EndsWith), [typeof(string)])
        ?? throw new TargetException("The string.EndsWith method not found");

    /// <summary>
    /// Filters <paramref name="source"/> by applying an 'SQL LIKE' operation to it.
    /// </summary>
    /// <typeparam name="T">The type being queried against.</typeparam>
    /// <param name="source">The sequence of <typeparamref name="T"/></param>
    /// <param name="criterias">
    /// <list type="bullet">
    ///     <item>Selector, the property to apply the SQL LIKE against.</item>
    ///     <item>SearchString, the value to use for the SQL LIKE.</item>
    /// </list>
    /// </param>
    /// <returns></returns>
    public static IQueryable<T>? Search<T>(this IQueryable<T> source, IEnumerable<SqlLikeSearchInfo<T>> criterias)
        where T : class
    {
        if (criterias is null) return source;

        Expression? expression = null;
        var parameter = Expression.Parameter(typeof(T), "x");

        foreach (var group in criterias
                     .Where(criteria => criteria is not null &&
                                        criteria.SearchItem is not null &&
                                        !string.IsNullOrEmpty(criteria.SearchString))
                     .GroupBy(criteria => criteria.SearchGroup)
                     .OrderBy(group => group.Key))
        {
            Expression? groupExpression = null;

            foreach (var criteria in group)
            {
                var propertySelector = ParameterReplacerVisitor.Replace(
                    criteria.SearchItem!,
                    criteria.SearchItem!.Parameters[0],
                    parameter) as LambdaExpression
                    ?? throw new InvalidExpressionException();

                var searchTermAsExpression =
                    ((Expression<Func<string>>)(() => criteria.SearchString!)).Body;

                var searchExpression = StringValueObjectSearch.GetSearchExpression(propertySelector.Body);

                var likeExpression = BuildPredicate(searchExpression, criteria.SearchString!, searchTermAsExpression);

                groupExpression = groupExpression is null
                    ? likeExpression
                    : Expression.OrElse(groupExpression, likeExpression);
            }

            if (groupExpression is not null)
            {
                expression = expression is null
                    ? groupExpression
                    : Expression.AndAlso(expression, groupExpression);
            }
        }

        return expression is null
            ? source
            : source.Where(Expression.Lambda<Func<T, bool>>(expression, parameter));
    }

    private static Expression BuildPredicate(Expression searchExpression, string searchTerm, Expression searchTermAsExpression)
    {
        ArgumentNullException.ThrowIfNull(searchExpression);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchTerm);
        ArgumentNullException.ThrowIfNull(searchTermAsExpression);

        if (TryBuildSimpleLikePredicate(searchExpression, searchTerm, out var predicate))
            return predicate;

        return Expression.Call(
            null,
            LikeMethodInfo,
            Functions,
            searchExpression,
            searchTermAsExpression);
    }

    private static bool TryBuildSimpleLikePredicate(Expression searchExpression, string searchTerm, out Expression predicate)
    {
        predicate = null!;

        if (searchExpression.Type != typeof(string) || searchTerm.Contains('_', StringComparison.Ordinal))
            return false;

        var startsWithWildcard = searchTerm.StartsWith('%');
        var endsWithWildcard = searchTerm.EndsWith('%');
        var value = searchTerm.Trim('%');

        if (searchTerm.Length != value.Length + (startsWithWildcard ? 1 : 0) + (endsWithWildcard ? 1 : 0))
            return false;

        var constant = Expression.Constant(value);
        var notNullExpression = Expression.NotEqual(searchExpression, Expression.Constant(null, typeof(string)));

        Expression comparison = (startsWithWildcard, endsWithWildcard) switch
        {
            (true, true) => Expression.Call(searchExpression, StringContainsMethodInfo, constant),
            (true, false) => Expression.Call(searchExpression, StringEndsWithMethodInfo, constant),
            (false, true) => Expression.Call(searchExpression, StringStartsWithMethodInfo, constant),
            (false, false) => Expression.Equal(searchExpression, constant)
        };

        predicate = Expression.AndAlso(notNullExpression, comparison);
        return true;
    }
}


