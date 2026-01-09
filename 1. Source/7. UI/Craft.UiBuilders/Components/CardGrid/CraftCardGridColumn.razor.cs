using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Craft.Domain;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components;

/// <summary>
/// Represents a column in the CraftCardGrid component.
/// </summary>
/// <typeparam name="TEntity">The entity type displayed in the card grid.</typeparam>
public partial class CraftCardGridColumn<TEntity> : ComponentBase, ICraftCardGridColumn<TEntity>
    where TEntity : class, IEntity, IModel, new()
{
    private string? _caption;
    private string? _propertyName;
    private Func<TEntity, object>? _compiledExpression;

    #region Cascading Parameters

    [CascadingParameter(Name = "CardGrid")]
    public ICraftCardGrid<TEntity>? CardGrid { get; set; }

    #endregion

    #region Parameters

    /// <summary>
    /// Column caption displayed in the card.
    /// If not provided, it will be auto-derived from the property name.
    /// </summary>
    [Parameter]
    public string Caption
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_caption))
                return _caption;

            // Auto-derive caption from property name
            if (!string.IsNullOrWhiteSpace(PropertyName))
            {
                // Convert PascalCase to Title Case with spaces
                return Regex.Replace(PropertyName, "([a-z])([A-Z])", "$1 $2");
            }

            return "Column";
        }
        set => _caption = value;
    }

    /// <summary>
    /// Property expression for accessing the entity property.
    /// Used for sorting, filtering, and default rendering.
    /// </summary>
    [Parameter] public Expression<Func<TEntity, object>>? PropertyExpression { get; set; }

    /// <summary>
    /// Custom template for rendering column content.
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
    /// Format string for displaying values (e.g., "N2" for numbers, "d" for dates, "C" for currency).
    /// </summary>
    [Parameter] public string? Format { get; set; }

    /// <summary>
    /// Type of column (Id, Title, SubTitle, or Field).
    /// Default is Field.
    /// </summary>
    [Parameter] public CardFieldType FieldType { get; set; } = CardFieldType.Field;

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

    #endregion

    #region Lifecycle Methods

    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Validate parameters
        if (Sortable && PropertyExpression is null)
            throw new InvalidOperationException($"Column '{Caption}' is marked as sortable but PropertyExpression is not provided.");

        if (Searchable && PropertyExpression is null)
            throw new InvalidOperationException($"Column '{Caption}' is marked as searchable but PropertyExpression is not provided.");

        if (FieldType == CardFieldType.Id && PropertyExpression is null)
            throw new InvalidOperationException($"Column '{Caption}' is marked as Id but PropertyExpression is not provided.");

        // Register with parent card grid
        CardGrid?.AddColumn(this);

        // Pre-compile the expression for better performance
        if (PropertyExpression is not null)
            _compiledExpression = PropertyExpression.Compile();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Renders the column content for the given item.
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
