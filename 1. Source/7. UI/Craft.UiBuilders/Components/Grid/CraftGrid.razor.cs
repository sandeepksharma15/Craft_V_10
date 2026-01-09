using Craft.Core;
using Craft.Domain;
using Craft.QuerySpec;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Components;

/// <summary>
/// A responsive grid component that automatically switches between table (DataGrid) and card (CardGrid) views based on screen size.
/// This wrapper component reduces code duplication at the page level.
/// </summary>
/// <typeparam name="TEntity">The entity type to display in the grid.</typeparam>
public partial class CraftGrid<TEntity>
    where TEntity : class, IEntity, IModel, new()
{
    #region Parameters - Data Source

    /// <summary>
    /// The HTTP service used to load data from the API.
    /// Required parameter.
    /// </summary>
    [Parameter, EditorRequired] public IHttpService<TEntity>? HttpService { get; set; }

    /// <summary>
    /// Optional custom query builder function to customize the data query.
    /// </summary>
    [Parameter] public Func<Query<TEntity>, Query<TEntity>>? QueryBuilder { get; set; }

    #endregion

    #region Public Properties

    /// <summary>
    /// List of columns registered by CraftGridColumn components.
    /// </summary>
    public List<CraftGridColumn<TEntity>> Columns { get; } = [];

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a column to the grid.
    /// Called by CraftGridColumn during initialization.
    /// </summary>
    public void AddColumn(CraftGridColumn<TEntity> column)
    {
        ArgumentNullException.ThrowIfNull(column);
        Columns.Add(column);
    }

    #endregion

    #region Parameters - Responsive Behavior

    /// <summary>
    /// Breakpoint at which to switch between DataGrid and CardGrid.
    /// Default is Medium (Md) - shows cards on small/extra-small screens, table on medium and larger.
    /// </summary>
    [Parameter] public Breakpoint SwitchBreakpoint { get; set; } = Breakpoint.Md;

    /// <summary>
    /// When true, shows CardGrid on screens smaller than SwitchBreakpoint and DataGrid on larger screens.
    /// When false, shows DataGrid on screens smaller than SwitchBreakpoint and CardGrid on larger screens.
    /// Default is true (cards on small screens).
    /// </summary>
    [Parameter] public bool ShowCardViewOnSmallScreens { get; set; } = true;

    #endregion

    #region Parameters - Appearance

    /// <summary>
    /// Title displayed in the toolbar.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Custom CSS class applied to the component.
    /// </summary>
    [Parameter] public new string? Class { get; set; }

    #endregion

    #region Parameters - DataGrid Specific

    /// <summary>
    /// Enables row hover effect in DataGrid.
    /// Default is true.
    /// </summary>
    [Parameter] public bool Hover { get; set; } = true;

    /// <summary>
    /// Enables striped rows in DataGrid.
    /// Default is true.
    /// </summary>
    [Parameter] public bool Striped { get; set; } = true;

    /// <summary>
    /// Enables dense padding for rows in DataGrid.
    /// Default is false.
    /// </summary>
    [Parameter] public bool Dense { get; set; }

    /// <summary>
    /// Enables borders around table cells in DataGrid.
    /// Default is false.
    /// </summary>
    [Parameter] public bool Bordered { get; set; }

    /// <summary>
    /// Fixes the header while scrolling in DataGrid.
    /// Default is false.
    /// </summary>
    [Parameter] public bool FixedHeader { get; set; }

    /// <summary>
    /// Height of the table container in DataGrid.
    /// Only applies when FixedHeader is true.
    /// </summary>
    [Parameter] public string? Height { get; set; }

    /// <summary>
    /// Color of the loading progress bar in DataGrid.
    /// Default is Primary.
    /// </summary>
    [Parameter] public Color LoadingProgressColor { get; set; } = Color.Primary;

    /// <summary>
    /// Available page size options for DataGrid.
    /// </summary>
    [Parameter] public int[] PageSizeOptions { get; set; } = [10, 25, 50, 100];

    /// <summary>
    /// Text displayed before the page size dropdown in DataGrid.
    /// Default is "Rows per page:".
    /// </summary>
    [Parameter] public string RowsPerPageString { get; set; } = "Rows per page:";

    /// <summary>
    /// Format string for pagination info in DataGrid.
    /// Default is "{first_item}-{last_item} of {all_items}".
    /// </summary>
    [Parameter] public string InfoFormat { get; set; } = "{first_item}-{last_item} of {all_items}";

    #endregion

    #region Parameters - CardGrid Specific

    /// <summary>
    /// Custom CSS class applied to each card in CardGrid.
    /// </summary>
    [Parameter] public string? CardClass { get; set; }

    /// <summary>
    /// Custom inline styles applied to each card in CardGrid.
    /// </summary>
    [Parameter] public string? CardStyle { get; set; }

    /// <summary>
    /// Custom CSS class applied to the card grid container in CardGrid.
    /// </summary>
    [Parameter] public string? CardGridClass { get; set; }

    /// <summary>
    /// Elevation of cards in CardGrid (0-25).
    /// Default is 2.
    /// </summary>
    [Parameter] public int CardElevation { get; set; } = 2;

    /// <summary>
    /// Spacing between cards in CardGrid (0-16).
    /// Default is 3.
    /// </summary>
    [Parameter] public int CardSpacing { get; set; } = 3;

    /// <summary>
    /// Number of columns on extra small screens in CardGrid.
    /// Default is 12 (full width).
    /// </summary>
    [Parameter] public int CardXs { get; set; } = 12;

    /// <summary>
    /// Number of columns on small screens in CardGrid.
    /// Default is 6 (2 cards per row).
    /// </summary>
    [Parameter] public int CardSm { get; set; } = 6;

    /// <summary>
    /// Number of columns on medium screens in CardGrid.
    /// Default is 4 (3 cards per row).
    /// </summary>
    [Parameter] public int CardMd { get; set; } = 4;

    /// <summary>
    /// Number of columns on large screens in CardGrid.
    /// Default is 3 (4 cards per row).
    /// </summary>
    [Parameter] public int CardLg { get; set; } = 3;

    /// <summary>
    /// Number of columns on extra large screens in CardGrid.
    /// Default is 3 (4 cards per row).
    /// </summary>
    [Parameter] public int CardXl { get; set; } = 3;

    /// <summary>
    /// Format string for pagination info in CardGrid.
    /// Default is "Showing {first_item}-{last_item} of {all_items}".
    /// </summary>
    [Parameter] public string PaginationInfoFormat { get; set; } = "Showing {first_item}-{last_item} of {all_items}";

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

    #endregion

    #region Parameters - Features

    /// <summary>
    /// Shows the action column/buttons.
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
    /// Callback invoked when the view button is clicked.
    /// </summary>
    [Parameter] public EventCallback<TEntity> OnView { get; set; }

    /// <summary>
    /// Callback invoked when the edit button is clicked.
    /// </summary>
    [Parameter] public EventCallback<TEntity> OnEdit { get; set; }

    /// <summary>
    /// Callback invoked when an item is confirmed for deletion.
    /// </summary>
    [Parameter] public EventCallback<TEntity> OnDelete { get; set; }

    /// <summary>
    /// Callback invoked when export is requested.
    /// </summary>
    [Parameter] public EventCallback<ExportFormat> OnExport { get; set; }

    /// <summary>
    /// Callback invoked when data is successfully loaded.
    /// </summary>
    [Parameter] public EventCallback<PageResponse<TEntity>> OnDataLoaded { get; set; }

    #endregion

        #region Lifecycle Methods

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (HttpService is null)
                throw new InvalidOperationException($"{nameof(HttpService)} parameter is required for {nameof(CraftGrid<>)}.");
        }

        #endregion
    }
