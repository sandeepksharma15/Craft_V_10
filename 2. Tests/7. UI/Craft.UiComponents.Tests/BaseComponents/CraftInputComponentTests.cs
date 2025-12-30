using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Bunit;
using Craft.UiComponents;
using Craft.UiComponents.Tests.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Craft.UiComponents.Tests.BaseComponents;

/// <summary>
/// Tests for CraftInputComponent form integration capabilities.
/// Tests EditContext integration, validation, value binding, and parsing.
/// </summary>
public class CraftInputComponentTests : ComponentTestBase
{
    private class TestModel
    {
        public string? StringValue { get; set; }
        public int IntValue { get; set; }
        public bool BoolValue { get; set; }
    }

    #region Value Binding Tests

    [Fact]
    public void CraftInputComponent_ShouldBindValue()
    {
        // Arrange
        var model = new TestModel { StringValue = "Test Value" };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .AddCascadingValue(editContext));

        // Assert
        Assert.Contains("Test Value", cut.Markup);
    }

    [Fact]
    public void CraftInputComponent_ShouldUpdateValueOnChange()
    {
        // Arrange
        var model = new TestModel { StringValue = "Initial" };
        var editContext = new EditContext(model);
        string? capturedValue = null;

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, value => capturedValue = value))
            .AddCascadingValue(editContext));

        cut.Find("input").Change("Updated Value");

        // Assert
        Assert.Equal("Updated Value", capturedValue);
    }

    [Fact]
    public void CraftInputComponent_ShouldHandleNullValue()
    {
        // Arrange
        var model = new TestModel { StringValue = null };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .AddCascadingValue(editContext));

        // Assert
        Assert.DoesNotContain("value=", cut.Markup);
    }

    [Fact]
    public void CraftInputComponent_ShouldHandleEmptyString()
    {
        // Arrange
        var model = new TestModel { StringValue = "" };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .AddCascadingValue(editContext));

        // Assert
        var input = cut.Find("input");
        Assert.Equal(string.Empty, input.GetAttribute("value"));
    }

    #endregion

    #region EditContext Integration Tests

    [Fact]
    public void CraftInputComponent_ShouldRequireEditContext()
    {
        // Arrange
        var model = new TestModel { StringValue = "Test" };

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!));

        // Assert - Should work without EditContext but not have validation
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public void CraftInputComponent_ShouldNotifyEditContextOnChange()
    {
        // Arrange
        var model = new TestModel { StringValue = "Initial" };
        var editContext = new EditContext(model);
        var fieldChangedCount = 0;
        
        // Subscribe before component is rendered
        editContext.OnFieldChanged += (sender, args) => fieldChangedCount++;

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .AddCascadingValue(editContext));

        var countBeforeChange = fieldChangedCount;
        cut.Find("input").Change("New Value");

        // Assert - At least one notification should have been sent
        Assert.True(fieldChangedCount > countBeforeChange, "EditContext should be notified of field changes");
    }

    [Fact]
    public void CraftInputComponent_ShouldIdentifyField()
    {
        // Arrange
        var model = new TestModel { StringValue = "Test" };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .AddCascadingValue(editContext));

        // Assert
        Assert.Equal(nameof(TestModel.StringValue), cut.Instance.GetFieldIdentifier().FieldName);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void CraftInputComponent_ShouldDetectValidationErrors()
    {
        // Arrange
        var model = new TestModel { StringValue = "" };
        var editContext = new EditContext(model);
        var messages = new ValidationMessageStore(editContext);
        var fieldIdentifier = FieldIdentifier.Create(() => model.StringValue!);
        
        messages.Add(fieldIdentifier, "This field is required");
        editContext.NotifyValidationStateChanged();

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .AddCascadingValue(editContext));

        // Assert
        Assert.True(cut.Instance.GetHasValidationErrors());
        Assert.Contains("craft-invalid", cut.Markup);
    }

    [Fact]
    public void CraftInputComponent_ShouldShowValidationMessages()
    {
        // Arrange
        var model = new TestModel { StringValue = "" };
        var editContext = new EditContext(model);
        var messages = new ValidationMessageStore(editContext);
        var fieldIdentifier = FieldIdentifier.Create(() => model.StringValue!);
        
        messages.Add(fieldIdentifier, "Error message 1");
        messages.Add(fieldIdentifier, "Error message 2");
        editContext.NotifyValidationStateChanged();

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .AddCascadingValue(editContext));

        // Assert
        var validationMessages = cut.Instance.GetValidationMessages().ToList();
        Assert.Equal(2, validationMessages.Count);
        Assert.Contains("Error message 1", validationMessages);
        Assert.Contains("Error message 2", validationMessages);
    }

    [Fact]
    public void CraftInputComponent_ShouldShowValidCssWhenModified()
    {
        // Arrange
        var model = new TestModel { StringValue = "Initial" };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .AddCascadingValue(editContext));

        cut.Find("input").Change("Modified");

        // Assert
        Assert.Contains("craft-valid", cut.Markup);
    }

    [Fact]
    public void CraftInputComponent_ShouldTrackModifiedState()
    {
        // Arrange
        var model = new TestModel { StringValue = "Initial" };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .AddCascadingValue(editContext));

        Assert.False(cut.Instance.GetIsModified());

        cut.Find("input").Change("Modified");

        // Assert
        Assert.True(cut.Instance.GetIsModified());
    }

    [Fact]
    public void CraftInputComponent_ShouldReactToValidationStateChanges()
    {
        // Arrange
        var model = new TestModel { StringValue = "Valid" };
        var editContext = new EditContext(model);
        var messages = new ValidationMessageStore(editContext);
        var fieldIdentifier = FieldIdentifier.Create(() => model.StringValue!);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .AddCascadingValue(editContext));

        var initialRenderCount = cut.RenderCount;

        messages.Add(fieldIdentifier, "Validation error");
        editContext.NotifyValidationStateChanged();

        // Assert
        Assert.True(cut.RenderCount > initialRenderCount);
    }

    #endregion

    #region Parsing Tests

    [Fact]
    public void CraftInputComponent_ShouldParseValidInput()
    {
        // Arrange
        var model = new TestModel { IntValue = 0 };
        var editContext = new EditContext(model);
        int? capturedValue = null;

        // Act
        var cut = Render<TestIntInput>(parameters => parameters
            .Add(p => p.Value, model.IntValue)
            .Add(p => p.ValueExpression, () => model.IntValue)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<int>(this, value => capturedValue = value))
            .AddCascadingValue(editContext));

        cut.Find("input").Change("42");

        // Assert
        Assert.Equal(42, capturedValue);
    }

    [Fact]
    public void CraftInputComponent_ShouldHandleParsingErrors()
    {
        // Arrange
        var model = new TestModel { IntValue = 0 };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestIntInput>(parameters => parameters
            .Add(p => p.Value, model.IntValue)
            .Add(p => p.ValueExpression, () => model.IntValue)
            .AddCascadingValue(editContext));

        cut.Find("input").Change("not a number");

        // Assert
        Assert.True(cut.Instance.GetHasValidationErrors());
    }

    [Fact]
    public void CraftInputComponent_ShouldClearParsingErrorsOnValidInput()
    {
        // Arrange
        var model = new TestModel { IntValue = 0 };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestIntInput>(parameters => parameters
            .Add(p => p.Value, model.IntValue)
            .Add(p => p.ValueExpression, () => model.IntValue)
            .AddCascadingValue(editContext));

        cut.Find("input").Change("invalid");
        Assert.True(cut.Instance.GetHasValidationErrors());

        cut.Find("input").Change("123");

        // Assert
        Assert.False(cut.Instance.GetHasValidationErrors());
    }

    #endregion

    #region Property Tests

    [Fact]
    public void CraftInputComponent_ShouldApplyPlaceholder()
    {
        // Arrange
        var model = new TestModel { StringValue = "" };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .Add(p => p.Placeholder, "Enter value...")
            .AddCascadingValue(editContext));

        // Assert
        Assert.Contains("placeholder=\"Enter value...\"", cut.Markup);
    }

    [Fact]
    public void CraftInputComponent_ShouldApplyReadOnly()
    {
        // Arrange
        var model = new TestModel { StringValue = "Read Only" };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .Add(p => p.ReadOnly, true)
            .AddCascadingValue(editContext));

        // Assert
        Assert.Contains("readonly", cut.Markup);
        Assert.Contains("craft-readonly", cut.Markup);
    }

    [Fact]
    public void CraftInputComponent_ShouldApplyRequired()
    {
        // Arrange
        var model = new TestModel { StringValue = "" };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .Add(p => p.Required, true)
            .AddCascadingValue(editContext));

        // Assert
        Assert.Contains("required", cut.Markup);
        Assert.Contains("craft-required", cut.Markup);
    }

    [Fact]
    public void CraftInputComponent_ShouldApplyDisplayName()
    {
        // Arrange
        var model = new TestModel { StringValue = "" };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .Add(p => p.DisplayName, "Custom Field Name")
            .AddCascadingValue(editContext));

        // Assert
        Assert.Equal("Custom Field Name", cut.Instance.DisplayName);
    }

    #endregion

    #region Event Callback Tests

    [Fact]
    public void CraftInputComponent_ShouldFireOnChange()
    {
        // Arrange
        var model = new TestModel { StringValue = "Initial" };
        var editContext = new EditContext(model);
        var changeFired = false;

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .Add(p => p.OnChange, EventCallback.Factory.Create<ChangeEventArgs>(this, _ => changeFired = true))
            .AddCascadingValue(editContext));

        cut.Find("input").Change("New Value");

        // Assert
        Assert.True(changeFired);
    }

    [Fact]
    public void CraftInputComponent_ShouldFireOnInput()
    {
        // Arrange
        var model = new TestModel { StringValue = "Initial" };
        var editContext = new EditContext(model);
        var inputCount = 0;

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .Add(p => p.OnInput, EventCallback.Factory.Create<ChangeEventArgs>(this, _ => inputCount++))
            .AddCascadingValue(editContext));

        cut.Find("input").Input("A");
        cut.Find("input").Input("AB");
        cut.Find("input").Input("ABC");

        // Assert
        Assert.Equal(3, inputCount);
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void CraftInputComponent_ShouldUnsubscribeFromEditContextOnDispose()
    {
        // Arrange
        var model = new TestModel { StringValue = "Test" };
        var editContext = new EditContext(model);

        // Act
        var cut = Render<TestStringInput>(parameters => parameters
            .Add(p => p.Value, model.StringValue)
            .Add(p => p.ValueExpression, () => model.StringValue!)
            .AddCascadingValue(editContext));

        var instance = cut.Instance;
        cut.Dispose();

        // Trigger validation state change after disposal
        editContext.NotifyValidationStateChanged();

        // Assert - No exception should be thrown
        Assert.NotNull(instance);
    }

    #endregion
}

