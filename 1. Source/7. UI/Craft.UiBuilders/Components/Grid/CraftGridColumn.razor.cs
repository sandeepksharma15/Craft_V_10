using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Craft.Domain;
using Craft.Extensions.Expressions;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components;

/// <summary>
/// Represents a unified column definition for both CraftDataGrid and CraftCardGrid.
/// Automatically adapts to table or card view based on parent grid context.
/// </summary>
/// <typeparam name="TEntity">The entity type displayed in the grid.</typeparam>
public partial class CraftGridColumn<TEntity> : ComponentBase
    where TEntity : class, IEntity, IModel, new()
{
    private string? _title;
    private string? _propertyName;
    private Func<TEntity, object>? _compiledExpression;
    private CardFieldType _resolvedCardFieldType;
    private bool _cardFieldTypeResolved;

    #region Cascading Parameters

    [CascadingParameter(Name = "Grid")]
    public CraftGrid<TEntity>? Grid { get; set; }

    #endregion

    #region Parameters - Basic

    /// <summary>
    /// Column title/caption displayed in headers.
    /// If not provided, auto-derived from the final property name (e.g., "Name" from "Location.Name").
    /// </summary>
    [Parameter]
    public string Title
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_title))
                return _title;

            if (PropertyExpression is not null)
            {
                var finalName = PropertyExpression.GetFinalPropertyName();
                return Regex.Replace(finalName, "([a-z])([A-Z])", "$1 $2");
            }

            return "Column";
        }
        set => _title = value;
    }

    /// <summary>
    /// Property expression for accessing the entity property.
    /// Used for sorting, filtering, and rendering.
    /// </summary>
    [Parameter] public Expression<Func<TEntity, object>>? PropertyExpression { get; set; }

    /// <summary>
    /// Custom template for rendering content.
    /// Same template used in both table and card views.
    /// </summary>
    [Parameter] public RenderFragment<TEntity>? Template { get; set; }

    /// <summary>
    /// Format string for displaying values (e.g., "N2", "d", "C").
    /// </summary>
    [Parameter] public string? Format { get; set; }

    #endregion

    #region Parameters - Visibility

    /// <summary>
    /// Indicates whether the column is visible in table view.
    /// Default is true.
    /// </summary>
    [Parameter] public bool Visible { get; set; } = true;

    /// <summary>
    /// Indicates whether the column should be shown in card view.
    /// If not specified, smart defaults apply (first 5 columns shown).
    /// </summary>
    [Parameter] public bool? ShowInCardView { get; set; }

    #endregion

    #region Parameters - Features

    /// <summary>
    /// Indicates whether the column is sortable.
    /// Default is false.
    /// </summary>
    [Parameter] public bool Sortable { get; set; }

    /// <summary>
    /// Indicates whether the column is searchable.
    /// Default is false.
    /// </summary>
    [Parameter] public bool Searchable { get; set; }

    /// <summary>
    /// Default sort direction for the column.
    /// Null means no default sorting.
    /// </summary>
    [Parameter] public GridSortDirection? DefaultSort { get; set; }

    /// <summary>
    /// Sort order when multiple columns have default sorting.
    /// Lower values are sorted first.
    /// Default is 0.
    /// </summary>
    [Parameter] public int SortOrder { get; set; }

    #endregion

    #region Parameters - Table View Specific

    /// <summary>
    /// Column width for table view. CSS value like "100px", "20%", "auto".
    /// Ignored in card view.
    /// </summary>
    [Parameter] public string? Width { get; set; }

    /// <summary>
    /// Column minimum width for table view. CSS value like "100px".
    /// Ignored in card view.
    /// </summary>
    [Parameter] public string? MinWidth { get; set; }

    /// <summary>
    /// Column maximum width for table view. CSS value like "300px".
    /// Ignored in card view.
    /// </summary>
    [Parameter] public string? MaxWidth { get; set; }

    /// <summary>
    /// Text alignment for table view.
    /// Default is Start (left). Ignored in card view.
    /// </summary>
    [Parameter] public Alignment Alignment { get; set; } = Alignment.Start;

    #endregion

    #region Parameters - Card View Specific

    /// <summary>
    /// Type of field in card view (Id, Title, SubTitle, or Field).
    /// If not specified, auto-detects Id from property name, rest are Field.
    /// </summary>
    [Parameter] public CardFieldType? CardFieldType { get; set; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Full property path derived from PropertyExpression (e.g., "Location.Name" for navigation properties).
    /// Used for sorting and uniquely identifying columns.
    /// </summary>
    public string? PropertyName
    {
        get
        {
            if (_propertyName is not null)
                return _propertyName;

            if (PropertyExpression is null)
                return null;

            _propertyName = PropertyExpression.GetFullPropertyPath();
            return _propertyName;
        }
    }

    /// <summary>
    /// Resolved card field type with auto-detection.
    /// </summary>
    public CardFieldType ResolvedCardFieldType
    {
        get
        {
            if (_cardFieldTypeResolved)
                return _resolvedCardFieldType;

            // If explicitly set, use it
            if (CardFieldType.HasValue)
            {
                _resolvedCardFieldType = CardFieldType.Value;
                _cardFieldTypeResolved = true;
                return _resolvedCardFieldType;
            }

            // Auto-detect Id field
            if (PropertyName?.Equals("Id", StringComparison.OrdinalIgnoreCase) == true)
            {
                _resolvedCardFieldType = Components.CardFieldType.Id;
                _cardFieldTypeResolved = true;
                return _resolvedCardFieldType;
            }

            // Default to regular field
            _resolvedCardFieldType = Components.CardFieldType.Field;
            _cardFieldTypeResolved = true;
            return _resolvedCardFieldType;
        }
    }

    /// <summary>
    /// Determines if this column should be visible in card view.
    /// Uses smart defaults if not explicitly set.
    /// </summary>
    public bool IsVisibleInCardView
    {
        get
        {
            // If explicitly set, honor it
            if (ShowInCardView.HasValue)
                return ShowInCardView.Value;

            // Always show Id field (even though it's hidden in rendering)
            if (ResolvedCardFieldType == Components.CardFieldType.Id)
                return true;

            // Smart default: Show first 5 visible columns in card
            if (Grid is null)
                return false;

            var visibleColumns = Grid.Columns
                .Where(c => c.Visible && c != this)
                .ToList();

            var thisIndex = Grid.Columns.IndexOf(this);
            var visibleIndex = visibleColumns.Count(c => Grid.Columns.IndexOf(c) < thisIndex);

            // Show first 5 visible columns (excluding Id which is always shown but hidden)
            return visibleIndex < 5;
        }
    }

    #endregion

    #region Lifecycle Methods

    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Validate parameters
        if (Sortable && PropertyExpression is null)
            throw new InvalidOperationException($"Column '{Title}' is marked as sortable but PropertyExpression is not provided.");

        if (Searchable && PropertyExpression is null)
            throw new InvalidOperationException($"Column '{Title}' is marked as searchable but PropertyExpression is not provided.");

        // Register with parent grid
        Grid?.AddColumn(this);

        // Pre-compile the expression for performance
        if (PropertyExpression is not null)
            _compiledExpression = PropertyExpression.Compile();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Renders the content for the given item.
    /// </summary>
    public string Render(TEntity item)
    {
        if (item is null)
            return string.Empty;

        if (PropertyExpression is null || _compiledExpression is null)
            return string.Empty;

        try
        {
            var value = _compiledExpression(item);

            if (value is null)
                return string.Empty;

            // Handle boolean values
            if (value is bool boolValue)
                return boolValue ? "Yes" : "No";

            // Handle enum values
            if (value.GetType().IsEnum)
                return FormatEnumValue(value);

            // Apply format string if provided
            if (!string.IsNullOrWhiteSpace(Format) && value is IFormattable formattable)
                return formattable.ToString(Format, CultureInfo.CurrentCulture);

            return value.ToString() ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    #endregion

    #region Private Methods

    private static string FormatEnumValue(object enumValue)
    {
        var enumType = enumValue.GetType();
        var memberInfo = enumType.GetMember(enumValue.ToString()!);

        if (memberInfo.Length > 0)
        {
            var displayAttribute = memberInfo[0].GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();

            if (displayAttribute is not null)
                return displayAttribute.GetName() ?? enumValue.ToString()!;
        }

        return Regex.Replace(enumValue.ToString()!, "([a-z])([A-Z])", "$1 $2");
    }

    #endregion
}

