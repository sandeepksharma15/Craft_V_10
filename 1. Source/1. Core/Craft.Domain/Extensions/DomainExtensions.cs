#pragma warning disable IDE0130 // Namespace does not match folder structure
using System.ComponentModel;
using Craft.Domain;

namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for domain entities and related types.
/// </summary>
public static class DomainExtensions
{
    /// <summary>
    /// Parses a string into the specified type. Returns default(TKey) if conversion fails.
    /// </summary>
    /// <typeparam name="TKey">The type to parse the string to.</typeparam>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed value, or default(TKey) if parsing fails.</returns>
    public static TKey? Parse<TKey>(this string? value)
    {
        if (value is null)
            return default;

        try
        {
            // Special case: object type should return the string itself
            if (typeof(TKey) == typeof(object))
                return (TKey?)(object?)value;

            var converter = TypeDescriptor.GetConverter(typeof(TKey));

            if (converter.CanConvertFrom(typeof(string)))
            {
                var result = converter.ConvertFromInvariantString(value);
                return (TKey?)result;
            }
        }
        catch (Exception)
        {
            return default;
        }

        return default;
    }

    /// <summary>
    /// Determines whether an entity is null or has a default identifier value.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity identifier.</typeparam>
    /// <param name="entity">The entity to check.</param>
    /// <returns>True if the entity is null or has a default ID; otherwise, false.</returns>
    public static bool IsNullOrDefault<TKey>(this IEntity<TKey> entity)
        => entity == null || entity.HasDefaultId();

    /// <summary>
    /// Determines whether an entity is null or has a default identifier value.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <returns>True if the entity is null or has a default ID; otherwise, false.</returns>
    public static bool IsNullOrDefault(this IEntity entity)
        => entity == null || entity.HasDefaultId();

    /// <summary>
    /// Sets the user identifier for an entity, typically used to track who created the entity.
    /// </summary>
    /// <typeparam name="TKey">The type of the user identifier.</typeparam>
    /// <param name="entity">The entity to update.</param>
    /// <param name="userId">The user identifier to set.</param>
    public static void SetCreatedBy<TKey>(this IHasUser<TKey> entity, TKey userId) 
        => entity.SetUserId(userId);

    /// <summary>
    /// Sets the user identifier for an entity, typically used to track who created the entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="userId">The user identifier to set.</param>
    public static void SetCreatedBy(this IHasUser entity, KeyType userId)
        => entity.SetUserId(userId);

    /// <summary>
    /// Determines whether an entity belongs to the specified tenant.
    /// </summary>
    /// <typeparam name="TKey">The type of the tenant identifier.</typeparam>
    /// <param name="entity">The entity to check.</param>
    /// <param name="tenantId">The tenant identifier to compare against.</param>
    /// <returns>True if the entity belongs to the specified tenant; otherwise, false.</returns>
    public static bool BelongsToTenant<TKey>(this IHasTenant<TKey> entity, TKey tenantId)
        => entity.TenantId?.Equals(tenantId) == true;

    /// <summary>
    /// Determines whether an entity belongs to the specified tenant.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <param name="tenantId">The tenant identifier to compare against.</param>
    /// <returns>True if the entity belongs to the specified tenant; otherwise, false.</returns>
    public static bool BelongsToTenant(this IHasTenant entity, KeyType tenantId)
        => entity.TenantId.Equals(tenantId) == true;
}
