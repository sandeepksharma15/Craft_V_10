# CustomValidator - HttpServiceResult Integration

## Overview

The `CustomValidator` component now provides seamless integration with `HttpServiceResult<T>` from the `Craft.HttpServices` library, making form validation in Blazor applications significantly simpler.

## New Methods

### 1. `DisplayErrors<T>(HttpServiceResult<T> serviceResult)`

Displays validation errors directly from an `HttpServiceResult`. Automatically detects and handles:
- Field-specific validation errors (as JSON dictionary)
- Form-level error messages

**Signature:**
```csharp
public void DisplayErrors<T>(HttpServiceResult<T> serviceResult)
```

**Example:**
```csharp
@code {
    private CustomValidator? customValidator;
    private UserModel model = new();
    
    [Inject] private IHttpChangeService<User, UserModel, UserDto> UserService { get; set; } = default!;
    
    private async Task HandleValidSubmit()
    {
        var result = await UserService.AddAsync(model);
        
        if (!result.Success)
        {
            // One line - that's it!
            customValidator?.DisplayErrors(result);
            return;
        }
        
        // Handle success
        NavigationManager.NavigateTo("/users");
    }
}
```

### 2. `DisplayErrorsAsync<T>(HttpServiceResult<T>, HttpResponseMessage?, CancellationToken)`

Enhanced version that attempts to parse detailed validation errors from the HTTP response message if provided.

**Signature:**
```csharp
public async Task DisplayErrorsAsync<T>(
    HttpServiceResult<T> serviceResult,
    HttpResponseMessage? responseMessage = null,
    CancellationToken cancellationToken = default)
```

**Example with Response Message:**
```csharp
@code {
    private async Task HandleValidSubmit()
    {
        HttpResponseMessage? response = null;
        
        try
        {
            var result = await UserService.AddAsync(model);
            
            if (!result.Success)
            {
                // Pass the response for more detailed error parsing
                await customValidator!.DisplayErrorsAsync(result, response);
                return;
            }
            
            NavigationManager.NavigateTo("/users");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting form");
            customValidator?.AddError(string.Empty, "An unexpected error occurred");
            customValidator?.NotifyValidationStateChanged();
        }
    }
}
```

## How It Works

### Smart Error Detection

The component intelligently detects the error format:

#### 1. **JSON Dictionary Format** (Field-Specific Errors)
```json
{
  "Email": ["Email is required", "Email format is invalid"],
  "Password": ["Password must be at least 8 characters"]
}
```

If the error string can be parsed as JSON with field-value pairs, errors are automatically mapped to their respective fields.

#### 2. **Simple String Array** (Form-Level Errors)
```csharp
result.Errors = ["User not found", "Session expired"];
```

If errors cannot be parsed as JSON dictionary, they're treated as form-level errors.

## Complete Example

### Simple CRUD Form

```razor
@page "/users/add"
@using Craft.Components.Generic
@inject IHttpChangeService<User, UserViewModel, UserDto> UserService
@inject NavigationManager NavigationManager
@inject ILogger<AddUser> Logger

<PageTitle>Add User</PageTitle>

<MudContainer MaxWidth="MaxWidth.Medium" Class="mt-4">
    <MudCard>
        <MudCardHeader>
            <CardHeaderContent>
                <MudText Typo="Typo.h5">Add New User</MudText>
            </CardHeaderContent>
        </MudCardHeader>
        <MudCardContent>
            <EditForm Model="@model" OnValidSubmit="@HandleValidSubmit">
                <DataAnnotationsValidator />
                <CustomValidator @ref="customValidator" />

                <MudTextField @bind-Value="model.Email"
                             For="@(() => model.Email)"
                             Label="Email"
                             Variant="Variant.Outlined"
                             Class="mb-3" />

                <MudTextField @bind-Value="model.FirstName"
                             For="@(() => model.FirstName)"
                             Label="First Name"
                             Variant="Variant.Outlined"
                             Class="mb-3" />

                <MudTextField @bind-Value="model.LastName"
                             For="@(() => model.LastName)"
                             Label="Last Name"
                             Variant="Variant.Outlined"
                             Class="mb-3" />

                <MudTextField @bind-Value="model.Password"
                             For="@(() => model.Password)"
                             Label="Password"
                             InputType="InputType.Password"
                             Variant="Variant.Outlined"
                             Class="mb-3" />

                <ValidationSummary />

                <MudCardActions>
                    <MudButton ButtonType="ButtonType.Submit"
                              Variant="Variant.Filled"
                              Color="Color.Primary"
                              Disabled="@isSubmitting">
                        @if (isSubmitting)
                        {
                            <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
                        }
                        Save User
                    </MudButton>
                    <MudButton Variant="Variant.Text"
                              OnClick="@(() => NavigationManager.NavigateTo("/users"))">
                        Cancel
                    </MudButton>
                </MudCardActions>
            </EditForm>
        </MudCardContent>
    </MudCard>
</MudContainer>

@code {
    private CustomValidator? customValidator;
    private UserViewModel model = new();
    private bool isSubmitting = false;

    private async Task HandleValidSubmit()
    {
        try
        {
            isSubmitting = true;
            customValidator?.ClearErrors();

            var result = await UserService.AddAsync(model);

            if (!result.Success)
            {
                // ONE LINE - handles all validation errors!
                customValidator?.DisplayErrors(result);
                return;
            }

            // Success
            NavigationManager.NavigateTo("/users");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding user");
            customValidator?.AddError(string.Empty, "An unexpected error occurred. Please try again.");
            customValidator?.NotifyValidationStateChanged();
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
```

