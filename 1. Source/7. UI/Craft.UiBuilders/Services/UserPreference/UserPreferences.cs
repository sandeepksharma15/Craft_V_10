namespace Craft.UiBuilders.Services.UserPreference;

public class UserPreferences : IUserPreferences
{
    public bool IsDarkMode { get; set; } = true;
    public bool IsDrawerOpen { get; set; } = true;
    public string ThemeName { get; set; } = string.Empty;

    public event Action? OnDarkModeChange;
    public event Action? OnDrawerStateChange;
    public event Action? OnThemeNameChange;

    public void SetDarkMode(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
        OnDarkModeChange?.Invoke();
    }

    public void SetDrawerState(bool isDrawerOpen)
    {
        IsDrawerOpen = isDrawerOpen;
        OnDrawerStateChange?.Invoke();
    }

    public void SetThemeName(string themeName)
    {
        ThemeName = themeName;
        OnThemeNameChange?.Invoke();
    }

    public void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        OnDarkModeChange?.Invoke();
    }

    public void ToggleDrawerState()
    {
        IsDrawerOpen = !IsDrawerOpen;
        OnDrawerStateChange?.Invoke();
    }
}
