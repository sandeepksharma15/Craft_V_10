namespace Craft.UiBuilders.Services.UserPreference;

public interface IUserPreferences
{
    public bool IsDarkMode { get; set; }
    public bool IsDrawerOpen { get; set; }
    public string ThemeName { get; set; }
}
