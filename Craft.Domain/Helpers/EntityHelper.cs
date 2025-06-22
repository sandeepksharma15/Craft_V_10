using System.Reflection;

namespace Craft.Domain;

public static class EntityHelper
{
    /// <summary>
    /// Determines if two entities are equal.
    /// </summary>
    /// <param name="entity1">The first entity.</param>
    /// <param name="entity2">The second entity.</param>
    /// <returns>True if the entities are equal, false otherwise.</returns>
    public static bool EntityEquals<TKey>(this IEntity<TKey> entity1, IEntity<TKey> entity2)
    {
        if (entity1 == null || entity2 == null)
            return false;

        if (ReferenceEquals(entity1, entity2))
            return true;

        // Must have a IS-A relation of types or must be same type (Must be of compatible types)
        if (entity1.GetType().IsNotCompatibleWith(entity2.GetType()))
            return false;

        // If both entities have default IDs, they are not equal
        if (entity1.HasDefaultId() && entity2.HasDefaultId())
            return false;

        // IDs should be same
        if (!EqualityComparer<TKey>.Default.Equals(entity1.Id, entity2.Id))
            return false;

        // Different tenants may have an entity with same Id.
        // Check tenant IDs if applicable
        if (entity1 is IHasTenant tenant1 && entity2 is IHasTenant tenant2)
            return tenant1.TenantId == tenant2.TenantId;

        return true;
    }

    /// <summary>
    /// Determines if two entities are equal.
    /// </summary>
    /// <param name="entity1">The first entity.</param>
    /// <param name="entity2">The second entity.</param>
    /// <returns>True if the entities are equal, false otherwise.</returns>
    public static bool EntityEquals(this IEntity entity1, IEntity entity2)
        => entity1.EntityEquals<KeyType>(entity2);

    /// <summary>
    /// Checks if an entity has a default ID value.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's Id property.</typeparam>
    /// <param name="entity">The entity to check.</param>
    /// <returns>True if the entity has a default ID, false otherwise.</returns>
    public static bool HasDefaultId<TKey>(this IEntity<TKey> entity)
    {
        if (EqualityComparer<TKey>.Default.Equals(entity.Id, default))
            return true;

        // Workaround for EF Core since it sets int/long to min value when attaching to dbcontext
        if (typeof(TKey) == typeof(int))
            return Convert.ToInt32(entity.Id) <= 0;

        if (typeof(TKey) == typeof(long))
            return Convert.ToInt64(entity.Id) <= 0;

        return false;
    }

    /// <summary>
    /// Simplified version of HasDefaultId for non-generic usage.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <returns>True if the entity has a default ID, false otherwise.</returns>
    public static bool HasDefaultId(this IEntity entity)
        => HasDefaultId<KeyType>(entity);

    /// <summary>
    /// Determines if a type implements the IEntity interface.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is an entity, false otherwise.</returns>
    public static bool IsEntity<TKey>(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return typeof(IEntity<TKey>).IsAssignableFrom(type);
    }

    /// <summary>
    /// Determines if a type implements the IEntity interface.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is an entity, false otherwise.</returns>
    public static bool IsEntity(this Type type)
        => type.IsEntity<KeyType>();

    /// <summary>
    /// Determines if a type or its generic version implements the IHasTenant interface.
    /// </summary>
    /// <typeparam name="TEntity">The type to check.</typeparam>
    /// <returns>True if the type is multi-tenant, false otherwise.</returns>
    public static bool IsMultiTenant<TEntity>() where TEntity : IEntity
        => IsMultiTenant(typeof(TEntity));

    /// <summary>
    /// Determines if a type implements or derives from the IHasTenant interface.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is multi-tenant, false otherwise.</returns>
    public static bool IsMultiTenant(this Type type)
        => typeof(IHasTenant).IsAssignableFrom(type);

    /// <summary>
    /// Retrieves the entity key type for a given entity type.
    /// </summary>
    /// <param name="entityType">The type to check for an entity key type.</param>
    /// <returns>The entity key type, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown if the entity type does not implement IEntity.</exception>
    public static Type? GetEntityKeyType(this Type entityType)
    {
        if (entityType == null) return null;

        if (!typeof(IEntity).IsAssignableFrom(entityType))
            throw new ArgumentException($"{nameof(entityType)} is not an entity; it should implement {nameof(IEntity)}");

        foreach (var interfaceType in entityType.GetTypeInfo().GetInterfaces())
            if (interfaceType.GetTypeInfo().IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEntity<>))
                return interfaceType.GenericTypeArguments[0];

        return null;
    }
}
