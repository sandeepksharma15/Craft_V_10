# Quick Start Guide - User Preferences & Theme Management

## 1. Service Registration

### Option A: Register All Services (Recommended)
```csharp
// Program.cs
builder.Services.AddUiBuilders("MyAppName");
```

### Option B: Register Services Individually
```csharp
// Program.cs
builder.Services.AddUserPreferences("MyAppName");
builder.Services.AddThemeManager();
```

## 2. Basic Usage in Components

### Toggle Dark Mode
```csharp
@inject IUserPreferencesManager PrefsManager

<MudSwitch @bind-Checked="isDarkMode" 
           Color="Color.Primary" 
           Label="Dark Mode"
           CheckedChanged="OnDarkModeToggled" />

@code {
    private bool isDarkMode;
    
    private async Task OnDarkModeToggled(bool value)
    {
        isDarkMode = await PrefsManager.ToggleDarkModeAsync();
    }
}
```

### Theme Selector
```csharp
@inject IThemeManager ThemeManager
@inject IUserPreferencesManager PrefsManager

<MudSelect T="string" 
           Label="Select Theme" 
           Value="@selectedTheme"
           ValueChanged="OnThemeChanged">
    @foreach (var theme in ThemeManager.AvailableThemes.Keys)
    {
        <MudSelectItem Value="@theme">@theme</MudSelectItem>
    }
</MudSelect>

@code {
    private string selectedTheme = "Default";
    
    private async Task OnThemeChanged(string themeName)
    {
        if (ThemeManager.SetTheme(themeName))
        {
            selectedTheme = themeName;
            await PrefsManager.SetThemeNameAsync(themeName);
        }
    }
}
```

## 3. MainLayout Setup

```razor
@inherits LayoutComponentBase
@inject IThemeManager ThemeManager
@inject IUserPreferencesManager PrefsManager

<MudThemeProvider Theme="@ThemeManager.CurrentTheme" 
                  IsDarkMode="@isDarkMode" />
<MudDialogProvider />
<MudSnackbarProvider />

<MudLayout>
    <MudAppBar Elevation="1">
        <MudIconButton Icon="@Icons.Material.Filled.Menu" 
                       Color="Color.Inherit" 
                       OnClick="@ToggleDrawer" />
        <MudText Typo="Typo.h6">My Application</MudText>
        <MudSpacer />
        <MudIconButton Icon="@(isDarkMode ? Icons.Material.Filled.LightMode : Icons.Material.Filled.DarkMode)"
                       Color="Color.Inherit"
                       OnClick="@ToggleDarkMode" />
    </MudAppBar>
    
    <MudDrawer @bind-Open="drawerOpen" Elevation="2">
        <MudDrawerHeader>
            <MudText Typo="Typo.h6">Navigation</MudText>
        </MudDrawerHeader>
        <NavMenu />
    </MudDrawer>
    
    <MudMainContent>
        <MudContainer MaxWidth="MaxWidth.Large" Class="my-4">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>

@code {
    private bool isDarkMode;
    private bool drawerOpen = true;
    
    protected override async Task OnInitializedAsync()
    {
        // Load saved preferences
        var prefs = await PrefsManager.GetUserPreferences();
        
        if (prefs != null)
        {
            isDarkMode = prefs.IsDarkMode;
            drawerOpen = prefs.IsDrawerOpen;
            
            ThemeManager.IsDarkMode = isDarkMode;
            
            if (!string.IsNullOrEmpty(prefs.ThemeName))
            {
                ThemeManager.SetTheme(prefs.ThemeName);
            }
        }
        
        // Subscribe to theme changes
        ThemeManager.OnThemeChanged += StateHasChanged;
        ThemeManager.OnDarkModeChanged += OnThemeManagerDarkModeChanged;
    }
    
    private async Task ToggleDarkMode()
    {
        isDarkMode = await PrefsManager.ToggleDarkModeAsync();
        ThemeManager.IsDarkMode = isDarkMode;
    }
    
    private async Task ToggleDrawer()
    {
        drawerOpen = await PrefsManager.ToggleDrawerStateAsync();
    }
    
    private void OnThemeManagerDarkModeChanged(bool isDark)
    {
        isDarkMode = isDark;
        InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
        ThemeManager.OnThemeChanged -= StateHasChanged;
        ThemeManager.OnDarkModeChanged -= OnThemeManagerDarkModeChanged;
    }
}
```

## 4. Register Custom Themes

