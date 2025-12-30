using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents an HTML dialog element for modal or non-modal dialogs.
/// </summary>
/// <remarks>
/// The dialog element represents a dialog box or interactive component.
/// It can be displayed as a modal (showModal()) or non-modal (show()) dialog.
/// Use IsModal to control whether the dialog is displayed as a modal.
/// </remarks>
/// <example>
/// <code>
/// &lt;CraftDialog @bind-IsOpen="isDialogOpen" IsModal="true"&gt;
///     &lt;h2&gt;Dialog Title&lt;/h2&gt;
///     &lt;p&gt;Dialog content goes here.&lt;/p&gt;
///     &lt;button @onclick="CloseDialog"&gt;Close&lt;/button&gt;
/// &lt;/CraftDialog&gt;
/// </code>
/// </example>
public partial class CraftDialog : CraftComponent
{
    private bool _isOpen;

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets or sets whether the dialog is open.
    /// </summary>
    [Parameter]
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen != value)
            {
                _isOpen = value;
                _ = ToggleDialogAsync(value);
            }
        }
    }

    /// <summary>
    /// Event callback for when the dialog open state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the dialog should be displayed as a modal.
    /// Modal dialogs prevent interaction with content outside the dialog.
    /// </summary>
    [Parameter] public bool IsModal { get; set; } = true;

    /// <summary>
    /// Event callback for when the dialog is opened.
    /// </summary>
    [Parameter] public EventCallback OnOpen { get; set; }

    /// <summary>
    /// Event callback for when the dialog is closed.
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    /// <summary>
    /// Gets the component-specific CSS class.
    /// </summary>
    /// <returns>The CSS class "craft-dialog" with optional modal class.</returns>
    protected override string? GetComponentCssClass()
    {
        var baseClass = "craft-dialog";
        return IsModal ? $"{baseClass} craft-dialog-modal" : baseClass;
    }

    /// <summary>
    /// Opens the dialog.
    /// </summary>
    public async Task OpenAsync()
    {
        if (!IsOpen)
        {
            IsOpen = true;
            await IsOpenChanged.InvokeAsync(true);
            if (OnOpen.HasDelegate)
            {
                await OnOpen.InvokeAsync();
            }
        }
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    public async Task CloseAsync()
    {
        if (IsOpen)
        {
            IsOpen = false;
            await IsOpenChanged.InvokeAsync(false);
            if (OnClose.HasDelegate)
            {
                await OnClose.InvokeAsync();
            }
        }
    }

    private async Task ToggleDialogAsync(bool open)
    {
        try
        {
            if (open)
            {
                var method = IsModal ? "showModal" : "show";
                await JSRuntime.InvokeVoidAsync($"eval", $"document.getElementById('{GetId()}')?.{method}()");

                if (OnOpen.HasDelegate)
                {
                    await OnOpen.InvokeAsync();
                }
            }
            else
            {
                await JSRuntime.InvokeVoidAsync($"eval", $"document.getElementById('{GetId()}')?.close()");

                if (OnClose.HasDelegate)
                {
                    await OnClose.InvokeAsync();
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Error toggling dialog state");
        }
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore()
    {
        if (IsOpen)
        {
            await CloseAsync();
        }

        await base.DisposeAsyncCore();
    }
}
