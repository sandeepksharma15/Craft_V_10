using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components;

/// <summary>
/// Represents a column in the CraftDataGrid component.
/// </summary>
/// <typeparam name="TEntity">The entity type displayed in the grid.</typeparam>
public partial class CraftDataGridColumn<TEntity> : ComponentBase, ICraftDataGridColumn<TEntity>, IDisposable
    where TEntity : class
{
    private string? _title;
    private string? _propertyName;
    private Type? _propertyType;
    private Func<TEntity, object>? _compiledExpression;
    private bool _disposed;

    #region Cascading Parameters

    [CascadingParameter(Name = "DataGrid")]
    public ICraftDataGrid<TEntity>? DataGrid { get; set; }

    #endregion

    #region Parameters

    /// <summary>
    /// Column title displayed in the header.
    /// If not provided, it will be auto-derived from the property name.
    /// </summary>
    [Parameter]
    public string Title
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_title))
                return _title;

            // Auto-derive title from property name
            if (!string.IsNullOrWhiteSpace(PropertyName))
            {
                // Convert PascalCase to Title Case with spaces
                return System.Text.RegularExpressions.Regex.Replace(
                    PropertyName,
                    "([a-z])([A-Z])",
                    "$1 $2"
                );
            }

            return "Column";
        }
        set => _title = value;
    }

    /// <summary>
    /// Property expression for accessing the entity property.
    /// Used for sorting, filtering, and default rendering.
    /// </summary>
    [Parameter] public Expression<Func<TEntity, object>>? PropertyExpression { get; set; }

    /// <summary>
    /// Custom template for rendering cell content.
    /// If not provided, the property value will be rendered with optional formatting.
    /// </summary>
    [Parameter] public RenderFragment<TEntity>? Template { get; set; }

    /// <summary>
    /// Indicates whether the column is visible.
    /// Default is true.
    /// </summary>
    [Parameter] public bool Visible { get; set; } = true;

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

    /// <summary>
    /// Column width. CSS value like "100px", "20%", "auto".
    /// </summary>
    [Parameter] public string? Width { get; set; }

    /// <summary>
    /// Column minimum width. CSS value like "100px".
    /// </summary>
    [Parameter] public string? MinWidth { get; set; }

    /// <summary>
    /// Column maximum width. CSS value like "300px".
    /// </summary>
    [Parameter] public string? MaxWidth { get; set; }

    /// <summary>
    /// Text alignment for the column.
    /// Default is Start (left).
    /// </summary>
    [Parameter] public Alignment Alignment { get; set; } = Alignment.Start;

    /// <summary>
    /// Format string for displaying values (e.g., "N2" for numbers, "d" for dates, "C" for currency).
    /// </summary>
    [Parameter] public string? Format { get; set; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Property name derived from the PropertyExpression.
    /// </summary>
    public string? PropertyName
    {
        get
        {
            if (_propertyName is not null)
                return _propertyName;

            if (PropertyExpression is null)
                return null;

            _propertyName = GetPropertyName(PropertyExpression);

            return _propertyName;
        }
    }

    /// <summary>
    /// Property type derived from the PropertyExpression.
    /// Used for determining appropriate search operators and input types.
    /// </summary>
    public Type? PropertyType
    {
        get
        {
            if (_propertyType is not null)
                return _propertyType;

            if (PropertyExpression is null)
                return null;

            _propertyType = GetPropertyType(PropertyExpression);

            return _propertyType;
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
        DataGrid?.AddColumn(this);

        // Pre-compile the expression for better performance
        if (PropertyExpression is not null)
            _compiledExpression = PropertyExpression.Compile();
    }

    /// <summary>
    /// Disposes the component and removes it from the parent grid.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        // Unregister from parent grid to prevent duplicates
        DataGrid?.RemoveColumn(this);

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Renders the cell content for the given item.
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

    private static string GetPropertyName(Expression expression)
    {
        return expression switch
        {
            MemberExpression memberExpression => memberExpression.Member.Name,
            UnaryExpression unaryExpression when unaryExpression.Operand is MemberExpression operand => operand.Member.Name,
            LambdaExpression lambdaExpression => GetPropertyName(lambdaExpression.Body),
            _ => throw new ArgumentException($"Cannot determine property name from expression: {expression}")
        };
    }

    private static Type? GetPropertyType(Expression expression)
    {
        return expression switch
        {
            MemberExpression memberExpression => GetMemberType(memberExpression.Member),
            UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression when unaryExpression.Operand is MemberExpression operand 
                => GetMemberType(operand.Member),
            LambdaExpression lambdaExpression => GetPropertyType(lambdaExpression.Body),
            _ => null
        };
    }

    private static Type? GetMemberType(MemberInfo member)
    {
        return member switch
        {
            PropertyInfo propertyInfo => propertyInfo.PropertyType,
            FieldInfo fieldInfo => fieldInfo.FieldType,
            _ => null
        };
    }

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

        // Convert PascalCase to Title Case with spaces
        return Regex.Replace(enumValue.ToString()!, "([a-z])([A-Z])", "$1 $2");
    }

    #endregion
}

