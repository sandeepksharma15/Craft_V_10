namespace Craft.Security.Tests.Models;

public class PasswordForgotRequestTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var req = new PasswordForgotRequest();
        Assert.Null(req.ClientURI);
        Assert.Null(req.Email);
    }

    [Fact]
    public void CanSetAndGet_ClientURI()
    {
        var req = new PasswordForgotRequest
        {
            ClientURI = "https://client.example.com/reset"
        };
        Assert.Equal("https://client.example.com/reset", req.ClientURI);
    }

    [Fact]
    public void CanSetAndGet_Email()
    {
        var req = new PasswordForgotRequest
        {
            Email = "user@example.com"
        };
        Assert.Equal("user@example.com", req.Email);
    }
}
