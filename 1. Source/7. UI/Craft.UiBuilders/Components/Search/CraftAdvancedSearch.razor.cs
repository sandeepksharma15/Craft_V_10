using Craft.UiBuilders.Models;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components;

/// <summary>
/// Reusable advanced search component with filter builder functionality.
/// </summary>
/// <typeparam name="TEntity">The entity type being filtered.</typeparam>
public partial class CraftAdvancedSearch<TEntity> : ComponentBase
    where TEntity : class
{
    private bool _showFilterBuilder;

    #region Parameters

    /// <summary>
    /// List of searchable columns available for filtering.
    /// </summary>
    [Parameter, EditorRequired]
    public List<ICraftDataGridColumn<TEntity>> SearchableColumns { get; set; } = [];

    /// <summary>
    /// List of currently applied filters.
    /// </summary>
    [Parameter]
    public List<FilterModel> Filters { get; set; } = [];

    /// <summary>
    /// Callback invoked when the filters list changes.
    /// </summary>
    [Parameter]
    public EventCallback<List<FilterModel>> FiltersChanged { get; set; }

    /// <summary>
    /// Callback invoked when filters are applied or removed.
    /// </summary>
    [Parameter]
    public EventCallback<List<FilterModel>> OnFiltersChanged { get; set; }

    #endregion

    #region Methods

    private void OpenFilterBuilder()
    {
        _showFilterBuilder = true;
    }

    private async Task HandleFilterAdded(FilterModel filter)
    {
        Filters.Add(filter);
        await NotifyFiltersChanged();
    }

    private async Task RemoveFilter(FilterModel filter)
    {
        Filters.Remove(filter);

        // Update logical operators for remaining filters
        if (Filters.Count > 0)
            Filters[0].LogicalOperator = null;

        await NotifyFiltersChanged();
    }

    private async Task ClearAllFilters()
    {
        Filters.Clear();
        await NotifyFiltersChanged();
    }

    private async Task NotifyFiltersChanged()
    {
        await FiltersChanged.InvokeAsync(Filters);
        await OnFiltersChanged.InvokeAsync(Filters);
    }

    #endregion
}
