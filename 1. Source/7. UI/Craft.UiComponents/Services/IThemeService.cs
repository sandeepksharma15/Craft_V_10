using Craft.UiConponents.Enums;

namespace Craft.UiConponents.Services;

/// <summary>
/// Provides theme management capabilities for the UI component library.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the current theme.
    /// </summary>
    Theme CurrentTheme { get; }

    /// <summary>
    /// Gets whether the current effective theme is dark mode.
    /// </summary>
    bool IsDarkMode { get; }

    /// <summary>
    /// Event raised when the theme changes.
    /// </summary>
    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// Sets the current theme.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    Task SetThemeAsync(Theme theme);

    /// <summary>
    /// Toggles between light and dark themes.
    /// </summary>
    Task ToggleThemeAsync();
}

/// <summary>
/// Event arguments for theme change events.
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous theme.
    /// </summary>
    public Theme PreviousTheme { get; }

    /// <summary>
    /// Gets the new theme.
    /// </summary>
    public Theme NewTheme { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ThemeChangedEventArgs"/>.
    /// </summary>
    /// <param name="previousTheme">The previous theme.</param>
    /// <param name="newTheme">The new theme.</param>
    public ThemeChangedEventArgs(Theme previousTheme, Theme newTheme)
    {
        PreviousTheme = previousTheme;
        NewTheme = newTheme;
    }
}
