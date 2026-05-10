using Craft.UiBuilders.Services.AppStatus;
using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components;

/// <summary>
/// A one-line status strip that displays application progress and status messages
/// posted by any component or service via <see cref="IAppStatusService"/>.
/// Place this component once in your main layout to activate status reporting.
/// </summary>
public partial class CraftStatusBar : CraftComponent
{
    [Inject] private IAppStatusService StatusService { get; set; } = default!;

    private async Task OnStatusChangedAsync()
        => await InvokeAsync(StateHasChanged);

    protected override void OnInitialized()
    {
        StatusService.OnStatusChanged += OnStatusChangedAsync;
        base.OnInitialized();
    }

    private async Task HandleCloseAsync()
        => await StatusService.ClearAsync();

    protected override ValueTask DisposeAsyncCore()
    {
        StatusService.OnStatusChanged -= OnStatusChangedAsync;
        return ValueTask.CompletedTask;
    }
}
