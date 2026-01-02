# CustomValidator - Before & After Comparison

## Key Improvements at a Glance

### Class Declaration
**BEFORE:**
```csharp
public class CustomValidator : ComponentBase
```

**AFTER:**
```csharp
public sealed class CustomValidator : ComponentBase, IDisposable
```
? Sealed to prevent inheritance issues  
? Implements IDisposable for proper cleanup  

---

### Fields & Properties
**BEFORE:**
```csharp
private ValidationMessageStore _messageStore;

[CascadingParameter] private EditContext CurrentEditContext { get; set; }
[Inject] private ISnackbar Snackbar { get; set; }
```

**AFTER:**
```csharp
private ValidationMessageStore? _messageStore;
private bool _disposed;

[CascadingParameter] private EditContext? CurrentEditContext { get; set; }
[Inject] private ISnackbar? Snackbar { get; set; }
[Inject] private ILogger<CustomValidator>? Logger { get; set; }

public bool HasErrors { get; }
```
? Nullable reference types  
? Disposal tracking  
? Optional logger injection  
? HasErrors property for validation state  

---

### Initialization
**BEFORE:**
```csharp
protected override void OnInitialized()
{
    if (CurrentEditContext == null)
        throw new InvalidOperationException(...);

    _messageStore = new ValidationMessageStore(CurrentEditContext);

    CurrentEditContext.OnValidationRequested += (s, e) =>
        _messageStore.Clear();
    CurrentEditContext.OnFieldChanged += (s, e) =>
        _messageStore.Clear(e.FieldIdentifier);
}
```

**AFTER:**
```csharp
protected override void OnInitialized()
{
    if (CurrentEditContext is null)
    {
        throw new InvalidOperationException(...);
    }

    _messageStore = new ValidationMessageStore(CurrentEditContext);

    CurrentEditContext.OnValidationRequested += OnValidationRequested;
    CurrentEditContext.OnFieldChanged += OnFieldChanged;
}

private void OnValidationRequested(object? sender, ValidationRequestedEventArgs args)
{
    _messageStore?.Clear();
}

private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
{
    _messageStore?.Clear(args.FieldIdentifier);
}
```
? Named event handlers for proper unsubscription  
? Null-safe operations  
? Pattern matching with `is null`  

---

### AddError Method
**BEFORE:**
```csharp
public void AddError(string key, string value)
{
    _messageStore.Add(CurrentEditContext.Field(key), value);
}
```

**AFTER:**
```csharp
/// <summary>
/// Adds a validation error for a specific field.
/// </summary>
/// <param name="fieldName">The name of the field to add the error to.</param>
/// <param name="errorMessage">The error message to display.</param>
/// <exception cref="ArgumentNullException">Thrown when fieldName or errorMessage is null.</exception>
/// <exception cref="ObjectDisposedException">Thrown when the component has been disposed.</exception>
public void AddError(string fieldName, string errorMessage)
{
    ArgumentNullException.ThrowIfNull(fieldName);
    ArgumentNullException.ThrowIfNull(errorMessage);
    ObjectDisposedException.ThrowIf(_disposed, this);

    _messageStore?.Add(CurrentEditContext!.Field(fieldName), errorMessage);
}
```
? XML documentation  
? Parameter validation  
? Disposal check  
? Null-safe operations  

---

### ClearErrors Method
**BEFORE:**
```csharp
public void ClearErrors()
{
    _messageStore.Clear();
    CurrentEditContext.NotifyValidationStateChanged();
}
```

**AFTER:**
```csharp
/// <summary>
/// Clears all validation errors and notifies the EditContext of the state change.
/// </summary>
/// <exception cref="ObjectDisposedException">Thrown when the component has been disposed.</exception>
public void ClearErrors()
{
    ObjectDisposedException.ThrowIf(_disposed, this);

    _messageStore?.Clear();
    CurrentEditContext?.NotifyValidationStateChanged();
}
```
? XML documentation  
? Disposal check  
? Null-safe operations  

---

### DisplayErrors - HTTP Response (Major Change)
**BEFORE:**
```csharp
public async Task DisplayErrors(HttpResponseMessage responseMessage)
{
    JObject errMsg = JObject.Parse(await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false));

    if (errMsg.SelectToken("detail") != null)
    {
        string status = errMsg!.SelectToken("status")?.Value<string>();
        string detail = errMsg!.SelectToken("detail")?.Value<string>();

        Snackbar.Add($"{status}: {detail}", Severity.Error);
    }
    else
        DisplayErrors(await responseMessage.Content.ReadFromJsonAsync<Dictionary<string, List<string>>>().ConfigureAwait(false));
}
```

