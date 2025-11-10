using Craft.Core;
using Craft.MultiTenant;
using Craft.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Data;

/// <summary>
/// Base DbContext with pluggable feature support.
/// Provides convention configuration, model building hooks, and save pipeline integration.
/// </summary>
/// <typeparam name="TContext">The concrete DbContext type.</typeparam>
public abstract class BaseDbContext<TContext> : DbContext, IDbContext where TContext : DbContext
{
    private readonly ITenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly ILoggerFactory? _loggerFactory;

    protected BaseDbContext(DbContextOptions<TContext> options, ITenant currentTenant, ICurrentUser currentUser,
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
}
