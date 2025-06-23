namespace Craft.Security.Tests.Models;

public class UserLoginRequestTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var req = new UserLoginRequest();
        Assert.Null(req.Email);
        Assert.Null(req.IpAddress);
        Assert.Null(req.Password);
        Assert.True(req.RememberMe);
    }

    [Fact]
    public void CanSetAndGet_Email()
    {
        var req = new UserLoginRequest
        {
            Email = "user@example.com"
        };
        Assert.Equal("user@example.com", req.Email);
    }

    [Fact]
    public void CanSetAndGet_IpAddress()
    {
        var req = new UserLoginRequest
        {
            IpAddress = "1.2.3.4"
        };
        Assert.Equal("1.2.3.4", req.IpAddress);
    }

    [Fact]
    public void CanSetAndGet_Password()
    {
        var req = new UserLoginRequest
        {
            Password = "pw"
        };
        Assert.Equal("pw", req.Password);
    }

    [Fact]
    public void CanSetAndGet_RememberMe()
    {
        var req = new UserLoginRequest
        {
            RememberMe = false
        };
        Assert.False(req.RememberMe);
        req.RememberMe = true;
        Assert.True(req.RememberMe);
    }
}
