# CustomValidator Component

## Overview

`CustomValidator` is a production-ready Blazor component that integrates with `EditContext` to display server-side validation errors and custom validation messages in Blazor forms.

## Features

- ? **IDisposable Pattern** - Proper cleanup of event handlers to prevent memory leaks
- ? **Null Safety** - Full nullable reference type support with guard clauses
- ? **System.Text.Json** - Modern JSON parsing without external dependencies
- ? **Flexible Error Display** - Support for multiple error formats (RFC 7807, dictionaries, custom responses)
- ? **Optional Dependencies** - Gracefully handles missing ISnackbar or ILogger
- ? **Testable Design** - Clear separation of concerns with dependency injection
- ? **Comprehensive API** - Multiple methods for different error scenarios
- ? **XML Documentation** - Full IntelliSense support

## Installation

The component is part of the `Craft.UiBuilders` library and requires:
- .NET 10.0
- Microsoft.AspNetCore.Components.Forms
- MudBlazor (optional, for Snackbar notifications)

## Basic Usage

### Simple Form with CustomValidator

```razor
<EditForm Model="@model" OnValidSubmit="@HandleValidSubmit">
    <DataAnnotationsValidator />
    <CustomValidator @ref="customValidator" />
    
    <MudTextField @bind-Value="model.Email" 
                  Label="Email" 
                  For="@(() => model.Email)" />
    
    <ValidationSummary />
    
    <MudButton ButtonType="ButtonType.Submit" Variant="Variant.Filled" Color="Color.Primary">
        Submit
    </MudButton>
</EditForm>

@code {
    private CustomValidator? customValidator;
    private MyModel model = new();
    
    private async Task HandleValidSubmit()
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync("/api/users", model);
            
            if (!response.IsSuccessStatusCode)
            {
                await customValidator!.DisplayErrorsAsync(response);
                return;
            }
            
            // Handle success
        }
        catch (Exception ex)
        {
            customValidator?.AddError(string.Empty, $"An error occurred: {ex.Message}");
            customValidator?.NotifyValidationStateChanged();
        }
    }
}
```

## API Reference

### Properties

#### `HasErrors`
```csharp
public bool HasErrors { get; }
```
Returns `true` if there are any validation errors currently displayed.

**Example:**
```csharp
if (customValidator.HasErrors)
{
    Logger.LogWarning("Form has validation errors");
}
```

### Methods

#### `AddError(string fieldName, string errorMessage)`
Adds a validation error for a specific field.

**Parameters:**
- `fieldName`: The name of the field (use empty string for form-level errors)
- `errorMessage`: The error message to display

**Example:**
```csharp
customValidator.AddError("Email", "Email address is already in use");
customValidator.AddError(string.Empty, "An unexpected error occurred");
```

#### `ClearErrors()`
Clears all validation errors and notifies the EditContext.

**Example:**
```csharp
customValidator.ClearErrors();
```

#### `DisplayErrorsAsync(HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)`
Displays validation errors from an HTTP response. Automatically detects and handles:
- RFC 7807 Problem Details format
- Dictionary-based validation errors
- Empty or invalid responses

**Example:**
```csharp
var response = await HttpClient.PostAsJsonAsync("/api/submit", data);
if (!response.IsSuccessStatusCode)
{
    await customValidator.DisplayErrorsAsync(response);
}
```

#### `DisplayErrors(ServerResponse serverResponse)`
Displays validation errors from a `ServerResponse` object.

**Example:**
```csharp
var result = await MyService.SubmitAsync(data);
if (!result.IsSuccess)
{
    customValidator.DisplayErrors(result);
}
```

#### `DisplayErrors(Dictionary<string, List<string>> errors)`
Displays validation errors from a dictionary.

**Example:**
```csharp
var errors = new Dictionary<string, List<string>>
{
    { "Email", new List<string> { "Email is required", "Email format is invalid" } },
    { "Password", new List<string> { "Password must be at least 8 characters" } }
};
customValidator.DisplayErrors(errors);
```

#### `NotifyValidationStateChanged()`
Manually triggers validation state notification. Use after adding errors manually.

