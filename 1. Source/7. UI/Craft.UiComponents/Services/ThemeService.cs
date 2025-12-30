using Craft.UiComponents.Enums;
using Microsoft.JSInterop;

namespace Craft.UiComponents.Services;

/// <summary>
/// Default implementation of <see cref="IThemeService"/> that manages theme state
/// and persists preferences using browser local storage.
/// </summary>
public class ThemeService : IThemeService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    private Theme _currentTheme = Theme.System;
    private bool _systemPrefersDark;
    private bool _isInitialized;

    /// <inheritdoc />
    public Theme CurrentTheme => _currentTheme;

    /// <inheritdoc />
    public bool IsDarkMode => _currentTheme switch
    {
        Theme.Dark => true,
        Theme.Light => false,
        Theme.System => _systemPrefersDark,
        _ => false
    };

    /// <inheritdoc />
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// Initializes a new instance of <see cref="ThemeService"/>.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime.</param>
    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
            _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Craft.UiComponents/js/craft-theme.js").AsTask());
    }

    /// <summary>
    /// Initializes the theme service by loading saved preferences.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        try
        {
            var module = await _moduleTask.Value;

            var savedTheme = await module.InvokeAsync<string?>("getStoredTheme");

            if (Enum.TryParse<Theme>(savedTheme, true, out var theme))
                _currentTheme = theme;

            _systemPrefersDark = await module.InvokeAsync<bool>("getSystemPreference");
            _isInitialized = true;

            await ApplyThemeAsync();
        }
        catch
        {
            // JS interop might fail during prerendering
            _isInitialized = true;
        }
    }

    /// <inheritdoc />
    public async Task SetThemeAsync(Theme theme)
    {
        if (_currentTheme == theme)
            return;

        var previousTheme = _currentTheme;
        _currentTheme = theme;

        await ApplyThemeAsync();
        await SaveThemeAsync();

        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(previousTheme, theme));
    }

    /// <inheritdoc />
    public async Task ToggleThemeAsync()
    {
        var newTheme = IsDarkMode ? Theme.Light : Theme.Dark;
        await SetThemeAsync(newTheme);
    }

    private async Task ApplyThemeAsync()
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("applyTheme", IsDarkMode ? "dark" : "light");
        }
        catch
        {
            // JS interop might fail during prerendering
        }
    }

    private async Task SaveThemeAsync()
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("storeTheme", _currentTheme.ToString());
        }
        catch
        {
            // JS interop might fail during prerendering
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}
