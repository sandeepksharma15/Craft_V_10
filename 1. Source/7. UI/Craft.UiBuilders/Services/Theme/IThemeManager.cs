using MudBlazor;

namespace Craft.UiBuilders.Services.Theme;

/// <summary>
/// Interface for managing MudBlazor themes in the application.
/// </summary>
public interface IThemeManager
{
    public const string DefaultThemeName = "Default";

    /// <summary>
    /// Gets the currently active theme.
    /// </summary>
    MudTheme CurrentTheme { get; }

    /// <summary>
    /// Gets all available themes.
    /// </summary>
    IReadOnlyDictionary<string, MudTheme> AvailableThemes { get; }

    /// <summary>
    /// Gets or sets whether dark mode is enabled.
    /// </summary>
    bool IsDarkMode { get; set; }

    /// <summary>
    /// Sets the active theme by name.
    /// </summary>
    /// <param name="themeName">The name of the theme to activate.</param>
    /// <returns>True if the theme was successfully set, false otherwise.</returns>
    bool SetTheme(string themeName);

    /// <summary>
    /// Toggles between light and dark mode.
    /// </summary>
    void ToggleDarkMode();

    /// <summary>
    /// Event raised when the theme changes.
    /// </summary>
    event Action? OnThemeChanged;

    /// <summary>
    /// Event raised when dark mode is toggled.
    /// </summary>
    event Action<bool>? OnDarkModeChanged;
}
