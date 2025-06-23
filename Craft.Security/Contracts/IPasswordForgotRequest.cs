namespace Craft.Security;

public interface IPasswordForgotRequest
{
    string? ClientURI { get; set; }
    string? Email { get; set; }
}
