using Craft.Core;
using Craft.MultiTenant;
using Craft.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Data;

/// <summary>
/// Base IdentityDbContext with pluggable feature support and ASP.NET Core Identity integration.
/// Provides convention configuration, model building hooks, and save pipeline integration for Identity-based applications.
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
    private readonly ILoggerFactory? _loggerFactory;

    protected BaseIdentityDbContext(DbContextOptions options, ITenant currentTenant, ICurrentUser currentUser,
        ILoggerFactory? loggerFactory = null) : base(options)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _loggerFactory = loggerFactory;
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
    /// Configure DbContext options like logging and tracking behavior.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Configure logger factory if provided
        if (_loggerFactory != null)
            optionsBuilder.UseLoggerFactory(_loggerFactory);

        // Default to NoTracking for better performance (consumers can override)
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        // Allow consumer customizations
        ConfigureAdditionalOptions(optionsBuilder);
    }

    /// <summary>
    /// Override to configure additional DbContext options.
    /// </summary>
    protected virtual void ConfigureAdditionalOptions(DbContextOptionsBuilder optionsBuilder) { }

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
    protected BaseIdentityDbContext(DbContextOptions options, ITenant currentTenant, ICurrentUser currentUser,
        ILoggerFactory? loggerFactory = null) : base(options, currentTenant, currentUser, loggerFactory) { }
}
