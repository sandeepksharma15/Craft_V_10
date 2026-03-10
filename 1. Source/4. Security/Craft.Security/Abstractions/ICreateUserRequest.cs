namespace Craft.Security;

/// <summary>Represents the data required to create a new user account.</summary>
public interface ICreateUserRequest
{
    string? Email { get; set; }
    string? FirstName { get; set; }
    string? LastName { get; set; }
    string? Password { get; set; }
}
