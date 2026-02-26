using System.ComponentModel.DataAnnotations;
using Craft.QuerySpec;
using Craft.UiBuilders.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.Auditing.UI;

public partial class AuditLog
{
    [Inject, Required] private IHttpService<AuditTrail>? _auditService { get; set; }
    [Inject, Required] private IDialogService? _dialogService { get; set; }

    /// <summary>
    /// Optional delegate to load distinct table names from the host application.
    /// </summary>
    [Parameter] public Func<Task<List<string>>>? LoadTableNames { get; set; }

    /// <summary>
    /// Optional delegate to load users who have audit entries from the host application.
    /// </summary>
    [Parameter] public Func<Task<List<AuditUserInfo>>>? LoadAuditUsers { get; set; }

    private DateTime? _fromDate;
    private DateTime? _toDate;
    private string? _selectedTableName;
    private EntityChangeType? _selectedChangeType;
    private long? _selectedUserId;
    private bool _filtersActive;

    private List<string> _tableNames = [];
    private List<AuditUserInfo> _auditUsers = [];

    private CraftDataGrid<AuditTrail>? _dataGrid;

    protected override async Task OnInitializedAsync()
    {
        await LoadReferenceDataAsync();
    }

    private async Task LoadReferenceDataAsync()
    {
        if (LoadTableNames is not null)
            _tableNames = await LoadTableNames();

        if (LoadAuditUsers is not null)
            _auditUsers = await LoadAuditUsers();
    }

    private Func<Query<AuditTrail>, Query<AuditTrail>> BuildQueryFilter()
    {
        // Capture 'this' so the lambda reads current field values at call time
        return query =>
        {
            if (_fromDate.HasValue)
                query.EntityFilterBuilder?.Add(x => x.DateTimeUTC >= _fromDate!.Value);

            if (_toDate.HasValue)
                query.EntityFilterBuilder?.Add(x => x.DateTimeUTC <= _toDate!.Value.AddDays(1).AddSeconds(-1));

            if (!string.IsNullOrWhiteSpace(_selectedTableName))
                query.EntityFilterBuilder?.Add(x => x.TableName == _selectedTableName);

            if (_selectedChangeType.HasValue)
                query.EntityFilterBuilder?.Add(x => x.ChangeType == _selectedChangeType!.Value);

            if (_selectedUserId.HasValue)
                query.EntityFilterBuilder?.Add(x => x.UserId == _selectedUserId!.Value);

            return query;
        };
    }

    private async Task ApplyFiltersAsync()
    {
        _filtersActive = _fromDate.HasValue
            || _toDate.HasValue
            || !string.IsNullOrWhiteSpace(_selectedTableName)
            || _selectedChangeType.HasValue
            || _selectedUserId.HasValue;

        StateHasChanged();

        if (_dataGrid is not null)
            await _dataGrid.RefreshAsync();
    }

    private async Task ClearFiltersAsync()
    {
        _fromDate = null;
        _toDate = null;
        _selectedTableName = null;
        _selectedChangeType = null;
        _selectedUserId = null;
        _filtersActive = false;

        StateHasChanged();

        if (_dataGrid is not null)
            await _dataGrid.RefreshAsync();
    }

    private async Task ViewDiffAsync(AuditTrail auditTrail)
    {
        var parameters = new DialogParameters<AuditValuesDiffDialog>
        {
            { x => x.TableName, auditTrail.TableName },
            { x => x.OldValues, auditTrail.OldValues },
            { x => x.NewValues, auditTrail.NewValues },
            { x => x.ChangedColumns, auditTrail.ChangedColumns },
            { x => x.ChangeType, auditTrail.ChangeType }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            CloseButton = true
        };

        await _dialogService!.ShowAsync<AuditValuesDiffDialog>("Change Details", parameters, options);
    }

    private string GetUserDisplayName(long userId)
    {
        var user = _auditUsers.FirstOrDefault(u => u.UserId == userId);
        return user?.DisplayName ?? userId.ToString();
    }

    private static Color GetChangeTypeColor(EntityChangeType changeType) => changeType switch
    {
        EntityChangeType.Created => Color.Success,
        EntityChangeType.Deleted => Color.Error,
        EntityChangeType.Updated => Color.Warning,
        _ => Color.Default
    };

    private static string GetChangeTypeIcon(EntityChangeType changeType) => changeType switch
    {
        EntityChangeType.Created => Icons.Material.Filled.AddCircle,
        EntityChangeType.Deleted => Icons.Material.Filled.RemoveCircle,
        EntityChangeType.Updated => Icons.Material.Filled.Edit,
        _ => Icons.Material.Filled.Circle
    };
}
