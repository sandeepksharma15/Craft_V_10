using Craft.Core;
using Craft.MultiTenant;
using Craft.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

/// <summary>
/// Base IdentityDbContext with pluggable feature support and ASP.NET Core Identity integration.
/// Provides convention configuration, model building hooks, and save pipeline integration for Identity-based applications.
/// All configuration should be done via DI at registration time for compatibility with DbContext pooling.
/// </summary>
/// <typeparam name="TContext">The concrete DbContext type.</typeparam>
/// <typeparam name="TUser">The user entity type (must inherit from CraftUser).</typeparam>
/// <typeparam name="TRole">The role entity type (must inherit from CraftRole).</typeparam>
/// <typeparam name="TKey">The type of the primary key for identity entities.</typeparam>
public abstract class BaseIdentityDbContext<TContext, TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey>, IDbContext
    where TContext : DbContext
    where TUser : CraftUser<TKey>
    where TRole : CraftRole<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly ITenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Creates a new instance of BaseIdentityDbContext for single-tenant scenarios.
    /// LoggerFactory and other options should be configured via DI, not in constructor.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    /// <param name="currentUser">The current user accessor.</param>
    /// <remarks>
    /// This constructor is for non-multi-tenant applications. It uses <see cref="NullTenant.Instance"/> internally.
    /// For multi-tenant applications, use the constructor that accepts an <see cref="ITenant"/> parameter.
    /// </remarks>
    protected BaseIdentityDbContext(DbContextOptions options, ICurrentUser currentUser)
        : this(options, NullTenant.Instance, currentUser)
    {
    }

    /// <summary>
    /// Creates a new instance of BaseIdentityDbContext for multi-tenant scenarios.
    /// LoggerFactory and other options should be configured via DI, not in constructor.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    /// <param name="currentTenant">The current tenant accessor.</param>
    /// <param name="currentUser">The current user accessor.</param>
    protected BaseIdentityDbContext(DbContextOptions options, ITenant currentTenant, ICurrentUser currentUser)
        : base(options)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Collection of features to be applied to this context.
    /// Consumers can populate this in their constructor to enable features.
    /// </summary>
    protected DbContextFeatureCollection Features { get; } = [];

    /// <summary>
    /// LoginHistory DbSet for tracking user login activity.
    /// </summary>
    public DbSet<LoginHistory<TKey>> LoginHistories { get; set; } = null!;

    /// <summary>
    /// RefreshToken DbSet for JWT token refresh management.
    /// </summary>
    public DbSet<RefreshToken<TKey>> RefreshTokens { get; set; } = null!;

    /// <summary>
    /// Configure provider-specific conventions. Sealed to ensure UTC DateTime conversion
    /// and feature-based conventions are always applied.
    /// </summary>
    protected override sealed void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Built-in UTC DateTime conversion
        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeToDateTimeUtc>();

        // Allow features to contribute conventions
        Features.ApplyConventions(configurationBuilder);

        // Allow consumer customizations
        ConfigureAdditionalConventions(configurationBuilder);
    }

    /// <summary>
    /// Override to add custom conventions beyond features and built-in UTC DateTime.
    /// </summary>
    protected virtual void ConfigureAdditionalConventions(ModelConfigurationBuilder configurationBuilder) { }

    /// <summary>
    /// Configure entity types and relationships. Features are applied after base configuration.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Register DbSets required by features
        RegisterFeatureDbSets(modelBuilder);

        // Allow features to configure the model
        Features.ApplyModel(modelBuilder);

        // Allow consumer customizations
        ConfigureAdditionalModel(modelBuilder);
    }

    /// <summary>
    /// Override to add custom model configuration beyond features.
    /// </summary>
    protected virtual void ConfigureAdditionalModel(ModelBuilder modelBuilder) { }

    /// <summary>
    /// Registers entity types as DbSets based on features that implement IDbSetProvider.
    /// </summary>
    private void RegisterFeatureDbSets(ModelBuilder modelBuilder)
    {
        foreach (var entityType in Features.GetRequiredDbSetTypes())
        {
            // Register the entity type with EF Core
            modelBuilder.Model.AddEntityType(entityType);
        }
    }

    /// <summary>
    /// SaveChanges with feature pipeline execution.
    /// </summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        Features.BeforeSaveChanges(this, _currentUser.GetId());
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// SaveChangesAsync with feature pipeline execution.
    /// </summary>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        Features.BeforeSaveChanges(this, _currentUser.GetId());
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // IDbContext interface implementations (inherited from DbContext)
    int IDbContext.SaveChanges() => SaveChanges();

    Task<int> IDbContext.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);
}

/// <summary>
/// Base IdentityDbContext with default CraftUser, CraftRole, and KeyType.
/// </summary>
/// <typeparam name="TContext">The concrete DbContext type.</typeparam>
public abstract class BaseIdentityDbContext<TContext> : BaseIdentityDbContext<TContext, CraftUser, CraftRole, KeyType>
    where TContext : DbContext
{
    /// <summary>
    /// Creates a new instance of BaseIdentityDbContext with default types.
    /// LoggerFactory and other options should be configured via DI, not in constructor.
    /// </summary>
    protected BaseIdentityDbContext(DbContextOptions options, ITenant currentTenant, ICurrentUser currentUser)
        : base(options, currentTenant, currentUser) { }
}