**AFTER:**
```csharp
/// <summary>
/// Displays validation errors from an HTTP response message.
/// Handles both RFC 7807 Problem Details format and dictionary-based validation errors.
/// </summary>
/// <param name="responseMessage">The HTTP response message containing validation errors.</param>
/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
/// <exception cref="ArgumentNullException">Thrown when responseMessage is null.</exception>
/// <exception cref="ObjectDisposedException">Thrown when the component has been disposed.</exception>
public async Task DisplayErrorsAsync(HttpResponseMessage responseMessage, CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(responseMessage);
    ObjectDisposedException.ThrowIf(_disposed, this);

    try
    {
        var contentString = await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(contentString))
        {
            Logger?.LogWarning("Received empty error response from server");
            AddFormLevelError("An error occurred but no details were provided by the server.");
            return;
        }

        using var document = JsonDocument.Parse(contentString);
        var root = document.RootElement;

        // Check for RFC 7807 Problem Details format
        if (root.TryGetProperty("detail", out var detailElement))
        {
            var status = root.TryGetProperty("status", out var statusElement)
                ? statusElement.ToString()
                : responseMessage.StatusCode.ToString();
            var detail = detailElement.GetString() ?? "Unknown error";

            if (Snackbar is not null)
            {
                Snackbar.Add($"{status}: {detail}", Severity.Error);
            }
            else
            {
                AddFormLevelError($"{status}: {detail}");
                CurrentEditContext?.NotifyValidationStateChanged();
            }
        }
        // Check for validation errors dictionary format
        else if (root.ValueKind == JsonValueKind.Object)
        {
            var errors = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(contentString);
            if (errors is not null && errors.Count > 0)
            {
                DisplayErrors(errors);
            }
            else
            {
                AddFormLevelError("Validation failed but no specific errors were provided.");
                CurrentEditContext?.NotifyValidationStateChanged();
            }
        }
        else
        {
            AddFormLevelError("Unable to parse error response from server.");
            CurrentEditContext?.NotifyValidationStateChanged();
        }
    }
    catch (JsonException ex)
    {
        Logger?.LogError(ex, "Failed to parse JSON error response");
        AddFormLevelError("An error occurred while processing the server response.");
        CurrentEditContext?.NotifyValidationStateChanged();
    }
    catch (Exception ex)
    {
        Logger?.LogError(ex, "Unexpected error while displaying validation errors");
        AddFormLevelError("An unexpected error occurred.");
        CurrentEditContext?.NotifyValidationStateChanged();
    }
}
```
? Renamed to `DisplayErrorsAsync` (clearer name)  
? CancellationToken support  
? Migrated from Newtonsoft.Json to System.Text.Json  
? Comprehensive error handling  
? Logging support  
? Handles empty responses  
? Handles invalid JSON  
? Handles missing Snackbar  
? XML documentation  
? Parameter validation  
? Disposal check  

---

### DisplayErrors - ServerResponse
**BEFORE:**
```csharp
public void DisplayErrors(ServerResponse responseMessage)
{
    _messageStore.Add(CurrentEditContext.Field(string.Empty), responseMessage.Message);

    responseMessage.Errors?.ForEach(err => _messageStore.Add(CurrentEditContext.Field(string.Empty), err));

    CurrentEditContext.NotifyValidationStateChanged();
}
```

**AFTER:**
```csharp
/// <summary>
/// Displays validation errors from a ServerResponse object.
/// </summary>
/// <param name="serverResponse">The server response containing validation errors.</param>
/// <exception cref="ArgumentNullException">Thrown when serverResponse is null.</exception>
/// <exception cref="ObjectDisposedException">Thrown when the component has been disposed.</exception>
public void DisplayErrors(ServerResponse serverResponse)
{
    ArgumentNullException.ThrowIfNull(serverResponse);
    ObjectDisposedException.ThrowIf(_disposed, this);

    if (!string.IsNullOrWhiteSpace(serverResponse.Message))
    {
        AddFormLevelError(serverResponse.Message);
    }

    if (serverResponse.Errors is not null && serverResponse.Errors.Count > 0)
    {
        foreach (var error in serverResponse.Errors)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                AddFormLevelError(error);
            }
        }
    }

    CurrentEditContext?.NotifyValidationStateChanged();
}
```
? XML documentation  
? Parameter validation  
? Disposal check  
? Null checks  
? Avoids empty error messages  
? Uses foreach instead of ForEach  
? Helper method for form-level errors  

---

### DisplayErrors - Dictionary
**BEFORE:**
```csharp
public void DisplayErrors(Dictionary<string, List<string>> errors)
{
    foreach (KeyValuePair<string, List<string>> err in errors)
        _messageStore.Add(CurrentEditContext.Field(err.Key), err.Value);

    CurrentEditContext.NotifyValidationStateChanged();
}
```

