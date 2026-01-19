using Craft.QuerySpec;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Components;

/// <summary>
/// Advanced search component that manages filters for data grids.
/// </summary>
/// <typeparam name="TEntity">The entity type being filtered.</typeparam>
public partial class CraftAdvancedSearch<TEntity> : ComponentBase
    where TEntity : class
{
    #region Injected Services

    [Inject] private IDialogService DialogService { get; set; } = null!;

    #endregion

    #region Parameters

    /// <summary>
    /// List of searchable columns to choose from.
    /// </summary>
    [Parameter, EditorRequired] public List<ICraftDataGridColumn<TEntity>> SearchableColumns { get; set; } = [];

    /// <summary>
    /// The filter builder containing all active filters.
    /// </summary>
    [Parameter] public EntityFilterBuilder<TEntity> FilterBuilder { get; set; } = new();

    /// <summary>
    /// Callback invoked when the FilterBuilder changes.
    /// </summary>
    [Parameter] public EventCallback<EntityFilterBuilder<TEntity>> FilterBuilderChanged { get; set; }

    /// <summary>
    /// Callback invoked when filters are changed (added, removed, or cleared).
    /// </summary>
    [Parameter] public EventCallback<EntityFilterBuilder<TEntity>> OnFiltersChanged { get; set; }

    #endregion

    #region Methods

    private async Task OpenFilterDialog()
    {
        var parameters = new DialogParameters<CraftFilterBuilder<TEntity>>
        {
            { x => x.SearchableColumns, SearchableColumns }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseOnEscapeKey = true
        };

        var dialog = await DialogService.ShowAsync<CraftFilterBuilder<TEntity>>("Add Filter", parameters, options);

        var result = await dialog.Result;

        if (!(result?.Canceled ?? true) && result.Data is FilterCriteria criteria)
            await HandleFilterAddedAsync(criteria);
    }

    private async Task HandleFilterAddedAsync(FilterCriteria criteria)
    {
        // Add the filter to the builder with metadata
        FilterBuilder.Add(criteria);

        // Notify parent of the change
        await FilterBuilderChanged.InvokeAsync(FilterBuilder);
        await OnFiltersChanged.InvokeAsync(FilterBuilder);
    }

    private async Task RemoveFilterAsync(FilterCriteria criteria)
    {
        // Find and remove the matching filter using metadata
        var toRemove = FilterBuilder.EntityFilterList
            .FirstOrDefault(f => f.Metadata is not null 
                              && f.Metadata.Name == criteria.Name 
                              && Equals(f.Metadata.Value, criteria.Value) 
                              && f.Metadata.Comparison == criteria.Comparison);

        if (toRemove is not null)
        {
            FilterBuilder.EntityFilterList.Remove(toRemove);

            // Notify parent of the change
            await FilterBuilderChanged.InvokeAsync(FilterBuilder);
            await OnFiltersChanged.InvokeAsync(FilterBuilder);
        }
    }

    private async Task ClearAllFiltersAsync()
    {
        FilterBuilder.Clear();

        // Notify parent of the change
        await FilterBuilderChanged.InvokeAsync(FilterBuilder);
        await OnFiltersChanged.InvokeAsync(FilterBuilder);
    }

    #endregion
}
