using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace Craft.UiBuilders.Services.UserPreference;

public class UserPreferencesManager(ProtectedLocalStorage protectedLocalStorage, ILogger<UserPreferencesManager> logger) : IUserPreferencesManager
{
    private const string UserPreferencesKey = "_USER_PREFERENCES_";
    private readonly ProtectedLocalStorage _protectedLocalStorage = protectedLocalStorage;
    private readonly ILogger<UserPreferencesManager> _logger = logger;

    public async Task<string> GetThemeNameAsync()
    {
        try
        {
            var userPreferences = await GetUserPreferences();
            return userPreferences?.ThemeName ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve theme name from user preferences");
            return string.Empty;
        }
    }

    public async Task<UserPreferences?> GetUserPreferences()
    {
        try
        {
            var result = await _protectedLocalStorage
                .GetAsync<UserPreferences>(UserPreferencesKey);

            if (result.Success && result.Value != null)
                return result.Value;

            _logger.LogDebug("No existing user preferences found, returning defaults");
            return new UserPreferences();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user preferences from protected storage");
            return new UserPreferences();
        }
    }

    public async Task<string> SetThemeNameAsync(string themeName)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(themeName);

            var userPreferences = await GetUserPreferences() ?? new UserPreferences();
            userPreferences.ThemeName = themeName;
            await SetUserPreferences(userPreferences);

            _logger.LogInformation("Theme name updated to: {ThemeName}", themeName);
            return userPreferences.ThemeName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set theme name: {ThemeName}", themeName);
            throw;
        }
    }

    public async Task SetUserPreferences(UserPreferences userPreferences)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(userPreferences);
            await _protectedLocalStorage.SetAsync(UserPreferencesKey, userPreferences);
            _logger.LogDebug("User preferences saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save user preferences to protected storage");
            throw;
        }
    }

    public async Task<bool> ToggleDarkModeAsync()
    {
        try
        {
            var userPreferences = await GetUserPreferences() ?? new UserPreferences();
            userPreferences.IsDarkMode = !userPreferences.IsDarkMode;
            await SetUserPreferences(userPreferences);

            _logger.LogInformation("Dark mode toggled to: {IsDarkMode}", userPreferences.IsDarkMode);
            return userPreferences.IsDarkMode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle dark mode");
            throw;
        }
    }

    public async Task<bool> ToggleDrawerStateAsync()
    {
        try
        {
            var userPreferences = await GetUserPreferences() ?? new UserPreferences();
            userPreferences.IsDrawerOpen = !userPreferences.IsDrawerOpen;
            await SetUserPreferences(userPreferences);

            _logger.LogInformation("Drawer state toggled to: {IsDrawerOpen}", userPreferences.IsDrawerOpen);
            return userPreferences.IsDrawerOpen;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle drawer state");
            throw;
        }
    }
}
