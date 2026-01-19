using Craft.Core;
using Craft.Domain;
using Craft.QuerySpec;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Linq.Expressions;

namespace Craft.UiBuilders.Components;

/// <summary>
/// A modern, production-ready card grid component for Blazor applications.
/// Displays data in card format, integrates with IHttpService for data loading and supports pagination, sorting, filtering, and CRUD operations.
/// </summary>
/// <typeparam name="TEntity">The entity type to display in the card grid.</typeparam>
public partial class CraftCardGrid<TEntity> : ICraftCardGrid<TEntity>
    where TEntity : class, IEntity, IModel, new()
{
    #region Private Fields

    private bool _isLoading;
    private bool _hasError;
    private string? _errorMessage;
    private EntityFilterBuilder<TEntity> _filterBuilder = new();
    private List<TEntity> _items = [];
    private int _currentPage = 1;
    private int _pageSize = 12;
    private long _totalCount;
    private TEntity? _itemToDelete;
    private bool _showDeleteDialog;
    private CancellationTokenSource? _cts;
    private ICraftCardGridColumn<TEntity>? _currentSortColumn;
    private GridSortDirection _currentSortDirection = GridSortDirection.None;
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, MaxWidth = MaxWidth.Small };

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
    /// Determines if actions should be shown in cards.
    /// </summary>
    private bool _showActionsInCard => ShowActions && (AllowEdit || AllowDelete || AllowView);

    /// <summary>
    /// Determines if the sort menu should be shown.
    /// </summary>
    private bool _showSortMenu => Columns.Any(c => c.Sortable && c.Visible);

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
    // [Parameter] public new RenderFragment? ChildContent { get; set; }

    #endregion

    #region Parameters - Appearance

    /// <summary>
    /// Title displayed in the toolbar.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Custom CSS class applied to the container.
    /// </summary>
    // [Parameter] public new string? Class { get; set; }

    /// <summary>
    /// Custom CSS class applied to each card.
    /// </summary>
    [Parameter] public string? CardClass { get; set; }

    /// <summary>
    /// Custom inline styles applied to each card.
    /// </summary>
    [Parameter] public string? CardStyle { get; set; }

    /// <summary>
    /// Custom CSS class applied to the card grid container.
    /// </summary>
    [Parameter] public string? CardGridClass { get; set; }

    /// <summary>
    /// Elevation of cards (0-25).
    /// Default is 2.
    /// </summary>
    [Parameter] public int CardElevation { get; set; } = 2;

    /// <summary>
    /// Spacing between cards (0-16).
    /// Default is 3.
    /// </summary>
    [Parameter] public int CardSpacing { get; set; } = 3;

    /// <summary>
    /// Number of columns on extra small screens.
    /// Default is 12 (full width).
    /// </summary>
    [Parameter] public int CardXs { get; set; } = 12;

    /// <summary>
    /// Number of columns on small screens.
    /// Default is 6 (2 cards per row).
    /// </summary>
    [Parameter] public int CardSm { get; set; } = 6;

    /// <summary>
    /// Number of columns on medium screens.
    /// Default is 4 (3 cards per row).
    /// </summary>
    [Parameter] public int CardMd { get; set; } = 4;

    /// <summary>
    /// Number of columns on large screens.
    /// Default is 3 (4 cards per row).
    /// </summary>
    [Parameter] public int CardLg { get; set; } = 3;

    /// <summary>
    /// Number of columns on extra large screens.
    /// Default is 3 (4 cards per row).
    /// </summary>
    [Parameter] public int CardXl { get; set; } = 3;

    #endregion

    #region Parameters - Pagination

    /// <summary>
    /// Enables pagination.
    /// Default is true.
    /// </summary>
    [Parameter] public bool EnablePagination { get; set; } = true;

    /// <summary>
    /// Initial page size.
    /// Default is 12.
    /// </summary>
    [Parameter] public int InitialPageSize { get; set; } = 12;

    /// <summary>
    /// Format string for pagination info. Use {first_item}, {last_item}, and {all_items} as placeholders.
    /// Default is "Showing {first_item}-{last_item} of {all_items}".
    /// </summary>
    [Parameter] public string PaginationInfoFormat { get; set; } = "Showing {first_item}-{last_item} of {all_items}";

    #endregion

    #region Parameters - Features

    /// <summary>
    /// Shows the action buttons in cards.
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
    /// Callback invoked when the view button is clicked for a card.
    /// </summary>
    [Parameter] public EventCallback<TEntity> OnView { get; set; }

    /// <summary>
    /// Callback invoked when the edit button is clicked for a card.
    /// </summary>
    [Parameter] public EventCallback<TEntity> OnEdit { get; set; }

    /// <summary>
    /// Callback invoked when a card is confirmed for deletion.
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
    /// List of columns in the card grid.
    /// </summary>
    public List<ICraftCardGridColumn<TEntity>> Columns { get; } = [];

    /// <summary>
    /// List of searchable columns available for filtering.
    /// For CardGrid, we create minimal wrappers to satisfy the ICraftDataGridColumn interface.
    /// </summary>
    public List<ICraftDataGridColumn<TEntity>> SearchableColumns =>
        [.. Columns
            .Where(c => c.Searchable && c.PropertyExpression is not null)
            .Select<ICraftCardGridColumn<TEntity>, ICraftDataGridColumn<TEntity>>(c => new CardColumnWrapper(c))];

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int CurrentPage => _currentPage;

    #endregion

    #region Helper Class

    /// <summary>
    /// Minimal wrapper to adapt CardGrid column to DataGrid column interface for filtering.
    /// </summary>
    private class CardColumnWrapper : ICraftDataGridColumn<TEntity>
    {
        private readonly ICraftCardGridColumn<TEntity> _source;

        public CardColumnWrapper(ICraftCardGridColumn<TEntity> source) => _source = source;

        // Core properties needed for filtering
        public Expression<Func<TEntity, object>>? PropertyExpression { get => _source.PropertyExpression; set { } }
        public string? PropertyName => _source.PropertyName;
        public Type? PropertyType => (_source as CraftCardGridColumn<TEntity>)?.PropertyType;
        public string Title { get => _source.Caption; set { } }
        public bool Searchable { get => true; set { } }
        public bool Visible { get => true; set { } }
        public bool Sortable { get => false; set { } }
        
        // Not used for filtering
        public ICraftDataGrid<TEntity>? DataGrid { get => null; set { } }
        public RenderFragment<TEntity>? Template { get => null; set { } }
        public GridSortDirection? DefaultSort { get => null; set { } }
        public int SortOrder { get => 0; set { } }
        public string? Width { get => null; set { } }
        public string? MinWidth { get => null; set { } }
        public string? MaxWidth { get => null; set { } }
        public Alignment Alignment { get => Alignment.Start; set { } }
        public string? Format { get => null; set { } }
        public string Render(TEntity entity) => string.Empty;
    }

    #endregion

    #region Public Properties Continued

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
    /// Currently loaded items in the card grid.
    /// </summary>
    public IReadOnlyList<TEntity> Items => _items.AsReadOnly();

    #endregion

    #region Lifecycle Methods

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _pageSize = InitialPageSize;

        if (HttpService is null)
            throw new InvalidOperationException($"{nameof(HttpService)} parameter is required for {nameof(CraftCardGrid<>)}.");
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
    /// Adds a column to the card grid.
    /// </summary>
    public void AddColumn(ICraftCardGridColumn<TEntity> column)
    {
        ArgumentNullException.ThrowIfNull(column);

        Columns.Add(column);
        StateHasChanged();
    }

    /// <summary>
    /// Removes a column from the card grid.
    /// </summary>
    public void RemoveColumn(ICraftCardGridColumn<TEntity> column)
    {
        ArgumentNullException.ThrowIfNull(column);

        Columns.Remove(column);
        StateHasChanged();
    }

    /// <summary>
    /// Refreshes the card grid data.
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

        // Cancel any pending operation
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        var cancellationToken = _cts.Token;

        try
        {
            _isLoading = true;
            _hasError = false;
            _errorMessage = null;
            await InvokeAsync(StateHasChanged);

            // Build the query
            var query = BuildQuery();

            // Execute the query
            var result = await HttpService.GetPagedListAsync(query, cancellationToken);

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
            // Operation was cancelled, ignore
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

        // Apply advanced filters if provided
        ApplyFilters(query);

        // Apply sorting
        ApplySorting(query);

        // Apply custom query builder if provided
        if (QueryBuilder is not null)
            query = QueryBuilder(query);

        return query;
    }

    private void ApplyFilters(Query<TEntity> query)
    {
        if (_filterBuilder.Count == 0 || query.EntityFilterBuilder is null)
            return;

        // Copy filters from our builder to the query's builder
        foreach (var filterCriteria in _filterBuilder.EntityFilterList)
            query.EntityFilterBuilder.Add(filterCriteria);
    }

    private void ApplySorting(Query<TEntity> query)
    {
        // Apply current sort if set
        if (_currentSortColumn is not null && _currentSortColumn.PropertyExpression is not null)
        {
            if (_currentSortDirection == GridSortDirection.Descending)
                query.OrderByDescending(_currentSortColumn.PropertyExpression);
            else if (_currentSortDirection == GridSortDirection.Ascending)
                query.OrderBy(_currentSortColumn.PropertyExpression);
        }
        else
        {
            // Apply default sorting from columns
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
    }

    #endregion

    #region Private Methods - Event Handlers

    private async Task HandleFiltersChangedAsync(EntityFilterBuilder<TEntity> filterBuilder)
    {
        _filterBuilder = filterBuilder;
        _currentPage = 1; // Reset to first page when filters change
        await LoadDataAsync();
    }

    private async Task ToggleSortAsync(ICraftCardGridColumn<TEntity> column)
    {
        // Compare by PropertyName instead of reference equality
        if (_currentSortColumn is not null && _currentSortColumn.PropertyName == column.PropertyName)
        {
            // Toggle sort direction
            _currentSortDirection = _currentSortDirection switch
            {
                GridSortDirection.None => GridSortDirection.Ascending,
                GridSortDirection.Ascending => GridSortDirection.Descending,
                GridSortDirection.Descending => GridSortDirection.None,
                _ => GridSortDirection.Ascending
            };

            if (_currentSortDirection == GridSortDirection.None)
                _currentSortColumn = null;
        }
        else
        {
            // Set new sort column
            _currentSortColumn = column;
            _currentSortDirection = GridSortDirection.Ascending;
        }

        _currentPage = 1; // Reset to first page when sorting
        await LoadDataAsync();
    }

    private async Task OnPageChangedAsync(int page)
    {
        _currentPage = page;
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

            // Refresh the card grid
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

    #region Private Methods - UI Helpers

    private string GetPaginationInfo()
    {
        if (_totalCount == 0)
            return "No records";

        var firstItem = (_currentPage - 1) * _pageSize + 1;
        var lastItem = Math.Min(_currentPage * _pageSize, _totalCount);

        return PaginationInfoFormat
            .Replace("{first_item}", firstItem.ToString())
            .Replace("{last_item}", lastItem.ToString())
            .Replace("{all_items}", _totalCount.ToString());
    }

    #endregion
}
