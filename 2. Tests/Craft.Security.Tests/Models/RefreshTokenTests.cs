namespace Craft.Security.Tests.Models;

public class RefreshTokenTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var token = new RefreshToken<Guid>();
        Assert.Equal(default, token.Id);
        Assert.False(token.IsDeleted);
        Assert.Null(token.Token);
        Assert.Equal(default, token.UserId);
        Assert.Equal(default, token.ExpiryTime);
    }

    [Fact]
    public void ParameterizedConstructor_SetsProperties()
    {
        var dt = new DateTime(2024, 1, 1, 12, 0, 0);
        var userId = Guid.NewGuid();
        var token = new RefreshToken<Guid>("refresh", dt, userId);
        Assert.Equal("refresh", token.Token);
        Assert.Equal(dt, token.ExpiryTime);
        Assert.Equal(userId, token.UserId);
    }

    [Fact]
    public void CanSetAndGet_Id()
    {
        var token = new RefreshToken<Guid>();
        var id = Guid.NewGuid();
        token.Id = id;
        Assert.Equal(id, token.Id);
    }

    [Fact]
    public void CanSetAndGet_IsDeleted()
    {
        var token = new RefreshToken<Guid>
        {
            IsDeleted = true
        };
        Assert.True(token.IsDeleted);
        token.IsDeleted = false;
        Assert.False(token.IsDeleted);
    }

    [Fact]
    public void CanSetAndGet_Token()
    {
        var token = new RefreshToken<Guid>
        {
            Token = "refresh-token"
        };
        Assert.Equal("refresh-token", token.Token);
    }

    [Fact]
    public void CanSetAndGet_ExpiryTime()
    {
        var token = new RefreshToken<Guid>();
        var dt = DateTime.UtcNow;
        token.ExpiryTime = dt;
        Assert.Equal(dt, token.ExpiryTime);
    }

    [Fact]
    public void CanSetAndGet_UserId()
    {
        var token = new RefreshToken<Guid>();
        var userId = Guid.NewGuid();
        token.UserId = userId;
        Assert.Equal(userId, token.UserId);
    }

    [Fact]
    public void NonGenericRefreshToken_Defaults()
    {
        var token = new RefreshToken();
        Assert.Equal(default, token.Id);
        Assert.False(token.IsDeleted);
        Assert.Null(token.Token);
        Assert.Equal(default, token.UserId);
        Assert.Equal(default, token.ExpiryTime);
    }
}
