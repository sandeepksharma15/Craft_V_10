using Craft.Core;
using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

/// <summary>
/// Base DbContext with pluggable feature support.
/// Provides convention configuration, model building hooks, and save pipeline integration.
/// All configuration should be done via DI at registration time for compatibility with DbContext pooling.
/// </summary>
/// <typeparam name="TContext">The concrete DbContext type.</typeparam>
public abstract class BaseDbContext<TContext> : DbContext, IDbContext where TContext : DbContext
{
    private readonly ITenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Creates a new instance of BaseDbContext.
    /// LoggerFactory and other options should be configured via DI, not in constructor.
    /// </summary>
    protected BaseDbContext(DbContextOptions<TContext> options, ITenant currentTenant, ICurrentUser currentUser)
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
        // Register the entity type with EF Core
        foreach (var entityType in Features.GetRequiredDbSetTypes())
            modelBuilder.Model.AddEntityType(entityType);
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
}