### Update Form

```csharp
@code {
    [Parameter] public string Id { get; set; } = string.Empty;

    private CustomValidator? customValidator;
    private UserViewModel model = new();
    private bool isLoading = true;
    private bool isSubmitting = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadUser();
    }

    private async Task LoadUser()
    {
        try
        {
            isLoading = true;
            
            var result = await UserService.GetAsync(Id);
            
            if (!result.Success || result.Data is null)
            {
                customValidator?.DisplayErrors(result);
                return;
            }
            
            // Map to view model
            model = result.Data.Adapt<UserViewModel>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user");
            customValidator?.AddError(string.Empty, "Failed to load user");
            customValidator?.NotifyValidationStateChanged();
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task HandleValidSubmit()
    {
        try
        {
            isSubmitting = true;
            customValidator?.ClearErrors();

            var result = await UserService.UpdateAsync(model);

            if (!result.Success)
            {
                customValidator?.DisplayErrors(result);
                return;
            }

            // Success
            Snackbar?.Add("User updated successfully", Severity.Success);
            NavigationManager.NavigateTo("/users");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating user");
            customValidator?.AddError(string.Empty, "An unexpected error occurred. Please try again.");
            customValidator?.NotifyValidationStateChanged();
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
```

### Delete Confirmation

```csharp
@code {
    private async Task DeleteUser(string id)
    {
        bool? confirmed = await DialogService.ShowMessageBox(
            "Confirm Delete",
            "Are you sure you want to delete this user?",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
            return;

        try
        {
            var result = await UserService.DeleteAsync(id);

            if (!result.Success)
            {
                customValidator?.DisplayErrors(result);
                return;
            }

            Snackbar?.Add("User deleted successfully", Severity.Success);
            await RefreshList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting user");
            customValidator?.AddError(string.Empty, "Failed to delete user");
            customValidator?.NotifyValidationStateChanged();
        }
    }
}
```

## Comparison: Before vs After

### BEFORE (Without Integration)
```csharp
private async Task HandleValidSubmit()
{
    var result = await UserService.AddAsync(model);
    
    if (!result.Success)
    {
        // Manual error handling - verbose and error-prone
        if (result.Errors != null && result.Errors.Count > 0)
        {
            foreach (var error in result.Errors)
            {
                // Try to parse as JSON
                try
                {
                    var errors = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(error);
                    if (errors != null)
                    {
                        foreach (var kvp in errors)
                        {
                            foreach (var msg in kvp.Value)
                            {
                                customValidator?.AddError(kvp.Key, msg);
                            }
                        }
                    }
                }
                catch
                {
                    // Not JSON, add as form-level error
                    customValidator?.AddError(string.Empty, error);
                }
            }
            customValidator?.NotifyValidationStateChanged();
        }
        return;
    }
    
    NavigationManager.NavigateTo("/users");
}
```

### AFTER (With Integration)
```csharp
private async Task HandleValidSubmit()
{
    var result = await UserService.AddAsync(model);
    
    if (!result.Success)
    {
        // One line - simple and clean!
        customValidator?.DisplayErrors(result);
        return;
    }
    
    NavigationManager.NavigateTo("/users");
}
```

## Error Format Examples

### Server Returns Field-Specific Errors

**Server Response:**
```json
{
  "Data": null,
  "Success": false,
  "Errors": [
    "{\"Email\":[\"Email is required\",\"Email format is invalid\"],\"Password\":[\"Password must be at least 8 characters\"]}"
  ],
  "StatusCode": 400
}
```

**Result:**
- "Email is required" ? displayed under Email field
- "Email format is invalid" ? displayed under Email field
- "Password must be at least 8 characters" ? displayed under Password field

