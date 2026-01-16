using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components;

/// <summary>
/// Interface for the data grid component.
/// </summary>
/// <typeparam name="TEntity">The entity type displayed in the grid.</typeparam>
public interface ICraftDataGrid<TEntity> where TEntity : class
{
    /// <summary>
    /// List of columns in the grid.
    /// </summary>
    List<ICraftDataGridColumn<TEntity>> Columns { get; }

    /// <summary>
    /// Adds a column to the grid.
    /// </summary>
    void AddColumn(ICraftDataGridColumn<TEntity> column);

    /// <summary>
    /// Removes a column from the grid.
    /// </summary>
    void RemoveColumn(ICraftDataGridColumn<TEntity> column);

    /// <summary>
    /// Refreshes the grid data.
    /// </summary>
    Task RefreshAsync();
}

/// <summary>
/// Interface for a data grid column.
/// </summary>
/// <typeparam name="TEntity">The entity type displayed in the grid.</typeparam>
public interface ICraftDataGridColumn<TEntity> where TEntity : class
{
    /// <summary>
    /// The parent data grid.
    /// </summary>
    ICraftDataGrid<TEntity>? DataGrid { get; set; }

    /// <summary>
    /// Column title displayed in the header.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Property name for sorting. Auto-derived from PropertyExpression if not set.
    /// </summary>
    string? PropertyName { get; }

    /// <summary>
    /// Property type. Auto-derived from PropertyExpression if not set.
    /// Used for determining appropriate search operators and input types.
    /// </summary>
    Type? PropertyType { get; }

    /// <summary>
    /// Property expression for accessing the entity property.
    /// </summary>
    Expression<Func<TEntity, object>>? PropertyExpression { get; set; }

    /// <summary>
    /// Custom template for rendering cell content.
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
    /// Column width. CSS value like "100px", "20%", "auto".
    /// </summary>
    string? Width { get; set; }

    /// <summary>
    /// Column minimum width. CSS value like "100px".
    /// </summary>
    string? MinWidth { get; set; }

    /// <summary>
    /// Column maximum width. CSS value like "300px".
    /// </summary>
    string? MaxWidth { get; set; }

    /// <summary>
    /// Text alignment for the column.
    /// Default is Start (left).
    /// </summary>
    Alignment Alignment { get; set; }

    /// <summary>
    /// Format string for displaying values (e.g., "N2", "d", "C").
    /// </summary>
    string? Format { get; set; }

    /// <summary>
    /// Renders the cell content for the given item.
    /// </summary>
    string Render(TEntity item);
}
