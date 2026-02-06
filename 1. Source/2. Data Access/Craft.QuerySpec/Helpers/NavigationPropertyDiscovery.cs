using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Craft.QuerySpec;

/// <summary>
/// Helper class to discover navigation properties using reflection and EF Core metadata.
/// Uses caching for performance optimization.
/// </summary>
internal static class NavigationPropertyDiscovery
{
    private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> _navigationPropertiesCache = new();

    /// <summary>
    /// Discovers all navigation properties for the specified entity type.
    /// Results are cached for performance.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>List of navigation property PropertyInfo objects.</returns>
    public static List<PropertyInfo> DiscoverNavigationProperties<T>() where T : class
    {
        var entityType = typeof(T);

        if (_navigationPropertiesCache.TryGetValue(entityType, out var cachedProperties))
            return cachedProperties;

        var navigationProperties = new List<PropertyInfo>();

        // Get all public instance properties
        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Skip if property doesn't have a getter
            if (property.GetMethod is null)
                continue;

            // Skip value types and strings (they can't be navigation properties)
            if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                continue;

            // Skip if the property is an array (except byte[] which is usually not a navigation)
            if (property.PropertyType.IsArray)
                continue;

            // Check if it's a potential navigation property
            if (IsNavigationProperty(property))
                navigationProperties.Add(property);
        }

        // Cache the result
        _navigationPropertiesCache.TryAdd(entityType, navigationProperties);

        return navigationProperties;
    }

    /// <summary>
    /// Determines if a property is a navigation property.
    /// </summary>
    private static bool IsNavigationProperty(PropertyInfo property)
    {
        var propertyType = property.PropertyType;

        // Check if it's a reference navigation property (single entity)
        if (propertyType.IsClass && 
            !propertyType.IsAbstract && 
            propertyType != typeof(string) &&
            !propertyType.IsPrimitive)
        {
            // Check if it has an Id property or properties that suggest it's an entity
            var hasIdProperty = propertyType.GetProperty("Id") != null;
            if (hasIdProperty)
                return true;
        }

        // Check if it's a collection navigation property
        if (propertyType.IsGenericType)
        {
            var genericTypeDefinition = propertyType.GetGenericTypeDefinition();
            
            // Check for common collection types
            if (genericTypeDefinition == typeof(ICollection<>) ||
                genericTypeDefinition == typeof(IList<>) ||
                genericTypeDefinition == typeof(List<>) ||
                genericTypeDefinition == typeof(IEnumerable<>) ||
                genericTypeDefinition == typeof(HashSet<>))
            {
                var elementType = propertyType.GetGenericArguments()[0];
                
                // Check if the element type looks like an entity
                if (elementType.IsClass && 
                    !elementType.IsAbstract && 
                    elementType != typeof(string))
                {
                    var hasIdProperty = elementType.GetProperty("Id") != null;
                    return hasIdProperty;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Creates an Include expression for the specified navigation property.
    /// </summary>
    public static IncludeExpression CreateIncludeExpression<T>(PropertyInfo navigationProperty) where T : class
    {
        // Create parameter expression: entity => entity
        var parameter = Expression.Parameter(typeof(T), "entity");

        // Create property access: entity => entity.NavigationProperty
        var propertyAccess = Expression.Property(parameter, navigationProperty);

        // Create lambda expression
        var lambda = Expression.Lambda(propertyAccess, parameter);

        return new IncludeExpression(
            lambda,
            typeof(T),
            navigationProperty.PropertyType,
            isThenInclude: false,
            previousInclude: null);
    }

    /// <summary>
    /// Clears the navigation properties cache.
    /// Useful for testing or when entity model changes at runtime.
    /// </summary>
    public static void ClearCache()
    {
        _navigationPropertiesCache.Clear();
    }
}
