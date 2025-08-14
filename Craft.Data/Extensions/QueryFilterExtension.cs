using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Craft.Data;

public static class QueryFilterExtension
{
    public static ModelBuilder AddGlobalQueryFilter<T>(this ModelBuilder modelBuilder, string filterName, Expression<Func<T, bool>> filter)
    {
        if (typeof(T).IsInterface == false)
            throw new ArgumentException("Type argument must be an interface");

        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));
        ArgumentException.ThrowIfNullOrWhiteSpace(filterName, nameof(filterName));
        ArgumentNullException.ThrowIfNull(filter, nameof(filter));

        // Get List Of Entities That Implement The Interface TInterface
        var entities = modelBuilder
            .Model
            .GetEntityTypes()
            .Where(e => e.ClrType.GetInterface(typeof(T).Name) is not null)
            .Select(e => e.ClrType);

        foreach (var entity in entities)
        {
            // Get The Existing Query Filter using GetDeclaredQueryFilters()
            var declaredFilters = modelBuilder
                .Entity(entity)
                .Metadata
                .GetDeclaredQueryFilters();

            // If the filter with the specified name already exists; don't do anything
            if (declaredFilters.Count > 0)
                if (declaredFilters.Any(f => f.Key == filterName))
                    continue;

            var parameterType = Expression.Parameter(modelBuilder.Entity(entity).Metadata.ClrType);
            var filterBody = ReplacingExpressionVisitor.Replace(filter.Parameters.Single(), parameterType, filter.Body);

            // Apply The Query Filter
            modelBuilder
                .Entity(entity)
                .HasQueryFilter(filterName, Expression.Lambda(filterBody, parameterType));
        }

        return modelBuilder;
    }
}