### At Startup (Program.cs)
```csharp
builder.Services.AddUiBuilders();
builder.Services.AddMudServices();

// Configure custom themes after registration
var app = builder.Build();

// Or use a configuration class
public static class ThemeConfiguration
{
    public static IServiceCollection ConfigureThemes(this IServiceCollection services)
    {
        services.AddScoped<IThemeManager>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ThemeManager>>();
            var themeManager = new ThemeManager(logger);
            
            // Register Corporate Theme
            themeManager.RegisterTheme("Corporate", new MudTheme
            {
                PaletteLight = new PaletteLight
                {
                    Primary = "#1976D2",
                    Secondary = "#424242",
                    AppbarBackground = "#1976D2",
                },
                PaletteDark = new PaletteDark
                {
                    Primary = "#2196F3",
                    Secondary = "#616161",
                    AppbarBackground = "#1565C0",
                }
            });
            
            // Register Nature Theme
            themeManager.RegisterTheme("Nature", new MudTheme
            {
                PaletteLight = new PaletteLight
                {
                    Primary = "#2E7D32",
                    Secondary = "#558B2F",
                    AppbarBackground = "#2E7D32",
                },
                PaletteDark = new PaletteDark
                {
                    Primary = "#4CAF50",
                    Secondary = "#8BC34A",
                    AppbarBackground = "#1B5E20",
                }
            });
            
            return themeManager;
        });
        
        return services;
    }
}

// In Program.cs:
builder.Services.AddUiBuilders();
builder.Services.ConfigureThemes(); // Override ThemeManager with custom themes
```

## 5. Production Configuration

### Configure Data Protection
```csharp
// Program.cs - For Production
if (builder.Environment.IsProduction())
{
    builder.Services.AddDataProtection()
        .SetApplicationName("MyApp")
        .PersistKeysToFileSystem(new DirectoryInfo("/var/keys"))
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
}
else
{
    // Development uses in-memory keys
    builder.Services.AddUiBuilders("MyApp-Dev");
}
```

## 6. Common Patterns

### Load and Save Pattern
```csharp
// Load preferences on component initialization
protected override async Task OnInitializedAsync()
{
    var prefs = await PrefsManager.GetUserPreferences();
    // Use preferences
}

// Save preference changes
private async Task SavePreference()
{
    var prefs = await PrefsManager.GetUserPreferences();
    prefs.SomeProperty = newValue;
    await PrefsManager.SetUserPreferences(prefs);
}
```

### Event-Driven Updates
```csharp
protected override void OnInitialized()
{
    ThemeManager.OnThemeChanged += HandleThemeChange;
    ThemeManager.OnDarkModeChanged += HandleDarkModeChange;
}

private void HandleThemeChange()
{
    // React to theme changes
    StateHasChanged();
}

private void HandleDarkModeChange(bool isDark)
{
    // React to dark mode changes
    StateHasChanged();
}

public void Dispose()
{
    ThemeManager.OnThemeChanged -= HandleThemeChange;
    ThemeManager.OnDarkModeChanged -= HandleDarkModeChange;
}
```

## 7. Testing

### Mock Services
```csharp
public class UserPreferencesManagerTests
{
    [Fact]
    public async Task ToggleDarkMode_ShouldReturnNewValue()
    {
        // Arrange
        var storage = new Mock<ProtectedLocalStorage>();
        var logger = new Mock<ILogger<UserPreferencesManager>>();
        var manager = new UserPreferencesManager(storage.Object, logger.Object);
        
        // Setup storage to return preferences
        storage.Setup(s => s.GetAsync<UserPreferences>(It.IsAny<string>()))
               .ReturnsAsync(new ProtectedBrowserStorageResult<UserPreferences>
               {
                   Success = true,
                   Value = new UserPreferences { IsDarkMode = false }
               });
        
        // Act
        var result = await manager.ToggleDarkModeAsync();
        
        // Assert
        Assert.True(result);
    }
}
```

## 8. Troubleshooting

### Issue: Preferences not persisting
**Solution**: Ensure DataProtection keys are persisted in production

### Issue: Theme changes not reflecting
**Solution**: Check that `StateHasChanged()` is called after theme updates

### Issue: "Cannot resolve service" error
**Solution**: Ensure `AddUiBuilders()` is called before `Build()`

## Summary

| Service | Purpose | Lifetime |
|---------|---------|----------|
| `IUserPreferencesManager` | Persist user settings | Scoped |
| `IThemeManager` | Manage MudBlazor themes | Scoped |
| `IUserPreferences` | Data model for preferences | Scoped |

**Key Points:**
- ? Use `AddUiBuilders()` for complete setup
- ? Configure DataProtection for production
- ? Subscribe to theme change events
- ? Clean up event subscriptions in `Dispose()`
- ? Test preference persistence