/// <summary>
/// Test implementation of CraftInputComponent for string values.
/// </summary>
internal class TestStringInput : CraftInputComponent<string>
{
    public FieldIdentifier GetFieldIdentifier() => FieldIdentifier;
    public bool GetHasValidationErrors() => HasValidationErrors;
    public IEnumerable<string> GetValidationMessages() => ValidationMessages;
    public bool GetIsModified() => IsModified;

    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out string result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value ?? string.Empty;
        validationErrorMessage = null;
        return true;
    }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "id", Id);
        builder.AddAttribute(2, "class", BuildCssClass());
        
        var style = BuildStyle();
        if (!string.IsNullOrEmpty(style))
        {
            builder.AddAttribute(3, "style", style);
        }

        builder.AddAttribute(4, "value", CurrentValueAsString);
        
        if (!string.IsNullOrEmpty(Placeholder))
            builder.AddAttribute(5, "placeholder", Placeholder);
        
        if (ReadOnly)
            builder.AddAttribute(6, "readonly", true);
        
        if (Required)
            builder.AddAttribute(7, "required", true);

        builder.AddAttribute(8, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(
            this, async args => await HandleChangeAsync(args)));
        
        builder.AddAttribute(9, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(
            this, async args => await HandleInputAsync(args)));

        builder.CloseElement();
    }
}

