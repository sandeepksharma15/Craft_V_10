namespace Craft.Security.Tests.Models;

public class LoginHistoryTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var history = new LoginHistory<Guid>();
        Assert.Equal(default, history.Id);
        Assert.False(history.IsDeleted);
        Assert.Null(history.LastIpAddress);
        Assert.Null(history.LastLoginOn);
        Assert.Null(history.Provider);
        Assert.Equal(default, history.UserId);
    }

    [Fact]
    public void ParameterizedConstructor_SetsProperties()
    {
        var dt = new DateTime(2024, 1, 1, 12, 0, 0);
        var history = new LoginHistory<Guid>("1.2.3.4", dt, Guid.NewGuid());
        Assert.Equal("1.2.3.4", history.LastIpAddress);
        Assert.Equal(dt, history.LastLoginOn);
        Assert.NotEqual(default, history.UserId);
    }

    [Fact]
    public void CanSetAndGet_Id()
    {
        var history = new LoginHistory<Guid>();
        var id = Guid.NewGuid();
        history.Id = id;
        Assert.Equal(id, history.Id);
    }

    [Fact]
    public void CanSetAndGet_IsDeleted()
    {
        var history = new LoginHistory<Guid>
        {
            IsDeleted = true
        };
        Assert.True(history.IsDeleted);
        history.IsDeleted = false;
        Assert.False(history.IsDeleted);
    }

    [Fact]
    public void CanSetAndGet_LastIpAddress()
    {
        var history = new LoginHistory<Guid>
        {
            LastIpAddress = "127.0.0.1"
        };
        Assert.Equal("127.0.0.1", history.LastIpAddress);
    }

    [Fact]
    public void CanSetAndGet_LastLoginOn()
    {
        var history = new LoginHistory<Guid>();
        var dt = DateTime.UtcNow;
        history.LastLoginOn = dt;
        Assert.Equal(dt, history.LastLoginOn);
    }

    [Fact]
    public void CanSetAndGet_Provider()
    {
        var history = new LoginHistory<Guid>
        {
            Provider = "Google"
        };
        Assert.Equal("Google", history.Provider);
    }

    [Fact]
    public void CanSetAndGet_UserId()
    {
        var history = new LoginHistory<Guid>();
        var userId = Guid.NewGuid();
        history.UserId = userId;
        Assert.Equal(userId, history.UserId);
    }

    [Fact]
    public void NonGenericLoginHistory_Defaults()
    {
        var history = new LoginHistory();
        Assert.Equal(default, history.Id);
        Assert.False(history.IsDeleted);
        Assert.Null(history.LastIpAddress);
        Assert.Null(history.LastLoginOn);
        Assert.Null(history.Provider);
        Assert.Equal(default, history.UserId);
    }
}
