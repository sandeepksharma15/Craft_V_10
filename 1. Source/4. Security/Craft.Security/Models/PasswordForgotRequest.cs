using System.ComponentModel.DataAnnotations;

namespace Craft.Security;

public class PasswordForgotRequest : IPasswordForgotRequest
{
    [Required]
    [Url]
    [DataType(DataType.Url)]
    public string? ClientURI { get; set; }

    [Required]
    [EmailAddress]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
}
