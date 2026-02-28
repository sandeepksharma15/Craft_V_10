using Craft.Auditing;
using Craft.QuerySpec;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.AppComponents.Auditing;

public partial class AuditLog : ComponentBase
{
    [Inject] private IAuditTrailHttpService _auditTrailService { get; set; } = default!;
    [Inject] private IDialogService _dialogService { get; set; } = default!;

    // Filter state
    private string? _filterTable;
    private KeyType? _filterUserId;
    private EntityChangeType? _filterChangeType;
    private DateTime? _filterStartDate;
    private DateTime? _filterEndDate;

    // User lookup cache
    private Dictionary<KeyType, string> _userLookup = [];
    private bool _filtersApplied;

    // Stable delegate reference - assigned once so Blazor does not see a new instance on every render
    private Func<Query<AuditTrail>, Query<AuditTrail>>? _buildQueryFunc;

    protected override async Task OnInitializedAsync()
    {
        _buildQueryFunc = BuildQuery;
        await LoadAuditUsersAsync();
    }

    /// <summary>
    /// Loads audit users and builds a lookup dictionary for display.
    /// </summary>
    private async Task LoadAuditUsersAsync()
    {
        var result = await _auditTrailService.GetAuditUsersAsync();

        if (result.IsSuccess && result.Value != null)
            _userLookup = result.Value.ToDictionary(u => u.UserId, u => u.UserName ?? $"User {u.UserId}");
    }

    /// <summary>
    /// Applies filters to refresh the grid data.
    /// </summary>
    private void ApplyFilters()
    {
        _filtersApplied = true;
        StateHasChanged();
    }

    /// <summary>
    /// Builds the query with applied filters.
    /// </summary>
    private Query<AuditTrail> BuildQuery(Query<AuditTrail> query)
    {
        // Apply table filter
        if (!string.IsNullOrWhiteSpace(_filterTable))
            query.Where(a => a.TableName == _filterTable);

        // Apply user filter
        if (_filterUserId.HasValue && _filterUserId.Value != default)
            query.Where(a => a.UserId == _filterUserId.Value);

        // Apply change type filter
        if (_filterChangeType.HasValue)
            query.Where(a => a.ChangeType == _filterChangeType.Value);

        // Apply date range filter (inclusive of end date)
        if (_filterStartDate.HasValue)
        {
            var startDate = _filterStartDate.Value.Date;
            query.Where(a => a.DateTimeUTC >= startDate);
        }

        if (_filterEndDate.HasValue)
        {
            // Make end date inclusive by adding one day and using less than
            var endDate = _filterEndDate.Value.Date.AddDays(1);
            query.Where(a => a.DateTimeUTC < endDate);
        }

        // Default sort: most recent first
        query.OrderByDescending(a => a.DateTimeUTC);

        return query;
    }

    /// <summary>
    /// Gets the user name from the lookup dictionary.
    /// </summary>
    private string GetUserName(AuditTrail auditTrail)
    {
        if (_userLookup.TryGetValue(auditTrail.UserId, out var userName))
            return userName;

        return $"User {auditTrail.UserId}";
    }

    /// <summary>
    /// Handles the view action to show the diff dialog.
    /// </summary>
    private async Task HandleView(AuditTrail auditTrail)
    {
        var parameters = new DialogParameters
        {
            { nameof(AuditValuesDiffDialog.TableName), auditTrail.TableName },
            { nameof(AuditValuesDiffDialog.OldValues), auditTrail.OldValues },
            { nameof(AuditValuesDiffDialog.NewValues), auditTrail.NewValues },
            { nameof(AuditValuesDiffDialog.ChangedColumns), auditTrail.ChangedColumns },
            { nameof(AuditValuesDiffDialog.ChangeType), auditTrail.ChangeType }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            CloseButton = true,
            CloseOnEscapeKey = true
        };

        await _dialogService.ShowAsync<AuditValuesDiffDialog>("Audit Details", parameters, options);
    }
}
