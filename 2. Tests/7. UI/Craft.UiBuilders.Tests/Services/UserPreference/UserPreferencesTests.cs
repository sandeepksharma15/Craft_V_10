using Craft.UiBuilders.Services.UserPreference;

namespace Craft.UiBuilders.Tests.Services.UserPreference;

public class UserPreferencesTests
{
    #region Property Tests

    [Fact]
    public void IsDarkMode_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var preferences = new UserPreferences();

        // Assert
        Assert.True(preferences.IsDarkMode);
    }

    [Fact]
    public void IsDrawerOpen_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var preferences = new UserPreferences();

        // Assert
        Assert.True(preferences.IsDrawerOpen);
    }

    [Fact]
    public void ThemeName_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var preferences = new UserPreferences();

        // Assert
        Assert.Equal(string.Empty, preferences.ThemeName);
    }

    [Fact]
    public void IsDarkMode_ShouldBeSettable()
    {
        // Arrange
        // Act
        var preferences = new UserPreferences
        {
            IsDarkMode = false
        };

        // Assert
        Assert.False(preferences.IsDarkMode);
    }

    [Fact]
    public void IsDrawerOpen_ShouldBeSettable()
    {
        // Arrange
        // Act
        var preferences = new UserPreferences
        {
            IsDrawerOpen = false
        };

        // Assert
        Assert.False(preferences.IsDrawerOpen);
    }

    [Fact]
    public void ThemeName_ShouldBeSettable()
    {
        // Arrange
        // Act
        var preferences = new UserPreferences
        {
            ThemeName = "CustomTheme"
        };

        // Assert
        Assert.Equal("CustomTheme", preferences.ThemeName);
    }

    #endregion

    #region SetDarkMode Tests

    [Fact]
    public void SetDarkMode_ShouldUpdateProperty()
    {
        // Arrange
        var preferences = new UserPreferences();

        // Act
        preferences.SetDarkMode(false);

        // Assert
        Assert.False(preferences.IsDarkMode);
    }

    [Fact]
    public void SetDarkMode_ShouldRaiseOnDarkModeChangeEvent()
    {
        // Arrange
        var preferences = new UserPreferences();
        var eventRaised = false;
        preferences.OnDarkModeChange += () => eventRaised = true;

        // Act
        preferences.SetDarkMode(false);

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void SetDarkMode_ShouldNotThrow_WhenNoEventSubscribers()
    {
        // Arrange
        var preferences = new UserPreferences();

        // Act & Assert - Should not throw
        preferences.SetDarkMode(false);
    }

    [Fact]
    public void SetDarkMode_ShouldAllowMultipleSubscribers()
    {
        // Arrange
        var preferences = new UserPreferences();
        var subscriber1Called = false;
        var subscriber2Called = false;
        preferences.OnDarkModeChange += () => subscriber1Called = true;
        preferences.OnDarkModeChange += () => subscriber2Called = true;

        // Act
        preferences.SetDarkMode(false);

        // Assert
        Assert.True(subscriber1Called);
        Assert.True(subscriber2Called);
    }

    #endregion

    #region SetDrawerState Tests

    [Fact]
    public void SetDrawerState_ShouldUpdateProperty()
    {
        // Arrange
        var preferences = new UserPreferences();

        // Act
        preferences.SetDrawerState(false);

        // Assert
        Assert.False(preferences.IsDrawerOpen);
    }

    [Fact]
    public void SetDrawerState_ShouldRaiseOnDrawerStateChangeEvent()
    {
        // Arrange
        var preferences = new UserPreferences();
        var eventRaised = false;
        preferences.OnDrawerStateChange += () => eventRaised = true;

        // Act
        preferences.SetDrawerState(false);

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void SetDrawerState_ShouldNotThrow_WhenNoEventSubscribers()
    {
        // Arrange
        var preferences = new UserPreferences();

        // Act & Assert - Should not throw
        preferences.SetDrawerState(false);
    }

    [Fact]
    public void SetDrawerState_ShouldAllowMultipleSubscribers()
    {
        // Arrange
        var preferences = new UserPreferences();
        var subscriber1Called = false;
        var subscriber2Called = false;
        preferences.OnDrawerStateChange += () => subscriber1Called = true;
        preferences.OnDrawerStateChange += () => subscriber2Called = true;

        // Act
        preferences.SetDrawerState(false);

        // Assert
        Assert.True(subscriber1Called);
        Assert.True(subscriber2Called);
    }

    #endregion

    #region SetThemeName Tests

    [Fact]
    public void SetThemeName_ShouldUpdateProperty()
    {
        // Arrange
        var preferences = new UserPreferences();

        // Act
        preferences.SetThemeName("MyTheme");

        // Assert
        Assert.Equal("MyTheme", preferences.ThemeName);
    }

    [Fact]
    public void SetThemeName_ShouldRaiseOnThemeNameChangeEvent()
    {
        // Arrange
        var preferences = new UserPreferences();
        var eventRaised = false;
        preferences.OnThemeNameChange += () => eventRaised = true;

        // Act
        preferences.SetThemeName("MyTheme");

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void SetThemeName_ShouldNotThrow_WhenNoEventSubscribers()
    {
        // Arrange
        var preferences = new UserPreferences();

        // Act & Assert - Should not throw
        preferences.SetThemeName("MyTheme");
    }

    [Fact]
    public void SetThemeName_ShouldAllowMultipleSubscribers()
    {
        // Arrange
        var preferences = new UserPreferences();
        var subscriber1Called = false;
        var subscriber2Called = false;
        preferences.OnThemeNameChange += () => subscriber1Called = true;
        preferences.OnThemeNameChange += () => subscriber2Called = true;

        // Act
        preferences.SetThemeName("MyTheme");

        // Assert
        Assert.True(subscriber1Called);
        Assert.True(subscriber2Called);
    }

    [Fact]
    public void SetThemeName_ShouldAcceptEmptyString()
    {
        // Arrange
        var preferences = new UserPreferences { ThemeName = "Existing" };

        // Act
        preferences.SetThemeName(string.Empty);

        // Assert
        Assert.Equal(string.Empty, preferences.ThemeName);
    }

    [Fact]
    public void SetThemeName_ShouldAcceptNull()
    {
        // Arrange
        var preferences = new UserPreferences { ThemeName = "Existing" };

        // Act
        preferences.SetThemeName(null!);

        // Assert
        Assert.Null(preferences.ThemeName);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void AllSetMethods_ShouldWorkTogether()
    {
        // Arrange
        var preferences = new UserPreferences();
        var darkModeEvents = 0;
        var drawerStateEvents = 0;
        var themeNameEvents = 0;

        preferences.OnDarkModeChange += () => darkModeEvents++;
        preferences.OnDrawerStateChange += () => drawerStateEvents++;
        preferences.OnThemeNameChange += () => themeNameEvents++;

        // Act
        preferences.SetDarkMode(false);
        preferences.SetDrawerState(false);
        preferences.SetThemeName("CustomTheme");

        // Assert
        Assert.False(preferences.IsDarkMode);
        Assert.False(preferences.IsDrawerOpen);
        Assert.Equal("CustomTheme", preferences.ThemeName);
        Assert.Equal(1, darkModeEvents);
        Assert.Equal(1, drawerStateEvents);
        Assert.Equal(1, themeNameEvents);
    }

    [Fact]
    public void DirectPropertySet_ShouldNotRaiseEvents()
    {
        // Arrange
        var preferences = new UserPreferences();
        var darkModeEvents = 0;
        var drawerStateEvents = 0;
        var themeNameEvents = 0;

        preferences.OnDarkModeChange += () => darkModeEvents++;
        preferences.OnDrawerStateChange += () => drawerStateEvents++;
        preferences.OnThemeNameChange += () => themeNameEvents++;

        // Act - Set properties directly instead of using Set methods
        preferences.IsDarkMode = false;
        preferences.IsDrawerOpen = false;
        preferences.ThemeName = "CustomTheme";

        // Assert - Properties updated but events not raised
        Assert.False(preferences.IsDarkMode);
        Assert.False(preferences.IsDrawerOpen);
        Assert.Equal("CustomTheme", preferences.ThemeName);
        Assert.Equal(0, darkModeEvents);
        Assert.Equal(0, drawerStateEvents);
        Assert.Equal(0, themeNameEvents);
    }

    [Fact]
    public void IUserPreferences_ShouldBeImplemented()
    {
        // Arrange & Act
        var preferences = new UserPreferences();

        // Assert
        Assert.IsType<IUserPreferences>(preferences, exactMatch: false);
    }

    [Fact]
    public void SetMethods_ShouldRaiseEvents_EvenWhenValueDoesNotChange()
    {
        // Arrange
        var preferences = new UserPreferences
        {
            IsDarkMode = true,
            IsDrawerOpen = true,
            ThemeName = "Theme"
        };
        var darkModeEvents = 0;
        var drawerStateEvents = 0;
        var themeNameEvents = 0;

        preferences.OnDarkModeChange += () => darkModeEvents++;
        preferences.OnDrawerStateChange += () => drawerStateEvents++;
        preferences.OnThemeNameChange += () => themeNameEvents++;

        // Act - Set same values
        preferences.SetDarkMode(true);
        preferences.SetDrawerState(true);
        preferences.SetThemeName("Theme");

        // Assert - Events still raised
        Assert.Equal(1, darkModeEvents);
        Assert.Equal(1, drawerStateEvents);
        Assert.Equal(1, themeNameEvents);
    }

    [Fact]
    public void MultipleSequentialCalls_ShouldRaiseEventsEachTime()
    {
        // Arrange
        var preferences = new UserPreferences();
        var eventCount = 0;
        preferences.OnDarkModeChange += () => eventCount++;

        // Act
        preferences.SetDarkMode(false);
        preferences.SetDarkMode(true);
        preferences.SetDarkMode(false);

        // Assert
        Assert.Equal(3, eventCount);
    }

    #endregion

    #region Event Unsubscription Tests

    [Fact]
    public void UnsubscribedEvents_ShouldNotBeInvoked()
    {
        // Arrange
        var preferences = new UserPreferences();
        var eventCount = 0;
        void handler() => eventCount++;
        preferences.OnDarkModeChange += handler;
        preferences.OnDarkModeChange -= handler;

        // Act
        preferences.SetDarkMode(false);

        // Assert
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void PartialUnsubscription_ShouldOnlyAffectUnsubscribedHandlers()
    {
        // Arrange
        var preferences = new UserPreferences();
        var handler1Called = false;
        var handler2Called = false;
        void handler1() => handler1Called = true;
        void handler2() => handler2Called = true;

        preferences.OnDarkModeChange += handler1;
        preferences.OnDarkModeChange += handler2;
        preferences.OnDarkModeChange -= handler1;

        // Act
        preferences.SetDarkMode(false);

        // Assert
        Assert.False(handler1Called);
        Assert.True(handler2Called);
    }

    #endregion
}
