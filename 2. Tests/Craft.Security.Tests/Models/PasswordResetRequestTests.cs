namespace Craft.Security.Tests.Models;

public class PasswordResetRequestTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var req = new PasswordResetRequest<Guid>();
        Assert.Null(req.Email);
        Assert.Equal(default, req.Id);
        Assert.Null(req.Password);
        Assert.Null(req.Token);
    }

    [Fact]
    public void CanSetAndGet_Email()
    {
        var req = new PasswordResetRequest<Guid>
        {
            Email = "user@example.com"
        };
        Assert.Equal("user@example.com", req.Email);
    }

    [Fact]
    public void CanSetAndGet_Id()
    {
        var req = new PasswordResetRequest<Guid>();
        var id = Guid.NewGuid();
        req.Id = id;
        Assert.Equal(id, req.Id);
    }

    [Fact]
    public void CanSetAndGet_Password()
    {
        var req = new PasswordResetRequest<Guid>
        {
            Password = "newpass"
        };
        Assert.Equal("newpass", req.Password);
    }

    [Fact]
    public void CanSetAndGet_Token()
    {
        var req = new PasswordResetRequest<Guid>
        {
            Token = "token123"
        };
        Assert.Equal("token123", req.Token);
    }

    [Fact]
    public void NonGenericResetPasswordRequest_Defaults()
    {
        var req = new ResetPasswordRequest();
        Assert.Null(req.Email);
        Assert.Equal(default, req.Id);
        Assert.Null(req.Password);
        Assert.Null(req.Token);
    }
}