**AFTER:**
```csharp
/// <summary>
/// Displays validation errors from a dictionary of field names to error messages.
/// </summary>
/// <param name="errors">Dictionary where keys are field names and values are lists of error messages.</param>
/// <exception cref="ArgumentNullException">Thrown when errors is null.</exception>
/// <exception cref="ObjectDisposedException">Thrown when the component has been disposed.</exception>
public void DisplayErrors(Dictionary<string, List<string>> errors)
{
    ArgumentNullException.ThrowIfNull(errors);
    ObjectDisposedException.ThrowIf(_disposed, this);

    if (errors.Count == 0)
    {
        return;
    }

    foreach (var (fieldName, errorMessages) in errors)
    {
        if (errorMessages is null || errorMessages.Count == 0)
        {
            continue;
        }

        var field = CurrentEditContext!.Field(fieldName);
        foreach (var message in errorMessages)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _messageStore?.Add(field, message);
            }
        }
    }

    CurrentEditContext?.NotifyValidationStateChanged();
}
```
? XML documentation  
? Parameter validation  
? Disposal check  
? Early return for empty dictionary  
? Null checks for error lists  
? Skips empty messages  
? Tuple deconstruction  
? Single notification after all errors  

---

### NEW: NotifyValidationStateChanged
**BEFORE:**
```csharp
public void DisplayErrors()
{
    CurrentEditContext.NotifyValidationStateChanged();
}
```

**AFTER:**
```csharp
/// <summary>
/// Notifies the EditContext that validation state has changed.
/// Use this when you've manually added errors and want to trigger validation display.
/// </summary>
/// <exception cref="ObjectDisposedException">Thrown when the component has been disposed.</exception>
public void NotifyValidationStateChanged()
{
    ObjectDisposedException.ThrowIf(_disposed, this);
    CurrentEditContext?.NotifyValidationStateChanged();
}
```
? Better method name  
? XML documentation  
? Disposal check  
? Null-safe operation  

---

### NEW: Disposal Pattern
**BEFORE:**
```csharp
// No disposal - memory leak risk!
```

**AFTER:**
```csharp
/// <inheritdoc/>
public void Dispose()
{
    if (_disposed)
    {
        return;
    }

    if (CurrentEditContext is not null)
    {
        CurrentEditContext.OnValidationRequested -= OnValidationRequested;
        CurrentEditContext.OnFieldChanged -= OnFieldChanged;
    }

    _messageStore = null;
    _disposed = true;
}
```
? Proper IDisposable implementation  
? Unsubscribes from events  
? Idempotent (safe to call multiple times)  
? Prevents memory leaks  

---

### NEW: HasErrors Property
**BEFORE:**
```csharp
// No way to check if errors exist!
```

**AFTER:**
```csharp
/// <summary>
/// Gets a value indicating whether the validator currently has any validation errors.
/// </summary>
public bool HasErrors
{
    get
    {
        if (_messageStore is null || CurrentEditContext is null)
        {
            return false;
        }

        return CurrentEditContext.GetValidationMessages().Any();
    }
}
```
? Easy validation state checking  
? Null-safe implementation  
? XML documentation  

---

## Usage Comparison

### BEFORE:
```csharp
<CustomValidator @ref="validator" />

@code {
    private CustomValidator validator;
    
    // Sync operation (not ideal for HTTP calls)
    validator.DisplayErrors(response);
    
    // Unclear method name
    validator.DisplayErrors();
    
    // No way to check if errors exist
    // ? Can't do: if (validator.HasErrors) { }
}
```

### AFTER:
```csharp
<CustomValidator @ref="validator" />

@code {
    private CustomValidator? validator;
    
    // Proper async with cancellation
    await validator!.DisplayErrorsAsync(response, cancellationToken);
    
    // Clear method name
    validator.NotifyValidationStateChanged();
    
    // Can check for errors
    if (validator?.HasErrors ?? false)
    {
        Logger.LogWarning("Validation failed");
    }
    
    // Component properly disposes itself
}
```

---

## Summary of Changes

| Category | Changes |
|----------|---------|
| **Null Safety** | ? Full nullable reference type support throughout |
| **Disposal** | ? Implements IDisposable, unsubscribes events |
| **JSON Parsing** | ? Migrated from Newtonsoft.Json to System.Text.Json |
| **Error Handling** | ? Try-catch blocks, logging, graceful failures |
| **API Improvements** | ? HasErrors property, NotifyValidationStateChanged, CancellationToken |
| **Documentation** | ? Complete XML documentation on all public members |
| **Validation** | ? ArgumentNullException.ThrowIfNull, ObjectDisposedException checks |
| **Dependencies** | ? Optional ISnackbar and ILogger injection |
| **Performance** | ? Single notification, efficient JSON parsing, proper disposal |
| **Testability** | ? All dependencies injectable, public API designed for testing |

---

## Lines of Code

| Metric | Before | After | Change |
|--------|--------|-------|--------|
| Total Lines | ~75 | ~280 | +205 |
| Documentation Lines | 0 | ~100 | +100 |
| Error Handling Lines | ~5 | ~50 | +45 |
| Null Safety Checks | ~1 | ~30 | +29 |

**Why more lines?**
- Comprehensive XML documentation
- Proper error handling and logging
- Null safety checks throughout
- Better separation of concerns
- More robust implementation

**Result:** Production-ready, maintainable, testable code that follows all .NET 10 best practices! ??