**Example:**
```csharp
customValidator.AddError("Field1", "Error 1");
customValidator.AddError("Field2", "Error 2");
customValidator.NotifyValidationStateChanged();
```

## Advanced Scenarios

### Handling RFC 7807 Problem Details

The component automatically handles RFC 7807 Problem Details responses:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "The request contains invalid data",
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 8 characters"]
  }
}
```

### Custom Error Handling with Logging

```csharp
private async Task SubmitForm()
{
    try
    {
        var response = await HttpClient.PostAsJsonAsync("/api/submit", model);
        
        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Form submitted successfully!", Severity.Success);
            NavigationManager.NavigateTo("/success");
        }
        else
        {
            await customValidator!.DisplayErrorsAsync(response);
            Logger.LogWarning("Form submission failed with status {StatusCode}", response.StatusCode);
        }
    }
    catch (HttpRequestException ex)
    {
        Logger.LogError(ex, "Network error during form submission");
        customValidator?.AddError(string.Empty, "Unable to connect to the server. Please try again.");
        customValidator?.NotifyValidationStateChanged();
    }
}
```

### Field-Specific Validation

```csharp
private void ValidateCustomRules()
{
    customValidator?.ClearErrors();
    
    if (model.StartDate > model.EndDate)
    {
        customValidator?.AddError(nameof(model.EndDate), "End date must be after start date");
    }
    
    if (model.Age < 18)
    {
        customValidator?.AddError(nameof(model.Age), "Must be 18 or older");
    }
    
    if (customValidator?.HasErrors ?? false)
    {
        customValidator.NotifyValidationStateChanged();
    }
}
```

### Without MudBlazor (Snackbar)

The component gracefully handles scenarios where MudBlazor's `ISnackbar` is not available. In such cases, RFC 7807 problem details are displayed as form-level validation errors instead of toast notifications.

## Testing

The component is designed to be easily testable:

```csharp
[Fact]
public void AddError_AddsErrorToField()
{
    // Arrange
    var editContext = new EditContext(new MyModel());
    var validator = new CustomValidator();
    // Set up cascading parameter and initialize
    
    // Act
    validator.AddError("Email", "Email is required");
    
    // Assert
    Assert.True(validator.HasErrors);
}
```

## Best Practices

1. **Always use `@ref`** to get a reference to the CustomValidator component
2. **Check response status** before calling `DisplayErrorsAsync`
3. **Use CancellationToken** for long-running operations
4. **Clear errors** before resubmission if needed
5. **Log errors** for debugging and monitoring
6. **Handle exceptions** when calling API methods
7. **Use empty string** for form-level errors that don't relate to specific fields

## Migration from Previous Version

### Breaking Changes

1. **Method Rename**: `DisplayErrors(HttpResponseMessage)` ? `DisplayErrorsAsync(HttpResponseMessage, CancellationToken)`
   ```csharp
   // Before
   customValidator.DisplayErrors(response);
   
   // After
   await customValidator.DisplayErrorsAsync(response);
   ```

2. **Nullable Snackbar**: `ISnackbar` is now nullable and optional
   
3. **Removed Method**: `DisplayErrors()` (no parameters) - use `NotifyValidationStateChanged()` instead
   ```csharp
   // Before
   customValidator.DisplayErrors();
   
   // After
   customValidator.NotifyValidationStateChanged();
   ```

### Non-Breaking Changes

- Added `ILogger<CustomValidator>` injection (optional)
- Added `HasErrors` property
- Added `NotifyValidationStateChanged()` method
- Improved null safety and error handling
- Migrated from Newtonsoft.Json to System.Text.Json

## Error Formats Supported

### 1. RFC 7807 Problem Details
```json
{
  "status": 400,
  "detail": "Validation failed"
}
```

### 2. Validation Errors Dictionary
```json
{
  "Email": ["Email is required", "Email format is invalid"],
  "Password": ["Password is required"]
}
```

### 3. ServerResponse Object
```csharp
new ServerResponse
{
    Message = "Validation failed",
    Errors = new List<string> { "Error 1", "Error 2" }
}
```

## License

This component is part of the Craft framework. See the main project LICENSE file for details.
