using Craft.QuerySpec;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Components;

/// <summary>
/// Reusable toolbar component for data grids and card grids.
/// Provides inline layout with filter chips, export, refresh, and add functionality.
/// </summary>
/// <typeparam name="TEntity">The entity type being displayed.</typeparam>
public partial class CraftDataToolbar<TEntity> : ComponentBase
    where TEntity : class
{
    #region Parameters

    /// <summary>
    /// Whether to show the search/filter section.
    /// </summary>
    [Parameter] public bool ShowSearch { get; set; }

    /// <summary>
    /// List of searchable columns for filtering.
    /// </summary>
    [Parameter] public List<ICraftDataGridColumn<TEntity>> SearchableColumns { get; set; } = [];

    /// <summary>
    /// The filter builder containing active filters.
    /// </summary>
    [Parameter] public EntityFilterBuilder<TEntity> FilterBuilder { get; set; } = new();

    /// <summary>
    /// Callback when filter builder changes.
    /// </summary>
    [Parameter] public EventCallback<EntityFilterBuilder<TEntity>> FilterBuilderChanged { get; set; }

    /// <summary>
    /// Callback when filters are changed.
    /// </summary>
    [Parameter] public EventCallback<EntityFilterBuilder<TEntity>> OnFiltersChanged { get; set; }

    /// <summary>
    /// Whether to show the export menu.
    /// </summary>
    [Parameter] public bool ShowExport { get; set; }

    /// <summary>
    /// Callback when export is requested.
    /// </summary>
    [Parameter] public EventCallback<ExportFormat> OnExport { get; set; }

    /// <summary>
    /// Whether to show the sort menu.
    /// </summary>
    [Parameter] public bool ShowSort { get; set; }

    /// <summary>
    /// Custom sort menu content (for CardGrid).
    /// </summary>
    [Parameter] public RenderFragment? SortContent { get; set; }

    /// <summary>
    /// Whether to show the refresh button.
    /// </summary>
    [Parameter] public bool ShowRefresh { get; set; }

    /// <summary>
    /// Callback when refresh is clicked.
    /// </summary>
    [Parameter] public EventCallback OnRefresh { get; set; }

    /// <summary>
    /// Whether to show the add button.
    /// </summary>
    [Parameter] public bool ShowAdd { get; set; }

    /// <summary>
    /// Text for the add button.
    /// </summary>
    [Parameter] public string AddButtonText { get; set; } = "Add";

    /// <summary>
    /// Icon for the add button.
    /// </summary>
    [Parameter] public string AddIcon { get; set; } = Icons.Material.Filled.Add;

    /// <summary>
    /// Callback when add is clicked.
    /// </summary>
    [Parameter] public EventCallback OnAdd { get; set; }

    #endregion
}
