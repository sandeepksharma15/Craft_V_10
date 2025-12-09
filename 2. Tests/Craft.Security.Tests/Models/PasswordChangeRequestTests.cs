namespace Craft.Security.Tests.Models;

public class PasswordChangeRequestTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var req = new PasswordChangeRequest<Guid>();
        Assert.Null(req.ConfirmNewPassword);
        Assert.Equal(default, req.Id);
        Assert.Null(req.NewPassword);
        Assert.Null(req.Password);
        Assert.Null(req.Token);
    }

    [Fact]
    public void CanSetAndGet_ConfirmNewPassword()
    {
        var req = new PasswordChangeRequest<Guid>
        {
            ConfirmNewPassword = "confirm"
        };
        Assert.Equal("confirm", req.ConfirmNewPassword);
    }

    [Fact]
    public void CanSetAndGet_Id()
    {
        var req = new PasswordChangeRequest<Guid>();
        var id = Guid.NewGuid();
        req.Id = id;
        Assert.Equal(id, req.Id);
    }

    [Fact]
    public void CanSetAndGet_NewPassword()
    {
        var req = new PasswordChangeRequest<Guid>
        {
            NewPassword = "newpass"
        };
        Assert.Equal("newpass", req.NewPassword);
    }

    [Fact]
    public void CanSetAndGet_Password()
    {
        var req = new PasswordChangeRequest<Guid>
        {
            Password = "oldpass"
        };
        Assert.Equal("oldpass", req.Password);
    }

    [Fact]
    public void CanSetAndGet_Token()
    {
        var req = new PasswordChangeRequest<Guid>
        {
            Token = "token123"
        };
        Assert.Equal("token123", req.Token);
    }

    [Fact]
    public void NonGenericPasswordChangeRequest_Defaults()
    {
        var req = new PasswordChangeRequest();
        Assert.Null(req.ConfirmNewPassword);
        Assert.Equal(default, req.Id);
        Assert.Null(req.NewPassword);
        Assert.Null(req.Password);
        Assert.Null(req.Token);
    }
}
