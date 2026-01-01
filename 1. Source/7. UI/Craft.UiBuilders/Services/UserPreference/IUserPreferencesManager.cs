using MudBlazor;

namespace Craft.UiBuilders.Services.UserPreference;

public interface IUserPreferencesManager
{
    public Task<string> GetThemeNameAsync();
    public Task<string> SetThemeNameAsync(string themeName);   

    public Task<UserPreferences?> GetUserPreferences();
    public Task SetUserPreferences(UserPreferences userPreferences);

    public Task<bool> ToggleDarkModeAsync();
    public Task<bool> ToggleDrawerStateAsync();
}
