using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Craft.Data.Abstractions;

/// <summary>
/// Represents a contract for a database context that provides access to entity sets,  change tracking, and database
/// operations.
/// </summary>
/// <remarks>This interface defines the core functionality required for interacting with a database  in an
/// object-relational mapping (ORM) context. It includes methods for saving changes,  accessing entity sets, tracking
/// changes, and configuring the database context.</remarks>
/// Usage: 
/// services.AddDbContext<AppDbContext>(options =>
///     options.UseSqlServer(configuration.GetConnectionString("Default")));
///     
/// services.AddScoped<IDbContext>(provider => provider.GetService<AppDbContext>());
/// 
public interface IDbContext
{
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    public DbSet<TEntity> Set<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TEntity>(string name) where TEntity : class;
    public DbSet<TEntity> Set<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TEntity>() where TEntity : class;

    public ChangeTracker ChangeTracker { get; }
    public DatabaseFacade Database { get; }

    public EntityEntry Entry(object entity);
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
