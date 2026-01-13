using Craft.Core;
using Craft.Domain;
using Craft.QuerySpec;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Components;

/// <summary>
/// A modern, production-ready data grid component for Blazor applications.
/// Integrates with IHttpService for data loading and supports pagination, sorting, filtering, and CRUD operations.
/// </summary>
/// <typeparam name="TEntity">The entity type to display in the grid.</typeparam>
public partial class CraftDataGrid<TEntity> : ICraftDataGrid<TEntity>
    where TEntity : class, IEntity, IModel, new()
{
    #region Private Fields

    private bool _isLoading;
    private bool _hasError;
    private string? _errorMessage;
    private string? _searchString;
    private List<TEntity> _items = [];
    private int _currentPage = 1;
    private int _pageSize = 10;
    private long _totalCount;
    private TEntity? _itemToDelete;
    private bool _showDeleteDialog;
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, MaxWidth = MaxWidth.Small };
    private string? _currentSortColumn;
    private GridSortDirection _currentSortDirection = GridSortDirection.None;
    private MudTable<TEntity>? _table;
    private CancellationTokenSource? _cts;

    #endregion

    #region Computed Properties for Declarative UI

    /// <summary>
    /// Determines if the title should be shown.
    /// </summary>
    private bool _showTitle => !string.IsNullOrWhiteSpace(Title);

    /// <summary>
    /// Determines if the add button should be shown.
    /// </summary>
    private bool _showAddButton => AllowAdd && OnAdd.HasDelegate;

    /// <summary>
    /// Determines if the export menu should be shown.
    /// </summary>
    private bool _showExportMenu => AllowExport && OnExport.HasDelegate;

    /// <summary>
    /// Determines if the actions column should be shown.
    /// </summary>
    private bool _showActionsColumn => ShowActions && (AllowEdit || AllowDelete || AllowView);

    /// <summary>
    /// Determines if any items are loaded.
    /// </summary>
    private bool _hasItems => _items?.Count > 0;

    #endregion

    #region Injected Services

    [Inject] private ISnackbar? Snackbar { get; set; }

    #endregion

    #region Parameters - Data Source

    /// <summary>
    /// The HTTP service used to load data from the API.
    /// Required parameter.
    /// </summary>
    [Parameter, EditorRequired] public IHttpService<TEntity>? HttpService { get; set; }

    /// <summary>
    /// Optional custom query builder function to customize the data query.
    /// Use this to add custom filters, includes, or other query specifications.
    /// </summary>
    [Parameter] public Func<Query<TEntity>, Query<TEntity>>? QueryBuilder { get; set; }

    /// <summary>
    /// Child content containing column definitions.
    /// </summary>
    //[Parameter] public new RenderFragment? ChildContent { get; set; }

    #endregion

    #region Parameters - Appearance

    /// <summary>
    /// Title displayed in the toolbar.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Custom CSS class applied to the table.
    /// </summary>
    // [Parameter] public new string? Class { get; set; }

    /// <summary>
    /// Custom inline styles applied to the table.
    /// </summary>
    //[Parameter] public new string? Style { get; set; }

    /// <summary>
    /// Enables row hover effect.
    /// Default is true.
    /// </summary>
    [Parameter] public bool Hover { get; set; } = true;

    /// <summary>
    /// Enables striped rows.
    /// Default is true.
    /// </summary>
    [Parameter] public bool Striped { get; set; } = true;

    /// <summary>
    /// Enables dense padding for rows.
    /// Default is false.
    /// </summary>
    [Parameter] public bool Dense { get; set; }

    /// <summary>
    /// Enables borders around table cells.
    /// Default is false.
    /// </summary>
    [Parameter] public bool Bordered { get; set; }

    /// <summary>
    /// Fixes the header while scrolling.
    /// Default is false.
    /// </summary>
    [Parameter] public bool FixedHeader { get; set; }

    /// <summary>
    /// Height of the table container. Use CSS values like "400px" or "50vh".
    /// Only applies when FixedHeader is true.
    /// </summary>
    [Parameter] public string? Height { get; set; }

    /// <summary>
    /// Color of the loading progress bar.
    /// Default is Primary.
    /// </summary>
    [Parameter] public Color LoadingProgressColor { get; set; } = Color.Primary;

    #endregion

    #region Parameters - Pagination

    /// <summary>
    /// Enables pagination.
    /// Default is true.
    /// </summary>
    [Parameter] public bool EnablePagination { get; set; } = true;

    /// <summary>
    /// Initial page size.
    /// Default is 10.
    /// </summary>
    [Parameter] public int InitialPageSize { get; set; } = 10;

    /// <summary>
    /// Available page size options.
    /// </summary>
    [Parameter] public int[] PageSizeOptions { get; set; } = [10, 25, 50, 100];

    /// <summary>
    /// Text displayed before the page size dropdown.
    /// Default is "Rows per page:".
    /// </summary>
    [Parameter] public string RowsPerPageString { get; set; } = "Rows per page:";

    /// <summary>
    /// Format string for pagination info. Use {first_item}, {last_item}, and {all_items} as placeholders.
    /// Default is "{first_item}-{last_item} of {all_items}".
    /// </summary>
    [Parameter] public string InfoFormat { get; set; } = "{first_item}-{last_item} of {all_items}";

    #endregion

    #region Parameters - Features

    /// <summary>
    /// Shows the action column.
    /// Default is true.
    /// </summary>
    [Parameter] public bool ShowActions { get; set; } = true;

    /// <summary>
    /// Enables the search box in the toolbar.
    /// Default is true.
    /// </summary>
    [Parameter] public bool ShowSearch { get; set; } = true;

    /// <summary>
    /// Placeholder text for the search box.
    /// Default is "Search...".
    /// </summary>
    [Parameter] public string SearchPlaceholder { get; set; } = "Search...";

    /// <summary>
    /// Enables the refresh button.
    /// Default is true.
    /// </summary>
    [Parameter] public bool AllowRefresh { get; set; } = true;

    /// <summary>
    /// Enables the add button and functionality.
    /// Default is false.
    /// </summary>
    [Parameter] public bool AllowAdd { get; set; }

    /// <summary>
    /// Text displayed on the add button.
    /// Default is "Add New".
    /// </summary>
    [Parameter] public string AddButtonText { get; set; } = "Add New";

    /// <summary>
    /// Icon displayed on the add button.
    /// Default is the MudBlazor Add icon.
    /// </summary>
    [Parameter] public string AddIcon { get; set; } = Icons.Material.Filled.Add;

    /// <summary>
    /// Enables the view action button.
    /// Default is false.
    /// </summary>
    [Parameter] public bool AllowView { get; set; }

    /// <summary>
    /// Enables the edit action button.
    /// Default is false.
    /// </summary>
    [Parameter] public bool AllowEdit { get; set; }

    /// <summary>
    /// Enables the delete action button.
    /// Default is false.
    /// </summary>
    [Parameter] public bool AllowDelete { get; set; }

    /// <summary>
    /// Enables export functionality.
    /// Default is false.
    /// </summary>
    [Parameter] public bool AllowExport { get; set; }

    #endregion

    #region Parameters - Messages

    /// <summary>
    /// Message displayed when no records are found.
    /// Default is "No records to display".
    /// </summary>
    [Parameter] public string NoRecordsMessage { get; set; } = "No records to display";

    /// <summary>
    /// Message displayed while loading data.
    /// Default is "Loading...".
    /// </summary>
    [Parameter] public string LoadingMessage { get; set; } = "Loading...";

    /// <summary>
    /// Message displayed in the delete confirmation dialog.
    /// Default is "Are you sure you want to delete this record? This action cannot be undone.".
    /// </summary>
    [Parameter] public string DeleteConfirmationMessage { get; set; } 
        = "Are you sure you want to delete this record? This action cannot be undone.";

    #endregion

    #region Parameters - Events

    /// <summary>
    /// Callback invoked when the add button is clicked.
    /// </summary>
    [Parameter] public EventCallback OnAdd { get; set; }

    /// <summary>
    /// Callback invoked when the view button is clicked for a row.
    /// </summary>
    [Parameter] public EventCallback<TEntity> OnView { get; set; }

    /// <summary>
    /// Callback invoked when the edit button is clicked for a row.
    /// </summary>
    [Parameter] public EventCallback<TEntity> OnEdit { get; set; }

    /// <summary>
    /// Callback invoked when a row is confirmed for deletion.
    /// Return true if deletion was successful, false otherwise.
    /// </summary>
    [Parameter] public EventCallback<TEntity> OnDelete { get; set; }

    /// <summary>
    /// Callback invoked when export is requested.
    /// Receives the export format as a parameter.
    /// </summary>
    [Parameter] public EventCallback<ExportFormat> OnExport { get; set; }

    /// <summary>
    /// Callback invoked when data is successfully loaded.
    /// Receives the page response as a parameter.
    /// </summary>
    [Parameter] public EventCallback<PageResponse<TEntity>> OnDataLoaded { get; set; }

    #endregion

    #region Public Properties

    /// <summary>
    /// List of columns in the grid.
    /// </summary>
    public List<ICraftDataGridColumn<TEntity>> Columns { get; } = [];

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int CurrentPage => _currentPage;

    /// <summary>
    /// Current page size.
    /// </summary>
    public int PageSize => _pageSize;

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public long TotalCount => _totalCount;

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(_totalCount / (double)_pageSize);

    /// <summary>
    /// Currently loaded items in the grid.
    /// </summary>
    public IReadOnlyList<TEntity> Items => _items.AsReadOnly();

    #endregion

    #region Lifecycle Methods

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _pageSize = InitialPageSize;

        if (HttpService is null)
            throw new InvalidOperationException($"{nameof(HttpService)} parameter is required for {nameof(CraftDataGrid<>)}.");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
            await LoadDataAsync();
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        // Don't cancel active requests during disposal to avoid errors when navigating away
        // (e.g., browser back button). Just dispose the token source cleanly.
        // The HTTP pipeline will handle cleanup of in-flight requests.
        _cts?.Dispose();
        await base.DisposeAsyncCore();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a column to the grid.
    /// </summary>
    public void AddColumn(ICraftDataGridColumn<TEntity> column)
    {
        ArgumentNullException.ThrowIfNull(column);

        Columns.Add(column);
        StateHasChanged();
    }

    /// <summary>
    /// Removes a column from the grid.
    /// </summary>
    public void RemoveColumn(ICraftDataGridColumn<TEntity> column)
    {
        ArgumentNullException.ThrowIfNull(column);

        Columns.Remove(column);
        StateHasChanged();
    }

    /// <summary>
    /// Refreshes the grid data.
    /// </summary>
    public new async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// Navigates to a specific page.
    /// </summary>
    public async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages)
            return;

        _currentPage = page;
        await LoadDataAsync();
    }

    /// <summary>
    /// Changes the page size and resets to the first page.
    /// </summary>
    public async Task ChangePageSizeAsync(int pageSize)
    {
        if (pageSize < 1)
            return;

        _pageSize = pageSize;
        _currentPage = 1;
        await LoadDataAsync();
    }

    #endregion

    #region Private Methods - Data Loading

    private async Task LoadDataAsync()
    {
        if (HttpService is null)
            return;

        // Cancel previous request if still running
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            _isLoading = true;
            _hasError = false;
            _errorMessage = null;
            await InvokeAsync(StateHasChanged);

            // Build the query
            var query = BuildQuery();

            // Execute the query
            var result = await HttpService.GetPagedListAsync(query, _cts.Token);

            if (result.Success && result.Data is not null)
            {
                _items = [.. result.Data.Items];
                _totalCount = result.Data.TotalCount;
                _currentPage = result.Data.CurrentPage;
                _pageSize = result.Data.PageSize;

                // Invoke the OnDataLoaded callback
                if (OnDataLoaded.HasDelegate)
                    await OnDataLoaded.InvokeAsync(result.Data);
            }
            else
            {
                _hasError = true;
                _errorMessage = result.Errors?.FirstOrDefault() ?? "Failed to load data.";
                
                Snackbar?.Add(_errorMessage, Severity.Error);
            }
        }
        catch (OperationCanceledException)
        {
            // Request was cancelled - this is normal when a new request supersedes an old one
        }
        catch (Exception ex)
        {
            _hasError = true;
            _errorMessage = $"An error occurred while loading data: {ex.Message}";
            
            Snackbar?.Add(_errorMessage, Severity.Error);
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private Query<TEntity> BuildQuery()
    {
        var query = new Query<TEntity>();

        // Set pagination
        query.SetPage(_currentPage, _pageSize);

        // Apply search if provided
        if (!string.IsNullOrWhiteSpace(_searchString))
            ApplySearch(query, _searchString);

        // Apply sorting from columns
        ApplySorting(query);

        // Apply custom query builder if provided
        if (QueryBuilder is not null)
            query = QueryBuilder(query);

        return query;
    }

    private void ApplySearch(Query<TEntity> query, string searchTerm)
    {
        var searchableColumns = Columns.Where(c => c.Searchable && c.PropertyExpression is not null).ToList();
        
        if (searchableColumns.Count == 0)
            return;

        foreach (var column in searchableColumns)
            if (column.PropertyExpression is not null)
                query.Search(column.PropertyExpression, searchTerm);
    }

    private void ApplySorting(Query<TEntity> query)
    {
        // First, apply user-initiated sorting if present
        if (!string.IsNullOrWhiteSpace(_currentSortColumn) && _currentSortDirection != GridSortDirection.None)
        {
            var sortColumn = Columns.FirstOrDefault(c => c.PropertyName == _currentSortColumn);
            
            if (sortColumn?.PropertyExpression is not null)
            {
                if (_currentSortDirection == GridSortDirection.Descending)
                    query.OrderByDescending(sortColumn.PropertyExpression);
                else
                    query.OrderBy(sortColumn.PropertyExpression);
                
                return; // User sorting takes precedence, skip default sorting
            }
        }

        // If no user sorting, apply default sorting from columns
        var sortableColumns = Columns
            .Where(c => c.Sortable && c.DefaultSort is not null && c.PropertyExpression is not null)
            .OrderBy(c => c.SortOrder)
            .ToList();

        foreach (var column in sortableColumns)
        {
            if (column.PropertyExpression is null)
                continue;

            if (column.DefaultSort == GridSortDirection.Descending)
                query.OrderByDescending(column.PropertyExpression);
            else
                query.OrderBy(column.PropertyExpression);
        }
    }

    #endregion

    #region Private Methods - Event Handlers

    private async Task SearchAsync(string searchValue)
    {
        _searchString = searchValue;
        _currentPage = 1; // Reset to first page when searching
        await LoadDataAsync();
    }

    private async Task HandleSortChangedAsync(string? sortLabel, MudBlazor.SortDirection direction)
    {
        if (string.IsNullOrWhiteSpace(sortLabel))
            return;

        // Update current sort state
        _currentSortColumn = sortLabel;
        _currentSortDirection = direction switch
        {
            MudBlazor.SortDirection.Ascending => GridSortDirection.Ascending,
            MudBlazor.SortDirection.Descending => GridSortDirection.Descending,
            _ => GridSortDirection.None
        };

        // Reset to first page when sorting changes
        _currentPage = 1;
        
        // Reload data with new sorting
        await LoadDataAsync();
    }

    private async Task HandleAddAsync()
    {
        if (OnAdd.HasDelegate)
            await OnAdd.InvokeAsync();
    }

    private async Task HandleViewAsync(TEntity item)
    {
        if (OnView.HasDelegate)
            await OnView.InvokeAsync(item);
    }

    private async Task HandleEditAsync(TEntity item)
    {
        if (OnEdit.HasDelegate)
            await OnEdit.InvokeAsync(item);
    }

    private Task HandleDeleteAsync(TEntity item)
    {
        _itemToDelete = item;
        _showDeleteDialog = true;
        return Task.CompletedTask;
    }

    private async Task ConfirmDeleteAsync()
    {
        _showDeleteDialog = false;

        if (_itemToDelete is null || !OnDelete.HasDelegate)
            return;

        try
        {
            await OnDelete.InvokeAsync(_itemToDelete);
            
            Snackbar?.Add("Record deleted successfully", Severity.Success);
            
            // Refresh the grid
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Snackbar?.Add($"Failed to delete record: {ex.Message}", Severity.Error);
        }
        finally
        {
            _itemToDelete = null;
        }
    }

    private Task CancelDeleteAsync()
    {
        _showDeleteDialog = false;
        _itemToDelete = null;
        return Task.CompletedTask;
    }

    private async Task HandleExportAsync(ExportFormat format)
    {
        if (OnExport.HasDelegate)
            await OnExport.InvokeAsync(format);
    }

    #endregion

    #region Private Methods - Styling

    private static string? GetColumnStyle(ICraftDataGridColumn<TEntity> column)
    {
        var styles = new List<string>();

        if (!string.IsNullOrWhiteSpace(column.Width))
            styles.Add($"width: {column.Width}");

        if (!string.IsNullOrWhiteSpace(column.MinWidth))
            styles.Add($"min-width: {column.MinWidth}");

        if (!string.IsNullOrWhiteSpace(column.MaxWidth))
            styles.Add($"max-width: {column.MaxWidth}");

        return styles.Count != 0 ? string.Join("; ", styles) : null;
    }

    private static string? GetCellStyle(ICraftDataGridColumn<TEntity> column)
    {
        var styles = new List<string>();

        if (column.Alignment != Alignment.Start)
            styles.Add($"text-align: {column.Alignment.ToString().ToLowerInvariant()}");

        return styles.Count != 0 ? string.Join("; ", styles) : null;
    }

    private MudBlazor.SortDirection GetSortDirection(ICraftDataGridColumn<TEntity> column)
    {
        // If this column is the currently sorted column, use the current sort direction
        if (_currentSortColumn == column.PropertyName)
        {
            return _currentSortDirection switch
            {
                GridSortDirection.Ascending => MudBlazor.SortDirection.Ascending,
                GridSortDirection.Descending => MudBlazor.SortDirection.Descending,
                _ => MudBlazor.SortDirection.None
            };
        }

        // Otherwise, use the default sort direction (typically None for initial display)
        return MudBlazor.SortDirection.None;
    }

    #endregion
}

/// <summary>
/// Represents the export format options.
/// </summary>
public enum ExportFormat
{
    Csv,
    Excel,
    Pdf
}

/// <summary>
/// Represents column text alignment options.
/// </summary>
public enum Alignment
{
    Start,
    Center,
    End
}

/// <summary>
/// Represents sort direction options.
/// </summary>
public enum GridSortDirection
{
    None,
    Ascending,
    Descending
}
