using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Craft.Utilities.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Craft.UiConponents;

/// <summary>
/// Base component for form input elements that integrates with Blazor's EditContext
/// for validation and two-way binding support.
/// </summary>
/// <typeparam name="TValue">The type of the input value.</typeparam>
public abstract class CraftInputComponent<TValue> : CraftComponent
{
    private bool _hasInitializedParameters;
    //private bool _previousParsingAttemptFailed;
    private ValidationMessageStore? _parsingValidationMessages;
    private Type? _nullableUnderlyingType;

    /// <summary>
    /// Gets the associated <see cref="EditContext"/> from the cascading parameter.
    /// </summary>
    protected EditContext? EditContext { get; private set; }

    /// <summary>
    /// Gets the <see cref="FieldIdentifier"/> for the bound value.
    /// </summary>
    protected FieldIdentifier FieldIdentifier { get; private set; }

    /// <summary>
    /// Gets or sets the current value of the input.
    /// </summary>
    protected TValue? CurrentValue
    {
        get => Value;
        set
        {
            var hasChanged = !EqualityComparer<TValue>.Default.Equals(value, Value);

            if (hasChanged)
            {
                Value = value;
                _ = ValueChanged.InvokeAsync(Value);
                EditContext?.NotifyFieldChanged(FieldIdentifier);

                LogDebug("Value changed for field: {FieldName}", FieldIdentifier.FieldName);
            }
        }
    }

    /// <summary>
    /// Gets or sets the current value as a string for binding to HTML input elements.
    /// </summary>
    protected string? CurrentValueAsString
    {
        get => FormatValueAsString(CurrentValue);
        set
        {
            _parsingValidationMessages?.Clear();

            if (_nullableUnderlyingType is not null && string.IsNullOrEmpty(value))
            {
                //_previousParsingAttemptFailed = false;
                CurrentValue = default;
            }
            else if (TryParseValueFromString(value, out var parsedValue, out var validationErrorMessage))
            {
                //_previousParsingAttemptFailed = false;
                CurrentValue = parsedValue;
            }
            else
            {
                //_previousParsingAttemptFailed = true;
                _parsingValidationMessages ??= new ValidationMessageStore(EditContext!);
                _parsingValidationMessages.Add(FieldIdentifier, validationErrorMessage ?? "Invalid value.");

                LogWarning("Failed to parse value for field: {FieldName}", FieldIdentifier.FieldName);
            }

            EditContext?.NotifyFieldChanged(FieldIdentifier);
        }
    }

    #region Parameters

    /// <summary>
    /// Gets or sets the cascading edit context.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    /// <summary>
    /// Gets or sets the value of the input.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the expression identifying the bound value.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the display name for the field, used in validation messages.
    /// </summary>
    [Parameter]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text for the input.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets whether the input is read-only.
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets whether the input is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the input value changes (before validation).
    /// </summary>
    [Parameter]
    public EventCallback<ChangeEventArgs> OnChange { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked on every input event.
    /// </summary>
    [Parameter]
    public EventCallback<ChangeEventArgs> OnInput { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets whether the current field has validation errors.
    /// </summary>
    protected bool HasValidationErrors =>
        EditContext?.GetValidationMessages(FieldIdentifier).Any() == true;

    /// <summary>
    /// Gets the validation messages for the current field.
    /// </summary>
    protected IEnumerable<string> ValidationMessages =>
        EditContext?.GetValidationMessages(FieldIdentifier) ?? [];

    /// <summary>
    /// Gets whether the field has been modified.
    /// </summary>
    protected bool IsModified => EditContext?.IsModified(FieldIdentifier) == true;

    /// <summary>
    /// Gets the CSS class for the current validation state.
    /// </summary>
    protected string ValidationCssClass
    {
        get
        {
            if (EditContext is null)
                return string.Empty;

            if (HasValidationErrors)
                return "craft-invalid";

            if (IsModified)
                return "craft-valid";

            return string.Empty;
        }
    }

    #endregion

    #region Lifecycle

    /// <inheritdoc />
    public override Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        if (!_hasInitializedParameters)
        {
            EditContext = CascadedEditContext;

            if (EditContext is not null && ValueExpression is not null)
            {
                FieldIdentifier = FieldIdentifier.Create(ValueExpression);
                _nullableUnderlyingType = Nullable.GetUnderlyingType(typeof(TValue));

                EditContext.OnValidationStateChanged += OnValidationStateChanged;

                LogDebug("Input component bound to field: {FieldName}", FieldIdentifier.FieldName);
            }

            _hasInitializedParameters = true;
        }
        else if (CascadedEditContext != EditContext)
        {
            throw new InvalidOperationException(
                $"{GetType()} does not support changing the EditContext dynamically.");
        }

        return base.SetParametersAsync(ParameterView.Empty);
    }

    /// <inheritdoc />
    protected override string BuildCssClass()
    {
        var baseClass = base.BuildCssClass();

        var builder = new CssBuilder(baseClass)
            .AddClass(ValidationCssClass, !string.IsNullOrEmpty(ValidationCssClass))
            .AddClass("craft-readonly", ReadOnly)
            .AddClass("craft-required", Required);

        return builder.Build();
    }

    #endregion

    #region Abstract/Virtual Methods

    /// <summary>
    /// Formats the value as a string for display.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted string representation.</returns>
    protected virtual string? FormatValueAsString(TValue? value) => value?.ToString();

    /// <summary>
    /// Parses a string value into the target type.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="result">The parsed result if successful.</param>
    /// <param name="validationErrorMessage">The validation error message if parsing fails.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    protected abstract bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out TValue result,
        [NotNullWhen(false)] out string? validationErrorMessage);

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the change event from the input element.
    /// </summary>
    protected virtual async Task HandleChangeAsync(ChangeEventArgs args)
    {
        CurrentValueAsString = args.Value?.ToString();
        await OnChange.InvokeAsync(args);
    }

    /// <summary>
    /// Handles the input event from the input element.
    /// </summary>
    protected virtual async Task HandleInputAsync(ChangeEventArgs args)
    {
        await OnInput.InvokeAsync(args);
    }

    private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    #endregion

    #region Disposal

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore()
    {
        EditContext?.OnValidationStateChanged -= OnValidationStateChanged;

        _parsingValidationMessages?.Clear();

        return base.DisposeAsyncCore();
    }

    #endregion
}
