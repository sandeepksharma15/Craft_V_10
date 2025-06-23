namespace Craft.Security;

public interface IUserLoginRequest
{
    string? Email { get; set; }
    string? IpAddress { get; set; }
    string? Password { get; set; }
    bool RememberMe { get; set; }
}
