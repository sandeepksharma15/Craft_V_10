using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Craft.UiBuilders.Services.Theme;

/// <summary>
/// Manages MudBlazor themes and provides theme switching functionality.
/// This service should be registered as Scoped to maintain state per user session.
/// </summary>
public class ThemeManager : IThemeManager
{
    private readonly ILogger<ThemeManager> _logger;
    private readonly Dictionary<string, MudTheme> _themes;
    private string _currentThemeName;

    public ThemeManager(ILogger<ThemeManager> logger)
    {
        _logger = logger;
        _themes = new Dictionary<string, MudTheme>(StringComparer.OrdinalIgnoreCase);
        _currentThemeName = "Default";
        
        // Register default theme
        RegisterDefaultTheme();
    }

    public MudTheme CurrentTheme => _themes.TryGetValue(_currentThemeName, out var theme) 
        ? theme 
        : _themes["Default"];

    public IReadOnlyDictionary<string, MudTheme> AvailableThemes => _themes;

    public bool IsDarkMode { get; set; }

    public event Action? OnThemeChanged;
    public event Action<bool>? OnDarkModeChanged;

    public bool SetTheme(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
        {
            _logger.LogWarning("Attempted to set theme with null or empty name");
            return false;
        }

        if (!_themes.ContainsKey(themeName))
        {
            _logger.LogWarning("Theme '{ThemeName}' not found. Available themes: {Themes}", 
                themeName, string.Join(", ", _themes.Keys));
            return false;
        }

        _currentThemeName = themeName;
        _logger.LogInformation("Theme changed to: {ThemeName}", themeName);
        OnThemeChanged?.Invoke();
        return true;
    }

    public void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        _logger.LogInformation("Dark mode toggled to: {IsDarkMode}", IsDarkMode);
        OnDarkModeChanged?.Invoke(IsDarkMode);
    }

    /// <summary>
    /// Registers a custom theme with the theme manager.
    /// </summary>
    /// <param name="name">The unique name for the theme.</param>
    /// <param name="theme">The MudTheme instance.</param>
    /// <returns>True if registration succeeded, false if theme name already exists.</returns>
    public bool RegisterTheme(string name, MudTheme theme)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(theme);

        if (_themes.ContainsKey(name))
        {
            _logger.LogWarning("Theme '{ThemeName}' already registered", name);
            return false;
        }

        _themes[name] = theme;
        _logger.LogInformation("Theme '{ThemeName}' registered successfully", name);
        return true;
    }

    private void RegisterDefaultTheme()
    {
        var defaultTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#594AE2",
                AppbarBackground = "#594AE2",
            },
            PaletteDark = new PaletteDark()
            {
                Primary = "#776BE7",
            }
        };

        _themes["Default"] = defaultTheme;
    }
}