### Server Returns General Errors

**Server Response:**
```json
{
  "Data": null,
  "Success": false,
  "Errors": [
    "User not found",
    "Session expired"
  ],
  "StatusCode": 404
}
```

**Result:**
- Both errors displayed in `<ValidationSummary />` as form-level errors

## Benefits

? **Simplicity** - One line of code instead of 20+  
? **Automatic Detection** - Handles both field-specific and form-level errors  
? **Type Safety** - Generic method works with any `HttpServiceResult<T>`  
? **Consistent** - Same pattern across all forms  
? **Less Error-Prone** - No manual JSON parsing  
? **Better UX** - Errors displayed in the right place automatically  
? **Maintainable** - Changes to error format handled in one place  

## Best Practices

### 1. Always Clear Errors Before Submission
```csharp
private async Task HandleValidSubmit()
{
    customValidator?.ClearErrors(); // Clear previous errors
    
    var result = await UserService.AddAsync(model);
    
    if (!result.Success)
    {
        customValidator?.DisplayErrors(result);
        return;
    }
}
```

### 2. Handle Exceptions Gracefully
```csharp
try
{
    var result = await UserService.AddAsync(model);
    
    if (!result.Success)
    {
        customValidator?.DisplayErrors(result);
        return;
    }
}
catch (HttpRequestException ex)
{
    Logger.LogError(ex, "Network error");
    customValidator?.AddError(string.Empty, "Network error. Please check your connection.");
    customValidator?.NotifyValidationStateChanged();
}
catch (Exception ex)
{
    Logger.LogError(ex, "Unexpected error");
    customValidator?.AddError(string.Empty, "An unexpected error occurred.");
    customValidator?.NotifyValidationStateChanged();
}
```

### 3. Use Loading States
```csharp
<MudButton ButtonType="ButtonType.Submit"
          Variant="Variant.Filled"
          Color="Color.Primary"
          Disabled="@isSubmitting">
    @if (isSubmitting)
    {
        <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
    }
    Submit
</MudButton>

@code {
    private bool isSubmitting = false;
    
    private async Task HandleValidSubmit()
    {
        try
        {
            isSubmitting = true;
            // ... handle submission
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
```

### 4. Check HasErrors Property
```csharp
if (customValidator?.HasErrors ?? false)
{
    Logger.LogWarning("Form has validation errors");
    return;
}
```

## API Reference

### DisplayErrors<T>(HttpServiceResult<T>)

**Parameters:**
- `serviceResult` - The HTTP service result containing validation errors

**Behavior:**
1. Returns immediately if `serviceResult.Success` is `true`
2. Returns immediately if no errors exist
3. Attempts to parse errors as JSON dictionary (field-specific)
4. Falls back to form-level errors if JSON parsing fails
5. Automatically calls `NotifyValidationStateChanged()`

**Throws:**
- `ArgumentNullException` - If `serviceResult` is null
- `ObjectDisposedException` - If component has been disposed

### DisplayErrorsAsync<T>(HttpServiceResult<T>, HttpResponseMessage?, CancellationToken)

**Parameters:**
- `serviceResult` - The HTTP service result containing validation errors
- `responseMessage` - Optional HTTP response for enhanced error parsing
- `cancellationToken` - Cancellation token

**Behavior:**
1. Returns immediately if `serviceResult.Success` is `true`
2. If `responseMessage` is provided, attempts detailed error parsing
3. Falls back to simple error display if response parsing fails
4. Supports cancellation via `CancellationToken`

**Throws:**
- `ArgumentNullException` - If `serviceResult` is null
- `ObjectDisposedException` - If component has been disposed
- `OperationCanceledException` - If operation is cancelled

## Migration Guide

If you're currently handling `HttpServiceResult` errors manually:

### Step 1: Remove Manual Error Handling
```csharp
// REMOVE THIS
if (result.Errors != null)
{
    foreach (var error in result.Errors)
    {
        customValidator?.AddError(string.Empty, error);
    }
    customValidator?.NotifyValidationStateChanged();
}
```

### Step 2: Use New Method
```csharp
// ADD THIS
customValidator?.DisplayErrors(result);
```

### Step 3: Test Your Forms
Ensure errors are displayed correctly for both field-specific and form-level errors.

## See Also

- [CustomValidator.README.md](./CustomValidator.README.md) - Full component documentation
- [SNACKBAR_REMOVAL.md](./SNACKBAR_REMOVAL.md) - Design decision documentation
- [HttpChangeService](../../5. Web/Craft.HttpServices/Services/HttpChangeService.cs) - HTTP service documentation
