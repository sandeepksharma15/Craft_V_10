namespace Craft.Security.Contracts;

public interface IPasswordForgotRequest
{
    string? ClientURI { get; set; }
    string? Email { get; set; }
}