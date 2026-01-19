using Craft.QuerySpec;
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
    private List<ComparisonType> _availableOperators = [];
    private List<(string Name, object Value)> _enumValues = [];

    private string? _stringValue;
    private decimal? _numericValue;
    private DateTime? _dateValue;
    private bool? _boolValue;
    private object? _enumValue;

    #region Cascading Parameters

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    #endregion

    #region Parameters

    /// <summary>
    /// List of searchable columns to choose from.
    /// </summary>
    [Parameter, EditorRequired] public List<ICraftDataGridColumn<TEntity>> SearchableColumns { get; set; } = [];

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

            if (_selectedColumn.PropertyType.IsNumeric())
                return _numericValue.HasValue;

            if (_selectedColumn.PropertyType.IsDateTime())
                return _dateValue.HasValue;

            if (_selectedColumn.PropertyType.IsBoolean())
                return _boolValue.HasValue;

            if (_selectedColumn.PropertyType.IsEnumType())
                return _enumValue is not null;

            return !string.IsNullOrWhiteSpace(_stringValue);
        }
    }

    #endregion

    #region Methods

    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Auto-select the first column when dialog opens
        if (SearchableColumns.Count > 0)
        {
            _selectedColumn = SearchableColumns[0];
            if (_selectedColumn.PropertyType is not null)
                UpdateAvailableOperators(_selectedColumn.PropertyType);
        }
    }

    private Task OnColumnSelectedAsync()
    {
        if (_selectedColumn?.PropertyType is not null)
            UpdateAvailableOperators(_selectedColumn.PropertyType);

        return Task.CompletedTask;
    }

    private void UpdateAvailableOperators(Type propertyType)
    {
        _availableOperators = propertyType.GetValidComparisonOperators();

        if (!_availableOperators.Contains(_selectedOperator))
            _selectedOperator = _availableOperators.FirstOrDefault();

        if (propertyType.IsEnumType())
            _enumValues = propertyType.GetEnumNameValuePairs();
    }

    private void AddFilter()
    {
        if (_selectedColumn is null || _selectedColumn.PropertyType is null)
            return;

        object? value = GetCurrentValue();

        if (value is null && !CanValueBeNull(_selectedColumn.PropertyType))
            return;

        var filter = new FilterCriteria(
            propertyType: _selectedColumn.PropertyType,
            name: _selectedColumn.PropertyName ?? string.Empty,
            value: value,
            comparison: _selectedOperator,
            displayTitle: _selectedColumn.Title
        );

        // Close the dialog and return the filter
        MudDialog.Close(DialogResult.Ok(filter));
    }

    private object? GetCurrentValue()
    {
        if (_selectedColumn?.PropertyType is null)
            return null;

        if (_selectedColumn.PropertyType.IsNumeric())
            return ConvertNumericValue(_selectedColumn.PropertyType, _numericValue);

        if (_selectedColumn.PropertyType.IsDateTime())
            return _dateValue;

        if (_selectedColumn.PropertyType.IsBoolean())
            return _boolValue;

        if (_selectedColumn.PropertyType.IsEnumType())
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

    private void Cancel() => MudDialog.Cancel();

    #endregion
}
