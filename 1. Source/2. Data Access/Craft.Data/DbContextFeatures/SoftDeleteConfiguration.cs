namespace Craft.Data;

/// <summary>
/// Configuration for the SoftDeleteFeature.
/// Provides options for customizing soft delete behavior.
/// </summary>
public class SoftDeleteConfiguration
{
    /// <summary>
    /// Gets or sets the database provider.
    /// Used to generate provider-specific SQL for filtered indexes.
    /// Defaults to AutoDetect.
    /// </summary>
    public DatabaseProvider DatabaseProvider { get; set; } = DatabaseProvider.AutoDetect;

    /// <summary>
    /// Gets or sets whether to apply filters to unique indexes to allow reusing unique values after soft deletion.
    /// Default is true.
    /// </summary>
    public bool ApplyFiltersToUniqueIndexes { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to create a filtered index on the IsDeleted column.
    /// Default is true.
    /// </summary>
    public bool CreateIsDeletedIndex { get; set; } = true;
}
