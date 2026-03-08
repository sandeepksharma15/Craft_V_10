using System.ComponentModel.DataAnnotations;

namespace Craft.Security.Tests.Blazor;

public class JwtCookieAuthenticationOptionsTests
{
    [Fact]
    public void DefaultValues_Should_BeSetCorrectly()
    {
        // Arrange & Act
        var options = new JwtCookieAuthenticationOptions();

        // Assert
        Assert.True(options.HttpOnly);
        Assert.Equal("BearerToken", options.CookieName);
        Assert.Equal("/login", options.LoginPath.Value);
        Assert.Equal("/access-denied", options.AccessDeniedPath.Value);
        Assert.Equal("/logout", options.LogoutPath.Value);
        Assert.Equal(60, options.ExpireTimeSpan);
        Assert.True(options.SlidingExpiration);
    }

    [Fact]
    public void Validate_Should_Pass_WithValidSettings()
    {
        // Arrange
        var options = new JwtCookieAuthenticationOptions
        {
            CookieName = "BearerToken",
            LoginPath = "/login",
            AccessDeniedPath = "/access-denied",
            LogoutPath = "/logout",
            ExpireTimeSpan = 60
        };

        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_Should_Fail_WhenCookieNameIsEmpty()
    {
        // Arrange
        var options = new JwtCookieAuthenticationOptions
        {
            CookieName = string.Empty,
            ExpireTimeSpan = 60
        };

        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains(nameof(JwtCookieAuthenticationOptions.CookieName), results[0].MemberNames);
    }

    [Fact]
    public void Validate_Should_Fail_WhenExpireTimeSpanIsZero()
    {
        // Arrange
        var options = new JwtCookieAuthenticationOptions
        {
            ExpireTimeSpan = 0
        };

        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains(nameof(JwtCookieAuthenticationOptions.ExpireTimeSpan), results[0].MemberNames);
    }

    [Fact]
    public void Validate_Should_Fail_WhenPathsAreMissing()
    {
        // Arrange
        var options = new JwtCookieAuthenticationOptions
        {
            LoginPath = new Microsoft.AspNetCore.Http.PathString(),
            AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString(),
            LogoutPath = new Microsoft.AspNetCore.Http.PathString(),
            ExpireTimeSpan = 60
        };

        var context = new ValidationContext(options);

        // Act
        var results = options.Validate(context).ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(JwtCookieAuthenticationOptions.LoginPath)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(JwtCookieAuthenticationOptions.AccessDeniedPath)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(JwtCookieAuthenticationOptions.LogoutPath)));
    }
}
