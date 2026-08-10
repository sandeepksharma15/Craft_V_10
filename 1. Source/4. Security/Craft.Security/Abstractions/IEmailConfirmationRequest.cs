namespace Craft.Security;

/// <summary>
/// Represents the data required to confirm a user's email address.
/// </summary>
public interface IEmailConfirmationRequest
{
    string? Email { get; set; }
    string? Token { get; set; }
}
