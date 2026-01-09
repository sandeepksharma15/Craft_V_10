using Craft.Core;
using Craft.Domain;
using Craft.QuerySpec;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components;

/// <summary>
/// Interface for the card grid component.
/// </summary>
/// <typeparam name="TEntity">The entity type displayed in the card grid.</typeparam>
public interface ICraftCardGrid<TEntity> where TEntity : class, IEntity, IModel
{
    /// <summary>
    /// List of fields in the card grid.
    /// </summary>
    List<ICraftCardGridField<TEntity>> Fields { get; }

    /// <summary>
    /// Adds a field to the card grid.
    /// </summary>
    void AddField(ICraftCardGridField<TEntity> field);

    /// <summary>
    /// Removes a field from the card grid.
    /// </summary>
    void RemoveField(ICraftCardGridField<TEntity> field);

    /// <summary>
    /// Refreshes the card grid data.
    /// </summary>
    Task RefreshAsync();

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    int CurrentPage { get; }

    /// <summary>
    /// Current page size.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    long TotalCount { get; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// Currently loaded items in the card grid.
    /// </summary>
    IReadOnlyList<TEntity> Items { get; }
}

/// <summary>
/// Interface for a card grid field.
/// </summary>
/// <typeparam name="TEntity">The entity type displayed in the card grid.</typeparam>
public interface ICraftCardGridField<TEntity> where TEntity : class, IEntity, IModel
{
    /// <summary>
    /// The parent card grid.
    /// </summary>
    ICraftCardGrid<TEntity>? CardGrid { get; set; }

    /// <summary>
    /// Field caption displayed in the card.
    /// </summary>
    string Caption { get; set; }

    /// <summary>
    /// Property name for sorting. Auto-derived from PropertyExpression if not set.
    /// </summary>
    string? PropertyName { get; }

    /// <summary>
    /// Property expression for accessing the entity property.
    /// </summary>
    System.Linq.Expressions.Expression<Func<TEntity, object>>? PropertyExpression { get; set; }

    /// <summary>
    /// Custom template for rendering field content.
    /// </summary>
    RenderFragment<TEntity>? Template { get; set; }

    /// <summary>
    /// Indicates whether the field is visible.
    /// Default is true.
    /// </summary>
    bool Visible { get; set; }

    /// <summary>
    /// Indicates whether the field is sortable.
    /// Default is false.
    /// </summary>
    bool Sortable { get; set; }

    /// <summary>
    /// Indicates whether the field is searchable.
    /// Default is false.
    /// </summary>
    bool Searchable { get; set; }

    /// <summary>
    /// Default sort direction for the field.
    /// Null means no default sorting.
    /// </summary>
    GridSortDirection? DefaultSort { get; set; }

    /// <summary>
    /// Sort order when multiple fields have default sorting.
    /// Lower values are sorted first.
    /// Default is 0.
    /// </summary>
    int SortOrder { get; set; }

    /// <summary>
    /// Format string for displaying values (e.g., "N2", "d", "C").
    /// </summary>
    string? Format { get; set; }

    /// <summary>
    /// Type of field (Id, Title, SubTitle, or Field).
    /// </summary>
    CardFieldType FieldType { get; set; }

    /// <summary>
    /// Renders the field content for the given item.
    /// </summary>
    string Render(TEntity item);
}

/// <summary>
/// Represents the type of card field.
/// </summary>
public enum CardFieldType
{
    /// <summary>
    /// Regular field displayed in the card body.
    /// </summary>
    Field,

    /// <summary>
    /// Entity ID field (used for CRUD operations).
    /// </summary>
    Id,

    /// <summary>
    /// Primary title field displayed prominently.
    /// </summary>
    Title,

    /// <summary>
    /// Subtitle field displayed below the title.
    /// </summary>
    SubTitle
}
