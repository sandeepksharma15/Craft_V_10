namespace Craft.Permissions.Tests.Services;

public class PermissionSessionCacheTests
{
    private static PermissionSessionCache CreateCache() => new();

    [Fact]
    public void HasPermission_ReturnsFalse_WhenCacheIsEmpty()
    {
        var cache = CreateCache();

        Assert.False(cache.HasPermission(1001));
    }

    [Fact]
    public void SetPermissions_PopulatesCache()
    {
        var cache = CreateCache();

        cache.SetPermissions([1001, 1002, 2001]);

        Assert.True(cache.HasPermission(1001));
        Assert.True(cache.HasPermission(1002));
        Assert.True(cache.HasPermission(2001));
    }

    [Fact]
    public void HasPermission_ReturnsFalse_WhenCodeNotInCache()
    {
        var cache = CreateCache();
        cache.SetPermissions([1001, 1002]);

        Assert.False(cache.HasPermission(9999));
    }

    [Fact]
    public void Clear_RemovesAllPermissions()
    {
        var cache = CreateCache();
        cache.SetPermissions([1001, 1002]);

        cache.Clear();

        Assert.False(cache.HasPermission(1001));
        Assert.Empty(cache.GetPermissions());
    }

    [Fact]
    public void SetPermissions_ReplacesExistingCache()
    {
        var cache = CreateCache();
        cache.SetPermissions([1001, 1002]);

        cache.SetPermissions([3001]);

        Assert.False(cache.HasPermission(1001));
        Assert.True(cache.HasPermission(3001));
    }

    [Fact]
    public void HasAnyPermission_ReturnsTrue_WhenAtLeastOneCodeMatches()
    {
        var cache = CreateCache();
        cache.SetPermissions([1001, 1002]);

        Assert.True(cache.HasAnyPermission(9999, 1001));
    }

    [Fact]
    public void HasAnyPermission_ReturnsFalse_WhenNoneMatch()
    {
        var cache = CreateCache();
        cache.SetPermissions([1001, 1002]);

        Assert.False(cache.HasAnyPermission(9998, 9999));
    }

    [Fact]
    public void HasAllPermissions_ReturnsTrue_WhenAllCodesMatch()
    {
        var cache = CreateCache();
        cache.SetPermissions([1001, 1002, 2001]);

        Assert.True(cache.HasAllPermissions(1001, 1002));
    }

    [Fact]
    public void HasAllPermissions_ReturnsFalse_WhenAnyCodeMissing()
    {
        var cache = CreateCache();
        cache.SetPermissions([1001, 1002]);

        Assert.False(cache.HasAllPermissions(1001, 9999));
    }

    [Fact]
    public void GetPermissions_ReturnsSnapshot_OfCurrentCodes()
    {
        var cache = CreateCache();
        cache.SetPermissions([1001, 1002, 2001]);

        var permissions = cache.GetPermissions();

        Assert.Equal(3, permissions.Count);
        Assert.Contains(1001, permissions);
        Assert.Contains(2001, permissions);
    }

    [Fact]
    public void SetPermissions_ThrowsArgumentNullException_WhenNullPassed()
    {
        var cache = CreateCache();

        Assert.Throws<ArgumentNullException>(() => cache.SetPermissions(null!));
    }
}
