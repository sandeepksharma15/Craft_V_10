using Craft.UiComponents;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that manages async data loading states with loading, error, and success states.
/// Provides a consistent way to handle async operations with loading indicators and error handling.
/// </summary>
public partial class DataLoader : CraftComponent
{
    /// <summary>
    /// Gets or sets a value indicating whether data is currently being loaded.
    /// </summary>
    [Parameter, EditorRequired] public bool IsLoading { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an error occurred during loading.
    /// </summary>
    [Parameter] public bool HasError { get; set; }

    /// <summary>
    /// Gets or sets the custom loading template.
    /// If not provided, a default spinner overlay will be displayed.
    /// </summary>
    [Parameter] public RenderFragment? Loading { get; set; }

    /// <summary>
    /// Gets or sets the custom error template.
    /// If not provided, a default error alert with retry option will be displayed.
    /// </summary>
    [Parameter] public RenderFragment? Error { get; set; }

    /// <summary>
    /// Gets or sets the color of the default loading spinner.
    /// Default is Primary.
    /// </summary>
    [Parameter] public Color LoadingColor { get; set; } = Color.Primary;

    /// <summary>
    /// Gets or sets the error title displayed in the default error template.
    /// Default is "Error".
    /// </summary>
    [Parameter] public string ErrorTitle { get; set; } = "Error";

    /// <summary>
    /// Gets or sets the error message displayed in the default error template.
    /// Default is "Something went wrong while loading the data. Please try again."
    /// </summary>
    [Parameter] public string ErrorMessage { get; set; } = "Something went wrong while loading the data. Please try again.";

    /// <summary>
    /// Gets or sets the text displayed on the retry button.
    /// Default is "Retry".
    /// </summary>
    [Parameter] public string RetryText { get; set; } = "Retry";

    /// <summary>
    /// Gets or sets the callback invoked when the retry button is clicked.
    /// If not set, the retry button will not be displayed.
    /// </summary>
    [Parameter] public EventCallback OnRetry { get; set; }

    /// <summary>
    /// Handles the retry button click event.
    /// </summary>
    private async Task HandleRetryAsync()
    {
        if (OnRetry.HasDelegate)
            await OnRetry.InvokeAsync();
    }
}
