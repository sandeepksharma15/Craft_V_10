using System.Net.Http.Json;
using System.Text.Json;
using Craft.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Craft.Components.Generic;

/// <summary>
/// A custom validation component for Blazor forms that integrates with EditContext
/// to display server-side validation errors and custom validation messages.
/// </summary>
public sealed class CustomValidator : ComponentBase, IDisposable
{
    private ValidationMessageStore? _messageStore;
    private bool _disposed;

    /// <summary>
    /// Gets the EditContext from the parent EditForm component.
    /// </summary>
    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    /// <summary>
    /// Gets the ISnackbar service for displaying toast notifications.
    /// Optional: Can be null if MudBlazor is not configured.
    /// </summary>
    [Inject]
    private ISnackbar? Snackbar { get; set; }

    /// <summary>
    /// Gets the ILogger service for logging errors and warnings.
    /// Optional: Can be null if logging is not configured.
    /// </summary>
    [Inject]
    private ILogger<CustomValidator>? Logger { get; set; }

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

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException(
                $"{nameof(CustomValidator)} requires a cascading " +
                $"parameter of type {nameof(EditContext)}. " +
                $"For example, you can use {nameof(CustomValidator)} " +
                $"inside an {nameof(EditForm)}.");
        }

        _messageStore = new ValidationMessageStore(CurrentEditContext);

        CurrentEditContext.OnValidationRequested += OnValidationRequested;
        CurrentEditContext.OnFieldChanged += OnFieldChanged;
    }

    /// <summary>
    /// Adds a validation error for a specific field.
    /// </summary>
    /// <param name="fieldName">The name of the field to add the error to. Use empty string for form-level errors.</param>
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

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs args)
    {
        _messageStore?.Clear();
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        _messageStore?.Clear(args.FieldIdentifier);
    }

    private void AddFormLevelError(string message)
    {
        _messageStore?.Add(CurrentEditContext!.Field(string.Empty), message);
    }
}
