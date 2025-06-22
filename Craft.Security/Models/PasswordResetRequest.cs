using System.ComponentModel.DataAnnotations;
using Craft.Security.Contracts;

namespace Craft.Security;

public class PasswordResetRequest<TKey> : IPasswordResetRequest<TKey>
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    public TKey Id { get; set; } = default!;

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required]
    public string? Token { get; set; }
}

public class ResetPasswordRequest : PasswordResetRequest<KeyType>, IPasswordResetRequest;
