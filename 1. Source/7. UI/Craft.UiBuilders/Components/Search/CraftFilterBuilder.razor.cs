using Craft.QuerySpec;
using Craft.UiBuilders.Helpers;
using Craft.UiBuilders.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Components;

/// <summary>
/// Dialog component for building advanced search filters.
/// </summary>
/// <typeparam name="TEntity">The entity type being filtered.</typeparam>
public partial class CraftFilterBuilder<TEntity> : ComponentBase
    where TEntity : class
{
    private ICraftDataGridColumn<TEntity>? _selectedColumn;
    private ComparisonType _selectedOperator = ComparisonType.EqualTo;
    private LogicalOperatorType _logicalOperator = LogicalOperatorType.And;
    private List<ComparisonType> _availableOperators = [];
    private List<(string Name, object Value)> _enumValues = [];

    private string? _stringValue;
    private decimal? _numericValue;
    private DateTime? _dateValue;
    private bool? _boolValue;
    private object? _enumValue;

    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
    };

    #region Parameters

    /// <summary>
    /// Controls the visibility of the dialog.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; }

    /// <summary>
    /// Callback invoked when visibility changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> VisibleChanged { get; set; }

    /// <summary>
    /// List of searchable columns to choose from.
    /// </summary>
    [Parameter, EditorRequired]
    public List<ICraftDataGridColumn<TEntity>> SearchableColumns { get; set; } = [];

    /// <summary>
    /// Indicates if this is the first filter (affects logical operator display).
    /// </summary>
    [Parameter]
    public bool IsFirstFilter { get; set; } = true;

    /// <summary>
    /// Callback invoked when a filter is added.
    /// </summary>
    [Parameter]
    public EventCallback<FilterModel> OnFilterAdded { get; set; }

    #endregion

    #region Computed Properties

    private bool CanAddFilter =>
        _selectedColumn is not null
        && HasValue;

    private bool HasValue
    {
        get
        {
            if (_selectedColumn?.PropertyType is null)
                return false;

            if (ColumnTypeHelper.IsNumericType(_selectedColumn.PropertyType))
                return _numericValue.HasValue;

            if (ColumnTypeHelper.IsDateTimeType(_selectedColumn.PropertyType))
                return _dateValue.HasValue;

            if (ColumnTypeHelper.IsBooleanType(_selectedColumn.PropertyType))
                return _boolValue.HasValue;

            if (ColumnTypeHelper.IsEnumType(_selectedColumn.PropertyType))
                return _enumValue is not null;

            return !string.IsNullOrWhiteSpace(_stringValue);
        }
    }

    #endregion

    #region Methods

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (_selectedColumn is not null && _selectedColumn.PropertyType is not null)
            UpdateAvailableOperators(_selectedColumn.PropertyType);
    }

    private void UpdateAvailableOperators(Type propertyType)
    {
        _availableOperators = ColumnTypeHelper.GetValidOperators(propertyType);

        if (!_availableOperators.Contains(_selectedOperator))
            _selectedOperator = _availableOperators.FirstOrDefault();

        if (ColumnTypeHelper.IsEnumType(propertyType))
            _enumValues = ColumnTypeHelper.GetEnumValues(propertyType);
    }

    private async Task AddFilter()
    {
        if (_selectedColumn is null || _selectedColumn.PropertyType is null)
            return;

        object? value = GetCurrentValue();

        if (value is null && !CanValueBeNull(_selectedColumn.PropertyType))
            return;

        var filter = new FilterModel
        {
            ColumnName = _selectedColumn.PropertyName ?? string.Empty,
            ColumnTitle = _selectedColumn.Title,
            PropertyType = _selectedColumn.PropertyType,
            Operator = _selectedOperator,
            Value = value,
            LogicalOperator = IsFirstFilter ? null : _logicalOperator
        };

        await OnFilterAdded.InvokeAsync(filter);
        await CloseDialog();
        ResetForm();
    }

    private object? GetCurrentValue()
    {
        if (_selectedColumn?.PropertyType is null)
            return null;

        if (ColumnTypeHelper.IsNumericType(_selectedColumn.PropertyType))
            return ConvertNumericValue(_selectedColumn.PropertyType, _numericValue);

        if (ColumnTypeHelper.IsDateTimeType(_selectedColumn.PropertyType))
            return _dateValue;

        if (ColumnTypeHelper.IsBooleanType(_selectedColumn.PropertyType))
            return _boolValue;

        if (ColumnTypeHelper.IsEnumType(_selectedColumn.PropertyType))
            return _enumValue;

        return _stringValue;
    }

    private static object? ConvertNumericValue(Type targetType, decimal? value)
    {
        if (value is null)
            return null;

        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        return underlyingType.Name switch
        {
            nameof(Int32) => (int)value,
            nameof(Int64) => (long)value,
            nameof(Int16) => (short)value,
            nameof(Byte) => (byte)value,
            nameof(Double) => (double)value,
            nameof(Single) => (float)value,
            nameof(Decimal) => value,
            _ => value
        };
    }

    private static bool CanValueBeNull(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
    }

    private async Task Cancel()
    {
        await CloseDialog();
        ResetForm();
    }

    private async Task CloseDialog()
    {
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
    }

    private void ResetForm()
    {
        _selectedColumn = null;
        _selectedOperator = ComparisonType.EqualTo;
        _logicalOperator = LogicalOperatorType.And;
        _stringValue = null;
        _numericValue = null;
        _dateValue = null;
        _boolValue = null;
        _enumValue = null;
        _availableOperators.Clear();
        _enumValues.Clear();
    }

    #endregion
}
