using System.Linq.Expressions;
using Craft.Domain;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components;

/// <summary>
/// Interface for the card grid component.
/// </summary>
/// <typeparam name="TEntity">The entity type displayed in the card grid.</typeparam>
public interface ICraftCardGrid<TEntity> where TEntity : class, IEntity, IModel
{
    /// <summary>
    /// List of columns in the card grid.
    /// </summary>
    List<ICraftCardGridColumn<TEntity>> Columns { get; }

    /// <summary>
    /// Adds a column to the card grid.
    /// </summary>
    void AddColumn(ICraftCardGridColumn<TEntity> column);

    /// <summary>
    /// Removes a column from the card grid.
    /// </summary>
    void RemoveColumn(ICraftCardGridColumn<TEntity> column);

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
/// Interface for a card grid column.
/// </summary>
/// <typeparam name="TEntity">The entity type displayed in the card grid.</typeparam>
public interface ICraftCardGridColumn<TEntity> where TEntity : class, IEntity, IModel
{
    /// <summary>
    /// The parent card grid.
    /// </summary>
    ICraftCardGrid<TEntity>? CardGrid { get; set; }

    /// <summary>
    /// Column caption displayed in the card.
    /// </summary>
    string Caption { get; set; }

    /// <summary>
    /// Property name for sorting. Auto-derived from PropertyExpression if not set.
    /// </summary>
    string? PropertyName { get; }

    /// <summary>
    /// Property expression for accessing the entity property.
    /// </summary>
    Expression<Func<TEntity, object>>? PropertyExpression { get; set; }

    /// <summary>
    /// Custom template for rendering column content.
    /// </summary>
    RenderFragment<TEntity>? Template { get; set; }

    /// <summary>
    /// Indicates whether the column is visible.
    /// Default is true.
    /// </summary>
    bool Visible { get; set; }

    /// <summary>
    /// Indicates whether the column is sortable.
    /// Default is false.
    /// </summary>
    bool Sortable { get; set; }

    /// <summary>
    /// Indicates whether the column is searchable.
    /// Default is false.
    /// </summary>
    bool Searchable { get; set; }

    /// <summary>
    /// Default sort direction for the column.
    /// Null means no default sorting.
    /// </summary>
    GridSortDirection? DefaultSort { get; set; }

    /// <summary>
    /// Sort order when multiple columns have default sorting.
    /// Lower values are sorted first.
    /// Default is 0.
    /// </summary>
    int SortOrder { get; set; }

    /// <summary>
    /// Format string for displaying values (e.g., "N2", "d", "C").
    /// </summary>
    string? Format { get; set; }

    /// <summary>
    /// Type of column (Id, Title, SubTitle, or Field).
    /// </summary>
    CardFieldType FieldType { get; set; }

    /// <summary>
    /// Renders the column content for the given item.
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