/// <summary>
/// Test implementation of CraftInputComponent for integer values.
/// </summary>
internal class TestIntInput : CraftInputComponent<int>
{
    public FieldIdentifier GetFieldIdentifier() => FieldIdentifier;
    public bool GetHasValidationErrors() => HasValidationErrors;
    public IEnumerable<string> GetValidationMessages() => ValidationMessages;
    public bool GetIsModified() => IsModified;

    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out int result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (int.TryParse(value, out var parsed))
        {
            result = parsed;
            validationErrorMessage = null;
            return true;
        }

        result = 0;
        validationErrorMessage = $"The value '{value}' is not a valid number.";
        return false;
    }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "type", "number");
        builder.AddAttribute(2, "id", Id);
        builder.AddAttribute(3, "class", BuildCssClass());
        
        var style = BuildStyle();
        if (!string.IsNullOrEmpty(style))
        {
            builder.AddAttribute(4, "style", style);
        }

        builder.AddAttribute(5, "value", CurrentValueAsString);
        
        if (!string.IsNullOrEmpty(Placeholder))
            builder.AddAttribute(6, "placeholder", Placeholder);
        
        if (ReadOnly)
            builder.AddAttribute(7, "readonly", true);
        
        if (Required)
            builder.AddAttribute(8, "required", true);

        builder.AddAttribute(9, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(
            this, async args => await HandleChangeAsync(args)));
        
        builder.AddAttribute(10, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(
            this, async args => await HandleInputAsync(args)));

        builder.CloseElement();
    }
}
