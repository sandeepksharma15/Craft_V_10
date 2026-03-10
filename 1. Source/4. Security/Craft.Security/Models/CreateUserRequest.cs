using System.ComponentModel.DataAnnotations;

namespace Craft.Security;

/// <summary>Represents the data required to create a new user account.</summary>
public class CreateUserRequest : ICreateUserRequest
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? LastName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}
