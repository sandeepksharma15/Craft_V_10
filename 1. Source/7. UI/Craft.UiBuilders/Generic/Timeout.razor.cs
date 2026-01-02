using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Generic;

/// <summary>
/// A component that renders content and automatically removes it after a specified timeout.
/// Useful for temporary notifications and messages.
/// </summary>
public partial class Timeout : CraftComponent
{
    private bool _expired;
    private Timer? _timer;

    /// <summary>
    /// Gets or sets the duration in milliseconds before the content expires.
    /// </summary>
    [Parameter]
    public int DurationMs { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the callback invoked when the timeout expires.
    /// </summary>
    [Parameter]
    public EventCallback OnExpired { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _timer = new Timer(async _ =>
        {
            _expired = true;
            await InvokeAsync(async () =>
            {
                StateHasChanged();
                if (OnExpired.HasDelegate)
                {
                    await OnExpired.InvokeAsync();
                }
            });
        }, null, DurationMs, System.Threading.Timeout.Infinite);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_timer != null)
        {
            await _timer.DisposeAsync();
            _timer = null;
        }

        await base.DisposeAsyncCore();
    }
}
