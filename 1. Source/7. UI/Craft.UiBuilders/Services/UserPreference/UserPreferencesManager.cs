using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Craft.UiBuilders.Services.UserPreference;

public class UserPreferencesManager(ProtectedLocalStorage protectedLocalStorage) : IUserPreferencesManager
{
    private const string UserPreferencesKey = "_USER_PREFERENCES_";
    private readonly ProtectedLocalStorage _protectedLocalStorage = protectedLocalStorage;

    public async Task<string> GetThemeNameAsync()
    {
        var userPreferences = await GetUserPreferences();

        return userPreferences?.ThemeName ?? string.Empty;
    }

    public async Task<UserPreferences?> GetUserPreferences()
    {
        var result = await _protectedLocalStorage
            .GetAsync<UserPreferences>(UserPreferencesKey);

        return result.Success ? result.Value : new UserPreferences();
    }

    public async Task<string> SetThemeNameAsync(string themeName)
    {
        var userPreferences = await GetUserPreferences();

        userPreferences.ThemeName = themeName;
        await SetUserPreferences(userPreferences);
        return userPreferences.ThemeName;
    }

    public async Task SetUserPreferences(UserPreferences userPreferences)
        => await _protectedLocalStorage.SetAsync(UserPreferencesKey, userPreferences);

    public async Task<bool> ToggleDarkModeAsync()
    {
        var userPreferences = await GetUserPreferences();

        if (userPreferences != null)
        {
            userPreferences.IsDarkMode = !userPreferences.IsDarkMode;
            await SetUserPreferences(userPreferences);
            return !userPreferences.IsDarkMode;
        }

        return false;
    }

    public async Task<bool> ToggleDrawerStateAsync()
    {
        var userPreferences = await GetUserPreferences();

        if (userPreferences != null)
        {
            userPreferences.IsDrawerOpen = !userPreferences.IsDrawerOpen;
            await SetUserPreferences(userPreferences);
            return !userPreferences.IsDrawerOpen;
        }

        return false;
    }
}
