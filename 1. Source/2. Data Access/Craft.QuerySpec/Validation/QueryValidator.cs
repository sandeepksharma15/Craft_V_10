using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;

namespace Craft.QuerySpec;

/// <summary>
/// Default implementation of query validator that checks query specifications against configured limits.
/// </summary>
/// <typeparam name="T">The entity type being queried.</typeparam>
public class QueryValidator<T> : IQueryValidator<T> where T : class
{
    private readonly ILogger<QueryValidator<T>> _logger;
    private readonly QueryOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryValidator{T}"/> class.
    /// </summary>
    /// <param name="options">Query configuration options.</param>
    /// <param name="logger">Logger for validation operations.</param>
    public QueryValidator(IOptions<QueryOptions> options, ILogger<QueryValidator<T>> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<ValidationResult> ValidateAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var errors = new List<string>();

        ValidateFilterCount(query, errors);
        ValidateIncludeCount(query, errors);
        ValidatePageSize(query, errors);
        ValidateOrderByCount(query, errors);
        ValidatePropertyAccess(query, errors);

        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Query validation failed for {EntityType} with {ErrorCount} error(s): {Errors}",
                typeof(T).Name, errors.Count, string.Join("; ", errors));
        }

        var result = errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);

        return Task.FromResult(result);
    }

    private void ValidateFilterCount(IQuery<T> query, List<string> errors)
    {
        if (_options.MaxFilterCount <= 0) return;

        var filterCount = query.EntityFilterBuilder?.Count ?? 0;

        if (filterCount > _options.MaxFilterCount)
        {
            errors.Add($"Too many filters ({filterCount}). Maximum allowed: {_options.MaxFilterCount}");
        }
    }

    private void ValidateIncludeCount(IQuery<T> query, List<string> errors)
    {
        if (_options.MaxIncludeCount <= 0) return;

        var includeCount = query.IncludeExpressions?.Count ?? 0;

        if (includeCount > _options.MaxIncludeCount)
        {
            errors.Add($"Too many includes ({includeCount}). Maximum allowed: {_options.MaxIncludeCount}");
        }
    }

    private void ValidatePageSize(IQuery<T> query, List<string> errors)
    {
        if (_options.MaxPageSize <= 0) return;

        if (query.Take.HasValue && query.Take.Value > _options.MaxPageSize)
        {
            errors.Add($"Page size too large ({query.Take.Value}). Maximum allowed: {_options.MaxPageSize}");
        }
    }

    private void ValidateOrderByCount(IQuery<T> query, List<string> errors)
    {
        if (_options.MaxOrderByFields <= 0) return;

        var orderByCount = query.SortOrderBuilder?.Count ?? 0;

        if (orderByCount > _options.MaxOrderByFields)
        {
            errors.Add($"Too many order by fields ({orderByCount}). Maximum allowed: {_options.MaxOrderByFields}");
        }
    }

    private void ValidatePropertyAccess(IQuery<T> query, List<string> errors)
    {
        // Validate filter properties exist and are accessible
        if (query.EntityFilterBuilder != null)
        {
            foreach (var filter in query.EntityFilterBuilder.EntityFilterList)
            {
                if (!IsValidFilterProperty(filter, out var propertyName))
                {
                    errors.Add($"Invalid or inaccessible property in filter: {propertyName ?? "unknown"}");
                }
            }
        }

        // Validate order by properties exist and are accessible
        if (query.SortOrderBuilder != null)
        {
            foreach (var orderDescriptor in query.SortOrderBuilder.OrderDescriptorList)
            {
                if (!IsValidOrderProperty(orderDescriptor, out var propertyName))
                {
                    errors.Add($"Invalid or inaccessible property in order by: {propertyName ?? "unknown"}");
                }
            }
        }
    }

    private bool IsValidFilterProperty(EntityFilterCriteria<T> filter, out string? propertyName)
    {
        propertyName = null;

        try
        {
            // Try to extract property name from the Metadata (FilterCriteria)
            if (filter.Metadata != null)
            {
                propertyName = filter.Metadata.Name;
                return IsPropertyAccessible(propertyName);
            }

            // If Metadata is null, the filter expression is still valid
            // as it was created through the builder - we trust it
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidOrderProperty(OrderDescriptor<T> orderDescriptor, out string? propertyName)
    {
        propertyName = null;

        try
        {
            // Extract property name from the OrderItem lambda expression
            if (orderDescriptor.OrderItem.Body is MemberExpression memberExpr)
            {
                propertyName = memberExpr.Member.Name;
                return IsPropertyAccessible(propertyName);
            }

            // Handle UnaryExpression (e.g., Convert)
            if (orderDescriptor.OrderItem.Body is UnaryExpression unaryExpr &&
                unaryExpr.Operand is MemberExpression innerMemberExpr)
            {
                propertyName = innerMemberExpr.Member.Name;
                return IsPropertyAccessible(propertyName);
            }

            return true; // Expression is valid, just couldn't extract property name
        }
        catch
        {
            return false;
        }
    }

    private bool IsPropertyAccessible(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return false;

        var property = typeof(T).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance);

        return property != null && property.CanRead;
    }
}
