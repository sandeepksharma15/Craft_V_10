using Craft.Components.Generic;
using Craft.Domain;
using Craft.QuerySpec;
using Craft.UiComponents;
using Microsoft.AspNetCore.Components;

namespace Craft.UiBuilders.Components.Dashboard;

/// <summary>
/// A self-contained dashboard card that fetches and displays an animated entity count with loading and error states.
/// </summary>
/// <typeparam name="TEntity">The entity type for which to display the count.</typeparam>
public partial class CraftEntityCountCard<TEntity> : CraftComponent, IDisposable
    where TEntity : class, IEntity, IModel, new()
{
    private long _count;
    private bool _isLoading = true;
    private bool _hasError;
    private string _href = string.Empty;
    private CancellationTokenSource? _cts;

    [CascadingParameter] public HandleError? HandleError { get; set; }

    [Inject] public required IHttpService<TEntity> EntityService { get; set; }

    /// <summary>Gets or sets the icon displayed on the card.</summary>
    [Parameter, EditorRequired] public required string Icon { get; set; }

    /// <summary>Gets or sets the title/label for the card.</summary>
    [Parameter, EditorRequired] public required string Title { get; set; }

    /// <summary>Gets or sets the route to navigate to when clicking "More Info".</summary>
    [Parameter]
    public string? Href
    {
        get => _href;
        set => _href = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the animation duration in milliseconds for the running number effect.
    /// Defaults to 500ms.
    /// </summary>
    [Parameter] public int AnimationDuration { get; set; } = 500;

    protected override async Task OnInitializedAsync()
    {
        _cts = new CancellationTokenSource();

        try
        {
            _isLoading = true;
            _hasError = false;

            var result = await EntityService.GetCountAsync(_cts.Token);

            if (result.IsSuccess)
            {
                _count = result.Value;
            }
            else
            {
                _hasError = true;

                if (result.Errors?.Count > 0)
                {
                    var errorMessage = string.Join(", ", result.Errors);
                    HandleError?.ProcessError(new InvalidOperationException(errorMessage));
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Component was disposed during fetch — expected, not an error
        }
        catch (Exception ex)
        {
            _hasError = true;
            HandleError?.ProcessError(ex);
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
