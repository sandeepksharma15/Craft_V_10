using System.ComponentModel.DataAnnotations;

namespace Craft.Security;

/// <summary>
/// Represents the data required to confirm a user's email address.
/// </summary>
public class EmailConfirmationRequest : IEmailConfirmationRequest
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [Required]
    public string? Token { get; set; }
}
