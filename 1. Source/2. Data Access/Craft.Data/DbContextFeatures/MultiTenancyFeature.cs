using Craft.Domain;
using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.DbContextFeatures;

/// <summary>
/// Feature that enables multi-tenancy for entities implementing IHasTenant.
/// Automatically applies global query filters and sets tenant ID on entity creation.
/// </summary>
public class MultiTenancyFeature : IDbContextFeature
{
    private readonly ITenant _currentTenant;

    /// <summary>
    /// Initializes a new instance of the MultiTenancyFeature with the current tenant.
    /// </summary>
    /// <param name="currentTenant">The current tenant context.</param>
    public MultiTenancyFeature(ITenant currentTenant) => _currentTenant = currentTenant;

    /// <summary>
    /// Applies global query filter to include only current tenant's data.
    /// </summary>
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        // Apply global query filter for all entities implementing IHasTenant
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
            {
                // Create the filter expression: entity => entity.TenantId == _currentTenant.Id
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(IHasTenant.TenantId));
                var tenantId = System.Linq.Expressions.Expression.Constant(_currentTenant.GetId());
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(property, tenantId),
                    parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    /// <summary>
    /// Automatically sets TenantId for new entities.
    /// </summary>
    public void OnBeforeSaveChanges(DbContext context, KeyType userId)
    {
        var entries = context.ChangeTracker.Entries<IHasTenant>()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        foreach (var entry in entries)
        {
            // Set tenant ID for new entities
            entry.Entity.SetTenantId(_currentTenant.GetId());
        }
    }
}

